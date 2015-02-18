namespace FSharpWebApi.Models

open System
open System.ComponentModel.DataAnnotations
open Newtonsoft.Json

// Models used as parameters to AccountController actions.

[<CLIMutable>]
type AddExternalLoginBindingModel =
  { [<Required>]
    [<Display(Name = "External access token")>]
    ExternalAccessToken : string }

[<CLIMutable>]
type ChangePasswordBindingModel =
  { [<Required>]
    [<DataType(DataType.Password)>]
    [<Display(Name = "Current password")>]
    OldPassword : string

    [<Required>]
    [<StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)>]
    [<DataType(DataType.Password)>]
    [<Display(Name = "New password")>]
    NewPassword : string

    [<DataType(DataType.Password)>]
    [<Display(Name = "Confirm new password")>]
    [<Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")>]
    ConfirmPassword : string }

[<CLIMutable>]
type RegisterBindingModel =
  { [<Required>]
    [<Display(Name = "Email")>]
    Email : string

    [<Required>]
    [<StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)>]
    [<DataType(DataType.Password)>]
    [<Display(Name = "Password")>]
    Password : string

    [<DataType(DataType.Password)>]
    [<Display(Name = "Confirm password")>]
    [<Compare("Password", ErrorMessage = "The password and confirmation password do not match.")>]
    ConfirmPassword : string }

[<CLIMutable>]
type RegisterExternalBindingModel =
  { [<Required>]
    [<Display(Name = "Email")>]
    Email : string }

[<CLIMutable>]
type RemoveLoginBindingModel =
  {
    [<Required>]
    [<Display(Name = "Login provider")>]
    LoginProvider : string

    [<Required>]
    [<Display(Name = "Provider key")>]
    ProviderKey : string }

[<CLIMutable>]
type SetPasswordBindingModel =
  { [<Required>]
    [<StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)>]
    [<DataType(DataType.Password)>]
    [<Display(Name = "New password")>]
    NewPassword : string

    [<DataType(DataType.Password)>]
    [<Display(Name = "Confirm new password")>]
    [<Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")>]
    ConfirmPassword : string }