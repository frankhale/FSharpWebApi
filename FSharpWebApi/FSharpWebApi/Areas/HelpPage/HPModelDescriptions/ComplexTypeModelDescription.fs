namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System.Collections.ObjectModel

type ComplexTypeModelDescription() =
  inherit ModelDescription()

  member val Properties = new Collection<string>() with get 
