namespace FSharpWeb1.Areas.HelpPage

open System
open System.Text
open System.Web
open System.Web.Http.Description
open System.Runtime.CompilerServices

[<Extension>]
type ApiDescriptionExtensions() =
  /// <summary>
  /// Generates an URI-friendly ID for the <see cref="ApiDescription"/>. E.g. "Get-Values-id_name" instead of "GetValues/{id}?name={name}"
  /// </summary>
  /// <param name="description">The <see cref="ApiDescription"/>.</param>
  /// <returns>The ID as a string.</returns>
  [<Extension>]
  static member GetFriendlyId(description:ApiDescription) =
    let path = description.RelativePath
    let urlParts = path.Split('?')
    let localPath = urlParts.[0]
    let mutable queryKeyString = null
    
    if urlParts.Length > 1 then
      let query = urlParts.[1]
      let queryKeys = HttpUtility.ParseQueryString(query).AllKeys
      queryKeyString <- String.Join("_", queryKeys)

    let friendlyPath = new StringBuilder()
    friendlyPath.AppendFormat("{0}-{1}",
        description.HttpMethod.Method,
        localPath.Replace("/", "-").Replace("{", String.Empty).Replace("}", String.Empty)) |> ignore
    
    if queryKeyString <> null then
      friendlyPath.AppendFormat("_{0}", queryKeyString.Replace('.', '-')) |> ignore
    
    friendlyPath.ToString()