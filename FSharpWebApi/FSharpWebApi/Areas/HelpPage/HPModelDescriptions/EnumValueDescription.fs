namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

type EnumValueDescription() =
  member val Documentation = Unchecked.defaultof<string> with get, set
  member val Name = Unchecked.defaultof<string> with get, set
  member val Value = Unchecked.defaultof<string> with get, set
