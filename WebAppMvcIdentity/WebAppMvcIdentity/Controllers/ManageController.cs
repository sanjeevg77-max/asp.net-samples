using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebAppMvcIdentity.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace WebAppMvcIdentity.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private SignInManager<ApplicationUser> _signInManager;
        private UserManager<ApplicationUser> _userManager;

        public ManageController()
        {
        }

        public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public SignInManager<ApplicationUser> SignInManager
        {
            get
            {
                // These objects are now typically setup for dependency injection, and if needed outside the class they can be fetched from the service collection.
                return _signInManager /* ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>() */;
            }
            private set
            {
                _signInManager = value;
            }
        }

        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                // These objects are now typically setup for dependency injection, and if needed outside the class they can be fetched from the service collection.
                return _userManager /* ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>() */;
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = _userManager.GetUserId(User);
            var currentUser = await UserManager.FindByIdAsync(userId);
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(currentUser),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(currentUser),
                Logins = await UserManager.GetLoginsAsync(currentUser),
                BrowserRemembered = await SignInManager.IsTwoFactorClientRememberedAsync(currentUser)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), loginProvider, providerKey);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), model.Number);
            // SmsService and IdentityMessage are no longer available in ASP.NET Core Identity.
            // To send SMS, use a custom ISmsSender service registered via dependency injection.
            // if (UserManager.SmsService != null)
            // {
            //     var message = new IdentityMessage
            //     {
            //         Destination = model.Number,
            //         Body = "Your security code is: " + code
            //     };
            //     await UserManager.SmsService.SendAsync(message);
            // }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), true);
            var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), false);
            var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(user);
            var otherLogins = new System.Collections.Generic.List<Microsoft.AspNetCore.Authentication.AuthenticationScheme>(); // AuthenticationManager.GetExternalAuthenticationTypes() is no longer available; populate via IAuthenticationSchemeProvider if needed
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), _userManager.GetUserId(User));
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await SignInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(await UserManager.FindByIdAsync(_userManager.GetUserId(User)), new UserLoginInfo(loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.LoginProvider));
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        // IAuthenticationManager is no longer available in ASP.NET Core. Authentication is handled via HttpContext.AuthenticateAsync() and SignInManager.
        // private IAuthenticationManager AuthenticationManager
        // {
        //     get
        //     {
        //         return HttpContext.GetOwinContext().Authentication;
        //     }
        // }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindByIdAsync(_userManager.GetUserId(User)).GetAwaiter().GetResult();
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindByIdAsync(_userManager.GetUserId(User)).GetAwaiter().GetResult();
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}
