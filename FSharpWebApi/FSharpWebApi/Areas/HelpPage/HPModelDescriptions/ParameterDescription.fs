
namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System.Collections.Generic
open System.Collections.ObjectModel

type ParameterDescription() =
  member val Annotations = new Collection<ParameterAnnotation>() 
    with get

  member val Documentation = Unchecked.defaultof<string> with get, set
  member val Name = Unchecked.defaultof<string> with get, set
  member val TypeDescription = Unchecked.defaultof<ModelDescription> with get, set