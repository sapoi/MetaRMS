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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Reflection;
using SharedLibrary.StaticFiles;

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
            // Registration of services
            services.AddSingleton(typeof(IAppInitService), typeof(AppInitService));
            services.AddSingleton(typeof(IAccountService), typeof(AccountService));
            services.AddSingleton(typeof(IDataService), typeof(DataService));
            services.AddSingleton(typeof(IRightsService), typeof(RightsService));
            services.AddSingleton(typeof(IUserService), typeof(UserService));
            services.AddSession();

            // In-memory cache registration, so it can be used in controllers
            services.AddMemoryCache();

            // Database
            var connection = @"Data Source=database.db";
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connection));

            // JWT authentication
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;       
            })
            // .AddCookie(options =>
            // {
            //     options.Cookie.Expiration = TimeSpan.FromHours(5);
            // })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration["TokenAuthentication:Issuer"],
                    ValidAudience = Configuration["TokenAuthentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:SecretKey"])),
                    ClockSkew = TimeSpan.Zero
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

            // Register the Swagger generator for API documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "MetaRMS API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            // With debug configuration the usage of HTTPS protocol depends on the settings
            // of Constants.UseHttps variable, but with release configuration HTTPS
            // protocol is disabled
            #if DEBUG
                // Require HTTPS in every controller
                if (Constants.UseHttps)
                {
                    services.Configure<MvcOptions>(options =>
                    {
                        options.Filters.Add(new RequireHttpsAttribute());
                    });
                }
            #endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Error page settings
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
            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "MetaRMS API V1");
                c.RoutePrefix = "api/swagger";
            });

            // With debug configuration the usage of HTTPS protocol depends on the settings
            // of Constants.UseHttps variable, but with release configuration HTTPS
            // protocol is disabled
            #if DEBUG
                // HTTPS redirect
                if (Constants.UseHttps)
                {
                    var options = new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently);
                }
            #endif

            app.UseMvc();
        }
    }
}
