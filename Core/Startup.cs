using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

namespace Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Razor web application
            services.AddSingleton(typeof(IAppInitService), typeof(AppInitService));
            services.AddSingleton(typeof(IAccountService), typeof(AccountService));
            services.AddSingleton(typeof(IDataService), typeof(DataService));
            services.AddSingleton(typeof(IRightsService), typeof(RightsService));
            services.AddSingleton(typeof(IUserService), typeof(UserService));

            // services.AddMvc();

            services.AddSession();
            // In-memory cache registration, so it can be used in controllers
            services.AddMemoryCache();
            // services.AddDistributedMemoryCache(); // uz nevim proc to tady bylo
            // services.AddDistributedRedisCache(options =>
            // {
            //     // o.Configuration = Configuration.GetConnectionString("Redis");
            //     options.Configuration = "redis-18644.c100.us-east-1-4.ec2.cloud.redislabs.com:18644,password=3QFe1OMqrPtBtbsOPxFTiTZyojSGod02";
            // });
            #endregion

            #region Server
            var connection = @"Data Source=database.db";
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connection));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;       
                })
                .AddCookie(options =>
                {
                    options.Cookie.Expiration = TimeSpan.FromHours(5);
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["TokenAuthentication:Issuer"],
                        ValidAudience = Configuration["TokenAuthentication:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:SecretKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                    cfg.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context => 
                            {
                                Console.WriteLine("OnAuthenticationFailed: " + 
                                    context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                Console.WriteLine("OnTokenValidated: " + 
                                    context.SecurityToken);
                                return Task.CompletedTask;
                            }
                        };
                });

            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            // services.Configure<MvcOptions>(options =>
            // {
            //     options.Filters.Add(new RequireHttpsAttribute());
            // });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            app.UseSession();

            // // HTTPS
            // var options = new RewriteOptions().AddRedirectToHttps();
            // app.UseRewriter(options);
            
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
