namespace FSharpWebApi.Results

open System
open System.Collections.Generic
open System.Linq
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open System.Web.Http

type ChallengeResult(loginProvider : string, controller : ApiController) =

  let mutable _loginProvider = loginProvider
  let mutable _request = controller.Request

  member this.LoginProvider 
    with get() = _loginProvider
    and set(value) = _loginProvider <- value
  
  member this.Request
    with get() = _request
    and set(value) = _request <- value

  interface IHttpActionResult with
    member this.ExecuteAsync(cancellationToken : CancellationToken) =
      controller.Request.GetOwinContext().Authentication.Challenge(loginProvider)

      let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
      response.RequestMessage <- this.Request
      Task.FromResult(response)
