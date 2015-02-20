namespace FSharpWebApi.Controllers

open System
open System.Collections.Generic
open System.Net.Http
open System.Security.Claims
open System.Security.Cryptography
open System.Threading.Tasks
open System.Web
open System.Web.Http
open System.Web.Http.ModelBinding
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth

open FSharpWebApi.Infrastructure
open FSharpWebApi.Models
open FSharpWebApi.Providers
open FSharpWebApi.Results

open Common.Helpers

[<AllowNullLiteral>]
type private ExternalLoginData() =
  let mutable _loginProvider = Unchecked.defaultof<string>
  let mutable _providerKey = Unchecked.defaultof<string>
  let mutable _userName = Unchecked.defaultof<string>

  member this.LoginProvider
    with get() = _loginProvider
    and set(value) = _loginProvider <- value 
  
  member this.ProviderKey
    with get() = _providerKey
    and set(value) = _providerKey <- value

  member this.UserName
    with get() = _userName
    and set(value) = _userName <- value

  member this.GetClaims() =   
    let claims = new List<Claim>()    
    claims.Add(new Claim(ClaimTypes.NameIdentifier, this.ProviderKey, null, this.LoginProvider))
  
    if this.UserName <> null then
      claims.Add(new Claim(ClaimTypes.Name, this.UserName, null, this.LoginProvider))

    claims
 
  static member FromIdentity(identity:ClaimsIdentity) =
    match identity with
    | null -> null
    | _ -> 
      let providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier)
      match providerKeyClaim, String.IsNullOrEmpty(providerKeyClaim.Issuer), String.IsNullOrEmpty(providerKeyClaim.Value) with
      | null, true, true -> null      
      | _ -> match providerKeyClaim.Issuer with
             | ClaimsIdentity.DefaultIssuer -> null
             | _ -> new ExternalLoginData(LoginProvider = providerKeyClaim.Issuer, ProviderKey = providerKeyClaim.Value, UserName = identity.FindFirstValue(ClaimTypes.Name))

type private RandomOAuthStateGenerator() =
  static member private _random = 
    new RNGCryptoServiceProvider()

  static member Generate(strengthInBits:int) =
    let bitsPerByte = 8

    if (strengthInBits % bitsPerByte <> 0) then
      raise(new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits"))

    let strengthInBytes = strengthInBits / bitsPerByte
    let data : byte [] = Array.zeroCreate strengthInBytes
    RandomOAuthStateGenerator._random.GetBytes(data);
    HttpServerUtility.UrlTokenEncode(data)

[<Authorize>]
[<RoutePrefix("api/Account")>]
type AccountController(userManager:ApplicationUserManager, accessTokenFormat:ISecureDataFormat<AuthenticationTicket>) =
  inherit ApiController()

  let LocalLoginProvider = "Local"
  let mutable _accessTokenFormat = accessTokenFormat
  let mutable _userManager = userManager

  member this.UserManager
    with get() = 
      match _userManager with
      | null -> this.Request.GetOwinContext().GetUserManager<ApplicationUserManager>()
      | _ -> _userManager
    and set(value) = _userManager <- value

  member this.AccessTokenFormat
    with get() = _accessTokenFormat
    and set(value) = _accessTokenFormat <- value
  
  new() = new AccountController()

  member private this.Authentication = 
    this.Request.GetOwinContext().Authentication

  // GET api/Account/UserInfo
  [<HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)>]
  [<Route("UserInfo")>]
  member this.GetUserInfo() =
    let externalLogin = ExternalLoginData.FromIdentity(this.User.Identity :?> ClaimsIdentity)
    let userInfoViewModel = { UserInfoViewModel.Email = this.User.Identity.GetUserName()
                              HasRegistered = (externalLogin = null)
                              LoginProvider = if externalLogin <> null then externalLogin.LoginProvider else null }
  
    userInfoViewModel

  // POST api/Account/Logout
  [<Route("Logout")>]
  member this.Logout() =
    this.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType)
    this.Ok()

  // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
//  [<Route("ManageInfo")>]
//  member this.GetManageInfo(returnUrl:string, generateState:bool) =
//    let user = await(fun () -> this.UserManager.FindByIdAsync(this.User.Identity.GetUserId()))
//
//    match user with
//    | null -> None
//    | _ ->
//      let logins = new List<UserLoginInfoViewModel>()
//      
//      for linkedAccount in user.Logins do
//        logins.Add({ LoginProvider = linkedAccount.LoginProvider; ProviderKey = linkedAccount.ProviderKey })
//
//      if user.PasswordHash <> null then
//        logins.Add({ LoginProvider = LocalLoginProvider; ProviderKey = user.UserName})
//
//      Some({ LocalLoginProvider = LocalLoginProvider; 
//        Email = user.Email; 
//        Logins = logins; 
//        ExternalLoginProviders = this.GetExternalLogins(returnUrl, generateState) })

  // POST api/Account/ChangePassword
  [<Route("ChangePassword")>]
  //public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
  member this.ChangePassword(model:ChangePasswordBindingModel) =
    match this.ModelState.IsValid with
    | false -> this.BadRequest(this.ModelState) :> IHttpActionResult
    | _ ->
        let result = await(fun () -> this.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword))

        match result.Succeeded with
        | false -> this.GetErrorResult(result) :> IHttpActionResult
        | _ -> this.Ok() :> IHttpActionResult

  // POST api/Account/SetPassword
  [<Route("SetPassword")>]
  member this.SetPassword(model:SetPasswordBindingModel) =
    match this.ModelState.IsValid with
    | false -> this.BadRequest(this.ModelState) :> IHttpActionResult
    | _ -> 
      let result = await(fun () -> this.UserManager.AddPasswordAsync(this.User.Identity.GetUserId(), model.NewPassword))
      
      match result.Succeeded with
      | false -> this.GetErrorResult(result) :> IHttpActionResult
      | _ -> this.Ok() :> IHttpActionResult

//	// POST api/Account/AddExternalLogin
//	[Route("AddExternalLogin")]
//	public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
//
//		AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
//
//		if (ticket == null || ticket.Identity == null || (ticket.Properties != null
//			&& ticket.Properties.ExpiresUtc.HasValue
//			&& ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
//		{
//			return BadRequest("External login failure.");
//		}
//
//		ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);
//
//		if (externalData == null)
//		{
//			return BadRequest("The external login is already associated with an account.");
//		}
//
//		IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
//			new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));
//
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		return Ok();
//	}
//
//	// POST api/Account/RemoveLogin
//	[Route("RemoveLogin")]
//	public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		IdentityResult result;
//
//		if (model.LoginProvider == LocalLoginProvider)
//		{
//			result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
//		}
//		else
//		{
//			result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
//				new UserLoginInfo(model.LoginProvider, model.ProviderKey));
//		}
//
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		return Ok();
//	}
//
//	// GET api/Account/ExternalLogin
//	[OverrideAuthentication]
//	[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
//	[AllowAnonymous]
//	[Route("ExternalLogin", Name = "ExternalLogin")]
//	public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
//	{
//		if (error != null)
//		{
//			return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
//		}
//
//		if (!User.Identity.IsAuthenticated)
//		{
//			return new ChallengeResult(provider, this);
//		}
//
//		ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
//
//		if (externalLogin == null)
//		{
//			return InternalServerError();
//		}
//
//		if (externalLogin.LoginProvider != provider)
//		{
//			Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
//			return new ChallengeResult(provider, this);
//		}
//
//		ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
//			externalLogin.ProviderKey));
//
//		bool hasRegistered = user != null;
//
//		if (hasRegistered)
//		{
//			Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
//			
//			 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
//				OAuthDefaults.AuthenticationType);
//			ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
//				CookieAuthenticationDefaults.AuthenticationType);
//
//			AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
//			Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
//		}
//		else
//		{
//			IEnumerable<Claim> claims = externalLogin.GetClaims();
//			ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
//			Authentication.SignIn(identity);
//		}
//
//		return Ok();
//	}
//
//	// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
//	[AllowAnonymous]
//	[Route("ExternalLogins")]
//	public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
//	{
//		IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
//		List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();
//
//		string state;
//
//		if (generateState)
//		{
//			const int strengthInBits = 256;
//			state = RandomOAuthStateGenerator.Generate(strengthInBits);
//		}
//		else
//		{
//			state = null;
//		}
//
//		foreach (AuthenticationDescription description in descriptions)
//		{
//			ExternalLoginViewModel login = new ExternalLoginViewModel
//			{
//				Name = description.Caption,
//				Url = Url.Route("ExternalLogin", new
//				{
//					provider = description.AuthenticationType,
//					response_type = "token",
//					client_id = Startup.PublicClientId,
//					redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
//					state = state
//				}),
//				State = state
//			};
//			logins.Add(login);
//		}
//
//		return logins;
//	}

  // POST api/Account/Register
  [<AllowAnonymous>]
  [<Route("Register")>]
  member this.Register(model:RegisterBindingModel) =
    match this.ModelState.IsValid with
    | false -> this.BadRequest(this.ModelState) :> IHttpActionResult
    | true -> 
      let user = new ApplicationUser(UserName = model.Email, Email = model.Email);   
      let result = await(fun () -> this.UserManager.CreateAsync(user, model.Password))
      
      match result.Succeeded with
      | false -> this.GetErrorResult(result) :> IHttpActionResult
      | true -> this.Ok() :> IHttpActionResult

  // POST api/Account/RegisterExternal
  [<OverrideAuthentication>]
  [<HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)>]
  [<Route("RegisterExternal")>]
  member this.RegisterExternal(model:RegisterExternalBindingModel) =
    match this.ModelState.IsValid with
    | false -> this.BadRequest(this.ModelState) :> IHttpActionResult
    | true ->
        let info = await(fun () -> this.Authentication.GetExternalLoginInfoAsync())
 
        match info with
        | null -> this.InternalServerError() :> IHttpActionResult
        | _ ->
            let user = new ApplicationUser(UserName = model.Email, Email = model.Email)
            let result = await(fun () -> this.UserManager.CreateAsync(user))
            
            match result.Succeeded with
            | false -> this.GetErrorResult(result) :> IHttpActionResult
            | _ ->
                let result = await(fun () -> this.UserManager.AddLoginAsync(user.Id, info.Login))
                
                match result.Succeeded with
                | false -> this.GetErrorResult(result) :> IHttpActionResult
                | _ -> this.Ok() :> IHttpActionResult            

  override this.Dispose(disposing:bool) =
    if (disposing && _userManager <> null) then
      _userManager.Dispose()
      _userManager <- null

    base.Dispose(disposing)

  member private this.GetErrorResult(result:IdentityResult) =
    match result with
    | null -> this.InternalServerError() :> IHttpActionResult
    | _ ->
        match result.Succeeded with
        | true -> null
        | false ->
            if result.Errors <> null then
              result.Errors |> Seq.iter(fun error -> this.ModelState.AddModelError("", error))

            match this.ModelState.IsValid with
            | true -> this.BadRequest() :> IHttpActionResult
            | false -> this.BadRequest(this.ModelState) :> IHttpActionResult