namespace FSharpWebApi.Providers

open System
open System.Collections.Generic
open System.Linq
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth

open Common.Helpers

open FSharpWebApi.Infrastructure
open FSharpWebApi.Models

type ApplicationOAuthProvider(publicClientId : string) =
  inherit OAuthAuthorizationServerProvider()

  let _publicClientId = 
    if publicClientId = null then
      raise(new ArgumentNullException("publicClientId"))

    publicClientId

  override this.TokenEndpoint(context:OAuthTokenEndpointContext) =
    for KeyValue(k, v) in context.Properties.Dictionary do
      context.AdditionalResponseParameters.Add(k, v)

    Task.FromResult<obj>(null) :> Task

  override this.ValidateClientAuthentication(context:OAuthValidateClientAuthenticationContext) =
    // Resource owner password credentials does not provide a client ID.
    if context.ClientId = null then
      context.Validated() |> ignore

    Task.FromResult<obj>(null) :> Task

  override this.ValidateClientRedirectUri(context:OAuthValidateClientRedirectUriContext) =
    if context.ClientId = _publicClientId then
      let expectedRootUri = new Uri(context.Request.Uri, "/")
    
      if expectedRootUri.AbsoluteUri = context.RedirectUri then
        context.Validated() |> ignore

    Task.FromResult<obj>(null) :> Task

  static member CreateProperties(userName:string) =
    new AuthenticationProperties(dict [ "userName", userName ])

  override this.GrantResourceOwnerCredentials(context:OAuthGrantResourceOwnerCredentialsContext) =
    let userManager = context.OwinContext.GetUserManager<ApplicationUserManager>()
    let user = await(fun () -> userManager.FindAsync(context.UserName, context.Password))

    match user with
    | null -> context.SetError("invalid_grant", "The user name or password is incorrect.")
    | _ -> 
      let oAuthIdentity = user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType)
      let cookiesIdentity = user.GenerateUserIdentityAsync(userManager, CookieAuthenticationDefaults.AuthenticationType)
      let properties = ApplicationOAuthProvider.CreateProperties(user.UserName)
      let ticket = new AuthenticationTicket(oAuthIdentity, properties)
      context.Validated(ticket) |> ignore
      context.Request.Context.Authentication.SignIn(cookiesIdentity)

    Task.FromResult<obj>(null) :> Task