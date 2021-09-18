using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Itransition_Task_4.Data;
using Itransition_Task_4.Models;


namespace Itransition_Task_4.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext _context;       
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ExternalAuthentication(string provider)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }       

        public async Task<IActionResult> ExternalLoginCallback()
        {           
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();			

			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            if (result.Succeeded)
            {
                var currentUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                currentUser.DataLastVisit = DateTime.Now.Date; 
                await _userManager.UpdateAsync(currentUser);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.Claims.ToList()[1].Value, 
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),                    
					DataRegistarion = DateTime.Today.Date,
					LoginUser = info.Principal.FindFirstValue(ClaimTypes.Name) ?? info.Principal.Claims.ToList()[1].Value,
					DataLastVisit = DateTime.Today.Date,
				};
                
                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, info);
                List<Claim> claims = new List<Claim>
                {
                    new Claim("LoginName", user.LoginUser),
                    new Claim("IsBlocked", (!user.LockoutEnabled).ToString())
                };
                await _userManager.AddClaimsAsync(user, claims); 
				await _signInManager.SignInAsync(user, false);               
			}
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
