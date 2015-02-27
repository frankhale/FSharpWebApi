namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System
open System.Reflection

type IModelDocumentationProvider =
  abstract GetDocumentation : _member:MemberInfo -> string 
  abstract GetDocumentation : _type:Type -> string