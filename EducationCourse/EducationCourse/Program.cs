using EducationCourse.Components;
using EducationCourse.Data.config;
using EducationCourse.Providers.Auth;
using EducationCourse.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EducationCourse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Get config
            builder.Services.Configure<SupabaseSetting>(builder.Configuration.GetSection("SupabaseSetting"));

            builder.Services.AddSingleton<SupabaseService>();


            //register service

            // Authenticated services
            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Student", policy => policy.RequireClaim("role", "student"));
                options.AddPolicy("Teacher", policy => policy.RequireClaim("role", "teacher"));
                options.AddPolicy("Admin", policy => policy.RequireClaim("role", "admin"));
            });


            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
