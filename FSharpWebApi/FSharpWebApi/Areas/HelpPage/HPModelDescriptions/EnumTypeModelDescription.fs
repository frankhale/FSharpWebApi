namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System.Collections.Generic
open System.Collections.ObjectModel

type EnumTypeModelDescription() = 
  inherit ModelDescription()

  member val Values = new Collection<EnumValueDescription>() with get