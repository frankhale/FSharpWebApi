namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System

/// <summary>
/// Use this attribute to change the name of the <see cref="ModelDescription"/> generated for a type.
/// </summary>
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct ||| AttributeTargets.Enum, AllowMultiple = false, Inherited = false)>]
type ModelNameAttribute(name:string) =
  inherit Attribute()

  member val Name = name with get
