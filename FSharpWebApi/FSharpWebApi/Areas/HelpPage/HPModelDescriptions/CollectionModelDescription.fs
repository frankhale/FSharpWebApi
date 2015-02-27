namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

type CollectionModelDescription() =
  inherit ModelDescription()
    member val ElementDescription = Unchecked.defaultof<ModelDescription> with get, set