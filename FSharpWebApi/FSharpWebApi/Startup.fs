namespace FSharpWeb1

open System
open System.Collections.Generic
open System.Linq
open Owin
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.Owin
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.Google
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

  // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
  member this.ConfigureAuth(app: IAppBuilder) =
    // Configure the db context and user manager to use a single instance per request
    app.CreatePerOwinContext(ApplicationDbContext.Create) |> ignore    
    //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

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

    // Uncomment the following lines to enable logging in with third party login providers
    //app.UseMicrosoftAccountAuthentication(
    //    clientId: "",
    //    clientSecret: "");

    //app.UseTwitterAuthentication(
    //    consumerKey: "",
    //    consumerSecret: "");

    //app.UseFacebookAuthentication(
    //    appId: "",
    //    appSecret: "");

    //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
    //{
    //    ClientId = "",
    //    ClientSecret = ""
    //});

[<assembly: OwinStartupAttribute(typeof<Startup>)>]
do()