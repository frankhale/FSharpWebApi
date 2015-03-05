namespace FSharpWeb1.Areas.HelpPage.ModelDescriptions

open System
open System.Globalization
open System.Linq
open System.Reflection

//    internal static class ModelNameHelper
type internal ModelNameHelper() =
  // Modify this to provide custom model name mapping.  
  static member GetModelName(_type:Type) =
    let modelNameAttribute = _type.GetCustomAttribute<ModelNameAttribute>()
  
    match (modelNameAttribute <> null && not (String.IsNullOrEmpty(modelNameAttribute.Name))) with
    | true -> modelNameAttribute.Name
    | false -> 
        let mutable modelName = _type.Name
  
        if _type.IsGenericType then
          // Format the generic type name to something like: GenericOfAgurment1AndArgument2
          let genericType = _type.GetGenericTypeDefinition()
          let genericArguments = _type.GetGenericArguments()
          let mutable genericTypeName = genericType.Name
        
          // Trim the generic parameter counts from the name
          genericTypeName <- genericTypeName.Substring(0, genericTypeName.IndexOf('`'))
          let argumentTypeNames = genericArguments.Select(fun t -> ModelNameHelper.GetModelName(t)).ToArray()
          modelName <- String.Format(CultureInfo.InvariantCulture, "{0}Of{1}", genericTypeName, String.Join("And", argumentTypeNames))
        
        modelName
