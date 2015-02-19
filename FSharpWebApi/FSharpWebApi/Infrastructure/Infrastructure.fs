namespace FSharpWebApi.Infrastructure

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.Owin
open Microsoft.Owin.Security
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open System.Threading.Tasks
open System.Web
open System.Web.Mvc

open Common.Helpers

open FSharpWebApi.Models

// Thanks to http://stackoverflow.com/a/5341186/170217 for this!
module Collections = 
  let inline init s =
    let coll = new ^t()
    Seq.iter (fun (k,v) -> (^t : (member Add : 'a * 'b -> unit) coll, k, v)) s
    coll

module Helpers =
  // Thank you Tomas Petricek!!! http://stackoverflow.com/a/8150139/170217
  let (?<-) (viewData:ViewDataDictionary) (name:string) (value:'T) =
    viewData.Add(name, box value)

type ApplicationUserManager(store:IUserStore<ApplicationUser>) =
  inherit UserManager<ApplicationUser>(store)
  
  static member Create(options:IdentityFactoryOptions<ApplicationUserManager>, context:IOwinContext) =  
    let manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()))
    
    // Configure validation logic for usernames
    let userValidator = UserValidator<ApplicationUser> (manager)
    userValidator.AllowOnlyAlphanumericUserNames <- false
    userValidator.RequireUniqueEmail <- true

    manager.UserValidator <- userValidator

    // Configure validation logic for passwords
    let passwordValidator = PasswordValidator()
    passwordValidator.RequiredLength <- 6
    passwordValidator.RequireNonLetterOrDigit <- true
    passwordValidator.RequireDigit <- true
    passwordValidator.RequireLowercase <- true
    passwordValidator.RequireUppercase <- true

    manager.PasswordValidator <- passwordValidator

    let dataProtectionProvider = 
      if options.DataProtectionProvider <> null then
        manager.UserTokenProvider <- 
          new DataProtectorTokenProvider<ApplicationUser>(options.DataProtectionProvider.Create("ASP.NET Identity"))

    manager