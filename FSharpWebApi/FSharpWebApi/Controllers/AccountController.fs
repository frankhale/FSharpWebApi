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

  //public IList<Claim> GetClaims()
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
//  [<Route("Logout")>]
//  member this.Logout() =
//    Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType)
//    Ok()

//
//	// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
//	[Route("ManageInfo")]
//	public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
//	{
//		IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
//
//		if (user == null)
//		{
//			return null;
//		}
//
//		List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();
//
//		foreach (IdentityUserLogin linkedAccount in user.Logins)
//		{
//			logins.Add(new UserLoginInfoViewModel
//			{
//				LoginProvider = linkedAccount.LoginProvider,
//				ProviderKey = linkedAccount.ProviderKey
//			});
//		}
//
//		if (user.PasswordHash != null)
//		{
//			logins.Add(new UserLoginInfoViewModel
//			{
//				LoginProvider = LocalLoginProvider,
//				ProviderKey = user.UserName,
//			});
//		}
//
//		return new ManageInfoViewModel
//		{
//			LocalLoginProvider = LocalLoginProvider,
//			Email = user.UserName,
//			Logins = logins,
//			ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
//		};
//	}
//
//	// POST api/Account/ChangePassword
//	[Route("ChangePassword")]
//	public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
//			model.NewPassword);
//		
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		return Ok();
//	}
//
//	// POST api/Account/SetPassword
//	[Route("SetPassword")]
//	public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
//
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		return Ok();
//	}
//
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
//
//	// POST api/Account/Register
//	[AllowAnonymous]
//	[Route("Register")]
//	public async Task<IHttpActionResult> Register(RegisterBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
//
//		IdentityResult result = await UserManager.CreateAsync(user, model.Password);
//
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		return Ok();
//	}
//
//	// POST api/Account/RegisterExternal
//	[OverrideAuthentication]
//	[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
//	[Route("RegisterExternal")]
//	public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
//	{
//		if (!ModelState.IsValid)
//		{
//			return BadRequest(ModelState);
//		}
//
//		var info = await Authentication.GetExternalLoginInfoAsync();
//		if (info == null)
//		{
//			return InternalServerError();
//		}
//
//		var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
//
//		IdentityResult result = await UserManager.CreateAsync(user);
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result);
//		}
//
//		result = await UserManager.AddLoginAsync(user.Id, info.Login);
//		if (!result.Succeeded)
//		{
//			return GetErrorResult(result); 
//		}
//		return Ok();
//	}
//
//	protected override void Dispose(bool disposing)
//	{
//		if (disposing && _userManager != null)
//		{
//			_userManager.Dispose();
//			_userManager = null;
//		}
//
//		base.Dispose(disposing);
//	}
//
//	#region Helpers
//
//	private IAuthenticationManager Authentication
//	{
//		get { return Request.GetOwinContext().Authentication; }
//	}
//
//	private IHttpActionResult GetErrorResult(IdentityResult result)
//	{
//		if (result == null)
//		{
//			return InternalServerError();
//		}
//
//		if (!result.Succeeded)
//		{
//			if (result.Errors != null)
//			{
//				foreach (string error in result.Errors)
//				{
//					ModelState.AddModelError("", error);
//				}
//			}
//
//			if (ModelState.IsValid)
//			{
//				// No ModelState errors are available to send, so just return an empty BadRequest.
//				return BadRequest();
//			}
//
//			return BadRequest(ModelState);
//		}
//
//		return null;
//	}
//
