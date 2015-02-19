namespace FSharpWebApi.Models

open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin

open Common.Helpers

// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
[<AllowNullLiteral>]
type ApplicationUser() =
  inherit IdentityUser()

  member this.GenerateUserIdentityAsync(manager : UserManager<ApplicationUser>, authenticationType : string) =
    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
    await(fun () -> manager.CreateIdentityAsync(this, authenticationType))    

type ApplicationDbContext() =
  inherit IdentityDbContext<ApplicationUser>("DefaultConnection", throwIfV1Schema = false) 

  static member Create() =
    new ApplicationDbContext()