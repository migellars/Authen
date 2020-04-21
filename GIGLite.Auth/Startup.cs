
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using GIGLite.Auth.Models;
using GIGLite.Auth.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation;
using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace GIGLite.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            //Database.SetInitializer(new DropCreateDatabaseAlways<BlogContext>());
        }
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<GigLiteDbContext>(options =>
            {
                options.UseOpenIddict();
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),sqlServerOptionsAction: sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
                
           
            });
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<GigLiteDbContext>()
                .AddDefaultTokenProviders();

           
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore().UseDbContext<GigLiteDbContext>();

                })
                .AddServer(options =>
                {

                    options.UseMvc();
                    options.EnableTokenEndpoint("/connect/token");
                    options.UseJsonWebTokens();
                    options.AllowPasswordFlow();
                    options.AcceptAnonymousClients();
                    options.DisableHttpsRequirement();
                    options.SetAccessTokenLifetime(new TimeSpan(0, 15, 0));
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess);
                    //options.DisableConfigurationEndpoint();
                    options.AddDevelopmentSigningCertificate();
                    //options.AddEphemeralSigningKey();
                    //options.AllowRefreshTokenFlow();

                    //options.UseReferenceTokens();
                    


                });
                //.AddValidation(options =>
                //{
                //    options.UseReferenceTokens();
                //    // options.AddAudiences("x-aud");
                //});

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                //Authority must be a url. It does not have a default value.
                options.Authority = "http://104.238.100.236:236"; //"this server's url, e.g. http://localhost:5051/ or https://auth.example.com/";
                //options.Authority = "https://localhost:44315"; //"this server's url, e.g. http://localhost:5051/ or https://auth.example.com/";
                //options.Authority = "http://localhost:82"; //"this server's url, e.g. http://localhost:5051/ or https://auth.example.com/";
                //options.Audience = "https://localhost:44315"; //This must be included in ticket creation
                options.RequireHttpsMetadata = false;
                options.IncludeErrorDetails = true; //
                //options.JwtBackChannelHandler = GetHandler();
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = Claims.Subject,
                    RoleClaimType = Claims.Role,
                    //ValidAudience = "https://localhost:44315",
                    ValidateAudience = false,
                  
                };
            });
            
            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Gig Lite Auth",
                    Description = "Development"
                });
            });
            IdentityModelEventSource.ShowPII = true;
            services.Configure<DefaultAdmin>(Configuration.GetSection("DefaultAdmin"));

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "GigAuth");
            });
            
            //app.UseMvcWithDefaultRoute();

        }

        private static HttpClientHandler GetHandler()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.SslProtocols = SslProtocols.Tls12;
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }
    }
}
