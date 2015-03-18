namespace FSharpWeb1.Areas.HelpPage

// Uncomment the following to provide samples for PageResult<T>. Must also add the Microsoft.AspNet.WebApi.OData
// package to your project.
////#define Handle_PageResultOfT

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics
open System.Diagnostics.CodeAnalysis
open System.Linq
open System.Net.Http.Headers
open System.Reflection
open System.Web
open System.Web.Http
#if Handle_PageResultOfT
open System.Web.Http.OData
#endif

/// <summary>
/// Use this class to customize the Help Page.
/// For example you can set a custom <see cref="System.Web.Http.Description.IDocumentationProvider"/> to supply the documentation
/// or you can provide the samples for the requests/responses.
/// </summary>

type HelpPageConfig() =
        [<SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "WebApplication1.Areas.HelpPage.TextSample.#ctor(System.String)",
            Justification = "End users may choose to merge this string with existing localized resources.")>]
        [<SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "bsonspec",
            Justification = "Part of a URI.")>]
        static member Register (config:HttpConfiguration) =
            //// Uncomment the following to use the documentation from XML documentation file.
            //config.SetDocumentationProvider(new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml")));

            //// Uncomment the following to use "sample string" as the sample for all actions that have string as the body parameter or return type.
            //// Also, the string arrays will be used for IEnumerable<string>. The sample objects will be serialized into different media type 
            //// formats by the available formatters.
            //config.SetSampleObjects(new Dictionary<Type, object>
            //{
            //    {typeof(string), "sample string"},
            //    {typeof(IEnumerable<string>), new string[]{"sample 1", "sample 2"}}
            //});

            // Extend the following to provide factories for types not handled automatically (those lacking parameterless
            // constructors) or for which you prefer to use non-default property values. Line below provides a fallback
            // since automatic handling will fail and GeneratePageResult handles only a single type.
#if Handle_PageResultOfT
            config.GetHelpPageSampleGenerator().SampleObjectFactories.Add(GeneratePageResult)
#endif

            // Extend the following to use a preset object directly as the sample for all actions that support a media
            // type, regardless of the body parameter or return type. The lines below avoid display of binary content.
            // The BsonMediaTypeFormatter (if available) is not used to serialize the TextSample object.
            
            //FIXME: WHAT THE FUCK? WHere is the SetSampleForMediaType extensions located?
            
            //config.SetSampleForMediaType(
            //    new TextSample("Binary JSON content. See http://bsonspec.org for details."),
            //    new MediaTypeHeaderValue("application/bson"))

            //// Uncomment the following to use "[0]=foo&[1]=bar" directly as the sample for all actions that support form URL encoded format
            //// and have IEnumerable<string> as the body parameter or return type.
            //config.SetSampleForType("[0]=foo&[1]=bar", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), typeof(IEnumerable<string>));

            //// Uncomment the following to use "1234" directly as the request sample for media type "text/plain" on the controller named "Values"
            //// and action named "Put".
            //config.SetSampleRequest("1234", new MediaTypeHeaderValue("text/plain"), "Values", "Put");

            //// Uncomment the following to use the image on "../images/aspNetHome.png" directly as the response sample for media type "image/png"
            //// on the controller named "Values" and action named "Get" with parameter "id".
            //config.SetSampleResponse(new ImageSample("../images/aspNetHome.png"), new MediaTypeHeaderValue("image/png"), "Values", "Get", "id");

            //// Uncomment the following to correct the sample request when the action expects an HttpRequestMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Get" were having string as the body parameter.
            //config.SetActualRequestType(typeof(string), "Values", "Get");

            //// Uncomment the following to correct the sample response when the action returns an HttpResponseMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Post" were returning a string.
            //config.SetActualResponseType(typeof(string), "Values", "Post");

            config // Make the compiler not complain since this function is not finished

#if Handle_PageResultOfT
        //FIXME: There are a couple of problems preventing this from being finished. 
        //       1. How to specify PageResult<> and List<> in the typeof() function calls in F#?
        //       2. HelpPageSampleGenerator is not ported to F# yet

        //static member private GeneratePageResult(sampleGenerator:HelpPageSampleGenerator, _type:Type) =
            //if _type.IsGenericType then
                //let openGenericType = _type.GetGenericTypeDefinition()
                //if openGenericType = typeof(PageResult<>) then                     
                    // Get the T in PageResult<T>
                    l//et typeParameters = _type.GetGenericArguments()
                    //Debug.Assert(typeParameters.Length = 1)

                    // Create an enumeration to pass as the first parameter to the PageResult<T> constuctor
                    //let itemsType = typeof(List<>).MakeGenericType(typeParameters)
                    //let items = sampleGenerator.GetSampleObject(itemsType)

                    // Fill in the other information needed to invoke the PageResult<T> constuctor
                    //let parameterTypes = [ itemsType, typeof(Uri), typeof(?long) ]
                    //let parameters = [ items, null, ObjectGenerator.DefaultCollectionSize :> long ]

                    // Call PageResult(IEnumerable<T> items, Uri nextPageLink, long? count) constructor
                    //let _constructor = _type.GetConstructor(parameterTypes)
                    //_constructor.Invoke(parameters)
                    
        //    null
#endif

            