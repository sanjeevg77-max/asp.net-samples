using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApp6UnitTestsandIdentity.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace WebApp6UnitTestsandIdentity.Controllers
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
            var removeUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var result = await UserManager.RemoveLoginAsync(removeUser, loginProvider, providerKey);
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
            var phoneUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(phoneUser, model.Number);
            // SmsService and IdentityMessage are no longer available in ASP.NET Core Identity.
            // SMS sending should be implemented via a custom service registered with dependency injection.
            // Example: inject an ISmsService and call await _smsService.SendAsync(model.Number, "Your security code is: " + code);
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            var tfUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            await UserManager.SetTwoFactorEnabledAsync(tfUser, true);
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
            var tfUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            await UserManager.SetTwoFactorEnabledAsync(tfUser, false);
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
            var verifyUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(verifyUser, phoneNumber);
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
            var changePhoneUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var result = await UserManager.ChangePhoneNumberAsync(changePhoneUser, model.PhoneNumber, model.Code);
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
            var removePhoneUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var result = await UserManager.SetPhoneNumberAsync(removePhoneUser, null);
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
            var changePwUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var result = await UserManager.ChangePasswordAsync(changePwUser, model.OldPassword, model.NewPassword);
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
                var addPwUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
                var result = await UserManager.AddPasswordAsync(addPwUser, model.NewPassword);
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
            var otherLogins = new System.Collections.Generic.List<Microsoft.AspNetCore.Authentication.AuthenticationScheme>();
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
            var linkUser = await UserManager.FindByIdAsync(_userManager.GetUserId(User));
            var result = await UserManager.AddLoginAsync(linkUser, new UserLoginInfo(loginInfo.LoginProvider, loginInfo.ProviderKey, loginInfo.LoginProvider));
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }



#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        // AuthenticationManager (IAuthenticationManager) is not available in ASP.NET Core.
        // Authentication is now handled via SignInManager and HttpContext directly.

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
