using System.Text.Json.Serialization;

namespace Lefty.Schematron;

/// <summary>
/// One line of a validation run, in the order the engine produced it.
/// </summary>
/// <remarks>
/// The derived types are registered for polymorphic JSON, so serializing a
/// <see cref="SchematronOutput" /> keeps each line's kind in a <c>$type</c>
/// discriminator rather than flattening them all together.
/// </remarks>
[JsonDerivedType( typeof( ActivePattern ), nameof( ActivePattern ) )]
[JsonDerivedType( typeof( FailedAssert ), nameof( FailedAssert ) )]
[JsonDerivedType( typeof( FiredRule ), nameof( FiredRule ) )]
[JsonDerivedType( typeof( SuccessfulReport ), nameof( SuccessfulReport ) )]
[JsonDerivedType( typeof( SuppressedRule ), nameof( SuppressedRule ) )]
public interface ISchematronLine
{
}