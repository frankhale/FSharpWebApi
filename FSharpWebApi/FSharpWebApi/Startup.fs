namespace FSharpWebApi

open System
open System.Collections.Generic
open System.Linq
open Owin
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth

open FSharpWebApi.Providers
open FSharpWebApi.Models
open FSharpWebApi.Infrastructure

type Startup() =
  static let mutable publicClientId = "self"
  static let mutable oauthOptions = Unchecked.defaultof<OAuthAuthorizationServerOptions>

  static member OAuthOptions
    with get() = oauthOptions
    and private set(value) = oauthOptions <- value

  static member PublicClientId
    with get() = publicClientId
    and private set(value) = publicClientId <- value

  member this.Configuration (app : IAppBuilder) =
    this.ConfigureAuth(app)

  // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
  member this.ConfigureAuth(app: IAppBuilder) =
    // Configure the db context and user manager to use a single instance per request
    app.CreatePerOwinContext(ApplicationDbContext.Create) |> ignore    
    
    // Courtesy of: https://social.msdn.microsoft.com/Forums/vstudio/en-US/46be37b6-83ef-411b-a748-96c506713a35/function-type-not-systemfuncstuffs-?forum=fsharpgeneral
    let appUserManagerCreate = new Func<IdentityFactoryOptions<ApplicationUserManager>,IOwinContext,ApplicationUserManager>(
                                fun options context -> ApplicationUserManager.Create (options, context))

    app.CreatePerOwinContext<ApplicationUserManager>(appUserManagerCreate) |> ignore
    
    // Enable the application to use a cookie to store information for the signed in user
    // and to use a cookie to temporarily store information about a user logging in with a third party login provider
    app.UseCookieAuthentication(new CookieAuthenticationOptions()) |> ignore
    app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)

    // Configure the application for OAuth based flow
    let OAuthOptions = new OAuthAuthorizationServerOptions()
    OAuthOptions.TokenEndpointPath <- new PathString("/Token")
    OAuthOptions.Provider <- new ApplicationOAuthProvider(Startup.PublicClientId)
    OAuthOptions.AuthorizeEndpointPath <- new PathString("/api/Account/ExternalLogin")
    OAuthOptions.AccessTokenExpireTimeSpan <- TimeSpan.FromDays(14.0)
    OAuthOptions.AllowInsecureHttp <- true

    // Enable the application to use bearer tokens to authenticate users
    app.UseOAuthBearerTokens(OAuthOptions)

[<assembly: OwinStartup(typeof<Startup>)>]
do()