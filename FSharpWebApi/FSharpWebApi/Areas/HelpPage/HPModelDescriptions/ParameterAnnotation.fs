namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System

type ParameterAnnotation() =
  member val AnnotationAttribute = Unchecked.defaultof<Attribute > with get, set
  member val Documentation = Unchecked.defaultof<string> with get, set
