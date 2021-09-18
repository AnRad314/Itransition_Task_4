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
			// todo: use Configuration.GetConnectionString(...)
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer("Server=tcp:itransition-task-4dbserver.database.windows.net,1433;Initial Catalog=Itransition_Task_4_db;Persist Security Info=False;User ID=dbadmin;Password=adminpass0#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

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
					options.ClientId = Configuration["Facebook:ClientId"];
					options.ClientSecret = Configuration["Facebook:ClientSecret"];				
				})
				.AddGoogle(options =>
				{				
					options.ClientId = Configuration["Google:ClientId"];
					options.ClientSecret = Configuration["Google:ClientSecret"];
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
				app.UseExceptionHandler("/Error");
				app.UseHsts();
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
