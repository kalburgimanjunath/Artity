﻿namespace Artity.Web
{
    using System;
    using System.Reflection;

    using Artity.Data;
    using Artity.Data.Models;
    using Artity.Data.Seeding;
    using Artity.Services.Mapping;
    using Artity.Web.Areas.Administration.ViewModels.Dashboard;
    using Artity.Web.Extensions;
    using Artity.Web.InputModels.Order;
    using Artity.Web.ViewModels;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection")));

            Account cloudinaryCredentials = new Account(
              this.configuration["Cloudinary:CloudName"],
              this.configuration["Cloudinary:ApiKey"],
              this.configuration["Cloudinary:ApiSecret"]);

            Cloudinary cloudinaryUtility = new Cloudinary(cloudinaryCredentials);

            services.AddSingleton(cloudinaryUtility);

            // services.AddAuthentication().AddFacebook(facebookOptions =>
            // {
            //    facebookOptions.AppId = this.configuration["Authentication:Facebook:AppId"];
            //    facebookOptions.AppSecret = this.configuration["Authentication:Facebook:AppSecret"];
            // });

            services.AddSignalR();

            services.RegisterIdentity();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddRazorPages(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/ArtistRegister");
                });

            services.ConfigureApplicationCookie(options =>
             {
                 options.LoginPath = "/Identity/Account/Login";
                 options.LogoutPath = "/Identity/Account/Logout";
                 options.AccessDeniedPath = "/Identity/Account/AccessDenied";
             });

            services
                .Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.Lax;
                    options.ConsentCookie.Name = ".AspNetCore.ConsentCookie";
                });

            services.AddSingleton(this.configuration);

            // Identity stores
            services.AddTransient<IUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, ApplicationRoleStore>();
            services.AddIdentityCore<ApplicationUser>();

            services.RegisterRepositoryServices();

            services.RegisterCustomServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, ISeeder seeder, ApplicationDbContext context)
        {
            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly, typeof(ArtistOrderCreateInputModel).GetTypeInfo().Assembly, typeof(ApprovedArtistViewModel).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (env.IsDevelopment() || env.IsProduction())
                {
                    dbContext.Database.Migrate();
                }

                new ApplicationDbContextSeeder()
                    .SeedAsync(dbContext, serviceScope.ServiceProvider)
                    .GetAwaiter()
                    .GetResult();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpointsRouting();

            seeder.SeedAsync(context, serviceProvider);
        }
    }
}
