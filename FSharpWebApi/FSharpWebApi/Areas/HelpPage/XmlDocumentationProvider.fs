namespace FSharpWeb1.Areas.HelpPage

open System
open System.Globalization
open System.Linq
open System.Reflection
open System.Web.Http.Controllers
open System.Web.Http.Description
open System.Xml.XPath
open FSharpWeb1.Areas.HelpPage.ModelDescriptions

/// <summary>
/// A custom <see cref="IDocumentationProvider"/> that reads the API documentation from an XML documentation file.
/// </summary>
type XmlDocumentationProvider(documentPath:string) =
  let _documentNavigator = 
    if documentPath = null then
      raise(new ArgumentNullException("documentPath"))

    (new XPathDocument(documentPath)).CreateNavigator()

  let TypeExpression = "/doc/members/member[@name='T:{0}']"
  let MethodExpression = "/doc/members/member[@name='M:{0}']"
  let PropertyExpression = "/doc/members/member[@name='P:{0}']"
  let FieldExpression = "/doc/members/member[@name='F:{0}']"
  let ParameterExpression = "param[@name='{0}']"

  interface IDocumentationProvider with
    member this.GetDocumentation(controllerDescriptor:HttpControllerDescriptor) =
      let typeNode = this.GetTypeNode(controllerDescriptor.ControllerType)
      XmlDocumentationProvider.GetTagValue(typeNode, "summary")
      
    member this.GetDocumentation(actionDescriptor:HttpActionDescriptor) =
      let methodNode = this.GetMethodNode(actionDescriptor)
      XmlDocumentationProvider.GetTagValue(methodNode, "summary")
      
    member this.GetDocumentation(parameterDescriptor:HttpParameterDescriptor) =
      let reflectedParameterDescriptor = parameterDescriptor :?> ReflectedHttpParameterDescriptor
      match reflectedParameterDescriptor <> null with
      | true ->
          let methodNode = this.GetMethodNode(reflectedParameterDescriptor.ActionDescriptor)
          match methodNode <> null with
          | true ->
              let parameterName = reflectedParameterDescriptor.ParameterInfo.Name
              let parameterNode = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression, parameterName))
              if parameterNode <> null then
                parameterNode.Value.Trim();
              else
                null
          | false -> null   
      | false -> null
      
    member this.GetResponseDocumentation(actionDescriptor:HttpActionDescriptor) =
      let methodNode = this.GetMethodNode(actionDescriptor)
      XmlDocumentationProvider.GetTagValue(methodNode, "returns")
  
  interface IModelDocumentationProvider with
    member this.GetDocumentation(_member:MemberInfo) =
      let memberName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", XmlDocumentationProvider.GetTypeName(_member.DeclaringType), _member.Name)
      let expression = if _member.MemberType = MemberTypes.Field then FieldExpression else PropertyExpression
      let selectExpression = String.Format(CultureInfo.InvariantCulture, expression, memberName)
      let propertyNode = _documentNavigator.SelectSingleNode(selectExpression)
      XmlDocumentationProvider.GetTagValue(propertyNode, "summary")

    member this.GetDocumentation(_type:Type) =
      let typeNode = this.GetTypeNode(_type)
      XmlDocumentationProvider.GetTagValue(typeNode, "summary")

  member private this.GetMethodNode(actionDescriptor:HttpActionDescriptor) =
    let reflectedActionDescriptor = actionDescriptor :?> ReflectedHttpActionDescriptor
    match reflectedActionDescriptor <> null with
    | true ->
        let selectExpression = String.Format(CultureInfo.InvariantCulture, MethodExpression, XmlDocumentationProvider.GetMemberName(reflectedActionDescriptor.MethodInfo))
        _documentNavigator.SelectSingleNode(selectExpression)
    | false ->
        null
  
  static member GetMemberName(_method:MethodInfo) : string =
    let mutable name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", XmlDocumentationProvider.GetTypeName(_method.DeclaringType), _method.Name)
    let parameters = _method.GetParameters()
    
    if parameters.Length <> 0 then
      let parameterTypeNames = parameters.Select(fun param -> XmlDocumentationProvider.GetTypeName(param.ParameterType)).ToArray()
      name <- name + String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames))
    
    name
  
  static member private GetTagValue(parentNode:XPathNavigator, tagName:string) =
    match parentNode <> null with
    | true -> 
        let node = parentNode.SelectSingleNode(tagName)
        if node <> null then node.Value.Trim() else null
    | false -> null

  member private this.GetTypeNode(_type:Type) =
    let controllerTypeName = XmlDocumentationProvider.GetTypeName(_type)
    let selectExpression = String.Format(CultureInfo.InvariantCulture, TypeExpression, controllerTypeName)
    _documentNavigator.SelectSingleNode(selectExpression)
  
  static member private GetTypeName(_type:Type) : string =
    let mutable name = _type.FullName
    
    if _type.IsGenericType then
      // Format the generic type name to something like: Generic{System.Int32,System.String}
      let genericType = _type.GetGenericTypeDefinition()
      let genericArguments = _type.GetGenericArguments()
      let mutable genericTypeName = genericType.FullName
      
      // Trim the generic parameter counts from the name
      genericTypeName <- genericTypeName.Substring(0, genericTypeName.IndexOf('`'))
      let argumentTypeNames = genericArguments.Select(fun t -> XmlDocumentationProvider.GetTypeName(t)).ToArray()
      name <- String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, String.Join(",", argumentTypeNames))

    if _type.IsNested then
      // Changing the nested type name from OuterType+InnerType to OuterType.InnerType to match the XML documentation syntax.
      name <- name.Replace("+", ".")

    name