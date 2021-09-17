using Itransition_Task_4.Data;
using Itransition_Task_4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Itransition_Task_4
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("UsersContext")));

			services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddEntityFrameworkStores<ApplicationDbContext>();				
			services.AddAuthentication()
				.AddCookie()
				.AddOAuth("GitHub", options =>
				{
					options.ClientId = Configuration["GitHub:ClientId"];
					options.ClientSecret = Configuration["GitHub:ClientSecret"];
					options.CallbackPath = new PathString("/github-oauth");
					options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
					options.TokenEndpoint = "https://github.com/login/oauth/access_token";
					options.UserInformationEndpoint = "https://api.github.com/user";
					options.SaveTokens = true;
					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
					options.ClaimActions.MapJsonKey("urn:github:login", "login");
					options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
					options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
					options.Events = new OAuthEvents
					{
						OnCreatingTicket = async context =>
						{
							var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
							request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
							var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
							response.EnsureSuccessStatusCode();
							var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
							context.RunClaimActions(json.RootElement);
							context.Success();
						}
					};
				})
			.AddFacebook(options =>
			{
				options.AppId = Configuration["Authentication:Facebook:AppId"];
				options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
				options.AccessDeniedPath = "/Home/Index1";
			})
			.AddGoogle(options =>
			{
				IConfigurationSection googleAuthNSection =
				Configuration.GetSection("Authentication:Google");
				options.ClientId = Configuration["Authentication:Google:ClientId"];
				options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
			})
			.AddMicrosoftAccount(options =>
			{
				options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
				options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
				options.AccessDeniedPath = "/Home/Index1"; 
			});
			
			services.AddControllersWithViews();
		}

		
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();
			
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");				
			});
		}
	}
}
