namespace FSharpWeb1.Areas.HelpPage.Models

open System.Collections.Generic
open System.Collections.ObjectModel
open System.Net.Http.Headers
open System.Web.Http.Description
open FSharpWeb1.Areas.HelpPage.ModelDescriptions

/// <summary>
/// The model that represents an API displayed on the help page.
/// </summary>
//type HelpPageApiModel() =
  
  //static member private GetParameterDescriptions(modelDescription:ModelDescription) =
    //let complexTypeModelDescription = modelDescription :?> ComplexTypeModelDescription
    //if (complexTypeModelDescription != null)
    //{
    //    return complexTypeModelDescription.Properties;
    //}
    //  
    //CollectionModelDescription collectionModelDescription = modelDescription as CollectionModelDescription;
    //if (collectionModelDescription != null)
    //{
    //    complexTypeModelDescription = collectionModelDescription.ElementDescription as ComplexTypeModelDescription;
    //    if (complexTypeModelDescription != null)
    //    {
    //        return complexTypeModelDescription.Properties;
    //    }
    //}
    //  
    //return null;  
  
  /// <summary>
  /// Gets or sets the <see cref="ApiDescription"/> that describes the API.
  /// </summary>
  //member this.ApiDescription : ApiDescription with get, set

  /// <summary>
  /// Gets or sets the <see cref="ParameterDescription"/> collection that describes the URI parameters for the API.
  /// </summary>
  //member this.UriParameters : Collection<ParameterDescription> with get, set

//        /// <summary>
//        /// Gets or sets the documentation for the request.
//        /// </summary>
//        public string RequestDocumentation { get; set; }
//
//        /// <summary>
//        /// Gets or sets the <see cref="ModelDescription"/> that describes the request body.
//        /// </summary>
//        public ModelDescription RequestModelDescription { get; set; }
//
//        /// <summary>
//        /// Gets the request body parameter descriptions.
//        /// </summary>
//        public IList<ParameterDescription> RequestBodyParameters
//        {
//            get
//            {
//                return GetParameterDescriptions(RequestModelDescription);
//            }
//        }
//
//        /// <summary>
//        /// Gets or sets the <see cref="ModelDescription"/> that describes the resource.
//        /// </summary>
//        public ModelDescription ResourceDescription { get; set; }
//
//        /// <summary>
//        /// Gets the resource property descriptions.
//        /// </summary>
//        public IList<ParameterDescription> ResourceProperties
//        {
//            get
//            {
//                return GetParameterDescriptions(ResourceDescription);
//            }
//        }
//
//        /// <summary>
//        /// Gets the sample requests associated with the API.
//        /// </summary>
//        public IDictionary<MediaTypeHeaderValue, object> SampleRequests { get; private set; }
//
//        /// <summary>
//        /// Gets the sample responses associated with the API.
//        /// </summary>
//        public IDictionary<MediaTypeHeaderValue, object> SampleResponses { get; private set; }


  /// <summary>
  /// Initializes a new instance of the <see cref="HelpPageApiModel"/> class.
  /// </summary>
  //public HelpPageApiModel()
  //{
  //    UriParameters = new Collection<ParameterDescription>();
  //    SampleRequests = new Dictionary<MediaTypeHeaderValue, object>();
  //    SampleResponses = new Dictionary<MediaTypeHeaderValue, object>();
  //    ErrorMessages = new Collection<string>();
  //}


//        /// <summary>
//        /// Gets the error messages associated with this model.
//        /// </summary>
//        public Collection<string> ErrorMessages { get; private set; }
//
