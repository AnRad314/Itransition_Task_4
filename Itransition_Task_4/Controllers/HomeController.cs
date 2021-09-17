using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Itransition_Task_4.Data;
using Itransition_Task_4.Models;
using Itransition_Task_4.ViewModel;

namespace TItransition_Task_4.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context; 
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public IActionResult Index()
		{
			ViewBag.Count = _context.Users.ToList().Count;			
			return View();
		}

		public IActionResult GetData()
		{
			var dataGoogle = _context.UserLogins.Where(x => x.LoginProvider == "Google").Count();
			var dataFacebook = _context.UserLogins.Where(x => x.LoginProvider == "Facebook").Count();
			var dataGitHub = _context.UserLogins.Where(x => x.LoginProvider == "GitHub").Count();
			DiagramViewModel source = new DiagramViewModel();
			source.Facebook = dataFacebook;
			source.Google = dataGoogle;
			source.GitHub = dataGitHub;
			return Json(source);
		}

		[Authorize]
		public IActionResult Privacy()
		{
			var usersData = _context.Users.Join(_context.UserLogins, u => u.Id, i => i.UserId, (us, prov) => new UsersViewModel
			{
				Id = us.Id,
				LoginName = us.LoginUser,
				Provider = prov.LoginProvider,
				DataRegistration = us.DataRegistarion,
				DataLastVisit = us.DataLastVisit,
				Islockedout = us.LockoutEnabled ? "active": "blocked"
			});
			return View(usersData.ToList());
		}
			

		[HttpPost]
		public async Task<string> Delete([FromBody] string[] model)
		{
			foreach(var idUser in model)
			{
				var user = await _userManager.FindByIdAsync(idUser);
				if (user != null )
				{
					var userLoginInfos = await _userManager.GetLoginsAsync(user);
					var userClaims = await _userManager.GetClaimsAsync(user);
					await _userManager.RemoveClaimsAsync(user, userClaims);
					foreach (var info in userLoginInfos)
					{
						await _userManager.RemoveLoginAsync(user, info.LoginProvider, info.ProviderKey);
					}
					await _userManager.DeleteAsync(user);
				}
				
			}
			return await Task.FromResult(Url.Action("Privacy", "Home"));
		}

		public async Task<string> Lock ([FromBody] string[] model)
		{
			foreach (var idUser in model)
			{
				var user = await _userManager.FindByIdAsync(idUser);				
				if (user != null && user.LockoutEnabled)
				{
					user.LockoutEnabled = false;					
					await _userManager.UpdateAsync(user);
					await _signInManager.RefreshSignInAsync(user);
				}				
			}
			return await Task.FromResult(Url.Action("Privacy", "Home"));
		}
		public async Task<string> UnLock([FromBody] string[] model)
		{
			foreach (var idUser in model)
			{				
				var user = await _userManager.FindByIdAsync(idUser);
				if (user != null && !user.LockoutEnabled)
				{
					user.LockoutEnabled = true;
					await _userManager.UpdateAsync(user);
				}
			}
			return await Task.FromResult(Url.Action("Privacy", "Home"));
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
