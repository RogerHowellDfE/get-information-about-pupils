using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Routes;
using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers
{
    [Route(Authentication.AuthenticationController)]
    public class AuthenticationController : Controller
    {
        private readonly AzureAppSettings _azureAppSettings;

        public AuthenticationController(IOptions<AzureAppSettings> azureAppSettings)
        {
            _azureAppSettings = azureAppSettings.Value;
        }

        [AllowAnonymous]
        [HttpGet(Authentication.LoginAction)]
        public IActionResult LoginDsi(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }

            if (User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return new ChallengeResult(new AuthenticationProperties() { RedirectUri = returnUrl });
            }
        }

        [AllowWithoutConsent]
        [AllowAnonymous]
        [HttpGet(Authentication.SignoutAction)]
        public async Task<IActionResult> SignoutDsi()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(_azureAppSettings.DsiRedirectUrlAfterSignout);
        }
    }
}
