using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement {
    public class Startup {
        private IConfiguration _config;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IConfiguration config) {
            _config = config;
        }
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDbConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation")
                .AddEntityFrameworkStores<AppDbContext>();
            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();
            services.ConfigureApplicationCookie(options => {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            services.AddAuthentication().AddGoogle(options => {
                    options.ClientId = "680425465862-nemusa88v9ieroavuc44u2bderjku57d.apps.googleusercontent.com";
                    options.ClientSecret = "pFqqFuRjJAUrpwXuJKZGyI_h";
                }).AddFacebook(options => {
                    options.ClientId = "216925522696819";
                    options.ClientSecret = "dfcfef71a16efa2d4a4d92d3906b46e5";
                });
            services.AddAuthorization(options => {
                options.AddPolicy("DeleteRolePolicy", policy => {
                    policy.RequireClaim("Delete Role");
                });
                options.AddPolicy("EditRolePolicy", policy => {
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement());
                });
                options.AddPolicy("AdminRolePolicy", policy => {
                    policy.RequireRole("Admin");
                });
            });
            services.Configure<DataProtectionTokenProviderOptions>(o => {
                o.TokenLifespan = TimeSpan.FromHours(5);
            });
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => {
                o.TokenLifespan = TimeSpan.FromDays(3);
            });

            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
