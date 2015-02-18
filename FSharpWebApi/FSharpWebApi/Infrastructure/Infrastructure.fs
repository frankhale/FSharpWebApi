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
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.Security

open Common.Helpers

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
