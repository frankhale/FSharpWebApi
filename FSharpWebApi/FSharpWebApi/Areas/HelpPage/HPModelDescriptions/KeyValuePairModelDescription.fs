namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

type KeyValuePairModelDescription() =
  inherit ModelDescription()

  member val KeyModelDescription = Unchecked.defaultof<ModelDescription> with get, set
  member val ValueModelDescription = Unchecked.defaultof<ModelDescription> with get, set
