using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using OsepMVC.Models;
using OsepMVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OsepMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add the DbContext service with the connection string
            builder.Services.AddDbContext<OsepDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            // Add Hangfire services.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer();

            // Add the EventResetService
            builder.Services.AddTransient<EventResetService>();

            // Add authentication and authorization services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/User/Login";
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("IsAdmin", "true"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); // Add session middleware

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard(); // Add Hangfire dashboard

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Hangfire job scheduling
            RecurringJob.AddOrUpdate<EventResetService>(service => service.ResetEventsAsync(), Cron.Weekly(DayOfWeek.Sunday, 0, 0));

            app.Run();
        }
    }
}
