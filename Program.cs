using AplicatieMagazinOnline.Models;
using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http;

namespace LibrarieOnline
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<LibrarieOnlineContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("LibrarieDB")));

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // ?? Ad?ug?m suport pentru roluri
                .AddEntityFrameworkStores<LibrarieOnlineContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
            });

            // ?? Ad?ug?m suport pentru sesiune
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ?? Permite acces la HttpContext în controlere
            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
          

            // Register SentimentAnalysisService
            builder.Services.AddHttpClient<SentimentAnalysisService>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var apiKey = configuration["WatsonNLP:ApiKey"];
                var serviceUrl = configuration["WatsonNLP:Url"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(serviceUrl))
                {
                    throw new InvalidOperationException("API key or Service URL is not configured in appsettings.json.");
                }

                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);
            });
            
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await CreateAdminRoleAndUser(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // ?? Activeaz? sesiunea în aplica?ie
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Book}/{action=Index}/{id?}");

            app.MapRazorPages();
            app.Run();
        }

        // ?? Metod? pentru crearea automat? a rolului Admin ?i a unui utilizator administrator
        private static async Task CreateAdminRoleAndUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string adminRole = "Admin";
            string adminEmail = "admin@bookheaven.com";
            string adminPassword = "Admin123!";

            // ?? Cre?m rolul Admin dac? nu exist?
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // ?? Verific?m dac? exist? un utilizator Admin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
        }
    }
}
