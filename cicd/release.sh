#!/bin/bash
# ------------------------------------------------------------------------
set -eux pipefail

yell() { echo "$0: $*" >&2; }
die() { yell "$*"; exit 111; }
try() { "$@" || die "cannot $*"; }


#
# Run all commands from the repository root!
# (That's the directory above the current one :)
# ------------------------------------------------------------------------
#
SCRIPT_PATH="${BASH_SOURCE[0]}"
if ([ -h "${SCRIPT_PATH}" ]); then
  while([ -h "${SCRIPT_PATH}" ]); do cd "$(dirname "$SCRIPT_PATH")";
  SCRIPT_PATH=$(readlink "${SCRIPT_PATH}"); done
fi
cd "$(dirname "${SCRIPT_PATH}")" > /dev/null
cd ..


#
# Ensure env
# ------------------------------------------------------------------------
if [ -z ${GITHUB_REF+x} ];      then die "GITHUB_REF is not set"; fi
if [ -z ${GITHUB_TOKEN+x} ];    then die "GITHUB_TOKEN is not set"; fi
if [ -z ${NUGET_APIKEY+x} ];    then die "NUGET_APIKEY is not set"; fi

if [[ ${GITHUB_REF} != refs/tags/v* ]]; then die "Script only works for tags"; fi

export VERSION="${GITHUB_REF##*/v}"
echo ${VERSION}


#
# Build
# ------------------------------------------------------------------------

dotnet clean   -c Release
dotnet restore --packages .nuget
dotnet build   -c Release --no-restore -p:Version=${VERSION}

rm -rf tmp/win-x64
dotnet publish -c Release --runtime=win-x64 --self-contained tools/Lefty.Schematron.Gui/Lefty.Schematron.Gui.csproj -p:Version=${VERSION} -o tmp/win-x64


#
# Publish to nuget.org
# ------------------------------------------------------------------------

mkdir -p nupkg
rm -f nupkg/*.*

dotnet pack    -c Release --no-restore --no-build src/Lefty.Schematron       -o nupkg -p:Version=${VERSION}
# dotnet pack    -c Release --no-restore --no-build tools/Lefty.Schematron.Cli -o nupkg -p:Version=${VERSION}

dotnet nuget push "nupkg/*.nupkg" --api-key ${NUGET_APIKEY} --source=https://api.nuget.org/v3/index.json


#
# Artifacts
# ------------------------------------------------------------------------

mkdir -p artifacts
rm -f artifacts/*.zip

(
    cd  tmp/win-x64
    zip -qr  ../../artifacts/schtronui-win-x64-${VERSION}.zip  .
)


#
# Release, including artifacts
# ------------------------------------------------------------------------

gh release create v${VERSION} --notes="Release v${VERSION}" \
   artifacts/schtronui-win-x64-${VERSION}.zip

# eof