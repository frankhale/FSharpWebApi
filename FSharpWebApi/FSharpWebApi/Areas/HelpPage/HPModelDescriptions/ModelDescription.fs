namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System

/// <summary>
/// Describes a type model.
/// </summary>
[<AbstractClass>]
type ModelDescription() =
  member val Documentation = Unchecked.defaultof<string> with get, set
  member val ModelType = Unchecked.defaultof<Type> with get, set
  member val Name = Unchecked.defaultof<string> with get, set
