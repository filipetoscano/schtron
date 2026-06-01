using System.Text.Json.Serialization;

namespace Lefty.Schematron;

/// <summary />
[JsonDerivedType( typeof( ActivePattern ), nameof( ActivePattern ) )]
[JsonDerivedType( typeof( FailedAssert ), nameof( FailedAssert ) )]
[JsonDerivedType( typeof( FiredRule ), nameof( FiredRule ) )]
[JsonDerivedType( typeof( SuppressedRule ), nameof( SuppressedRule ) )]
public interface ISchematronLine
{
}