using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GlobalSolutions.Services;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace GlobalSolutions
{
    public class Startup
    {
        protected readonly ILogger<Startup> Logger;
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            Logger = loggerFactory.CreateLogger<Startup>();

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Logger.LogDebug("1");
            var Dsn = Environment.GetEnvironmentVariable("DB_DSN");
            var UId = Environment.GetEnvironmentVariable("DB_UID");
            var pwd = Environment.GetEnvironmentVariable("DB_PWD");

            var odbcconnStr = Configuration["odbcConnection"]; //P21Connection is put up at appsettings.json

            var odbcEnv = odbcconnStr
                .Replace("{DB_DSN}", Dsn)
                .Replace("{DB_UID}", UId)
                .Replace("{DB_PWD}", pwd)
                .Replace("PASS=","PWD=");

            services.AddSingleton<IDbConnectionService>(new DbConnectionService(odbcEnv));
            services.AddSingleton<IProductService, ProductService>();
            services.AddControllers();
            var client = new Client()
            {
                ClientId = this.Configuration.GetSection("Client")["ClientId"],
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret(this.Configuration.GetSection("Client")["ClientSecret"].Sha256())
                },
                AllowedScopes = { "GSSwebapi" },
                AccessTokenLifetime = 86400

            };
            services.AddIdentityServer(options => {
                options.IssuerUri = Configuration["IdentityServer:IssuerUri"];
            })
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.IdentityResources())
                .AddInMemoryClients(new List<Client>() { client })
                .AddInMemoryApiScopes(Config.Apiscope)
                .AddInMemoryApiResources(Config.Apis());

            var requiredHttps = false;
            bool.TryParse(this.Configuration.GetSection("Jwt")["RequiredHttpsMetadata"], out requiredHttps);
            Logger.LogDebug("3");
            services.AddAuthentication()
               .AddJwtBearer(jwt =>
               {
                   jwt.Authority = this.Configuration.GetSection("Jwt")["Authority"];
                   jwt.RequireHttpsMetadata = requiredHttps;
                   jwt.Audience = this.Configuration.GetSection("Jwt")["Audience"];
                   jwt.TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateAudience = false
                   };
               });
            Logger.LogDebug(this.Configuration.GetSection("Jwt")["Audience"]);
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DCKAP GSS Web API", Version = "v1" });
            });
            Logger.LogDebug("2");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            Logger.LogDebug("3");
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DCKAP GSS Web API V1");
                c.RoutePrefix = string.Empty;
            });

        //    // Define redirection rules as a dictionary
        //    var redirectRules = new Dictionary<string, (string Destination, int StatusCode)>
        //    { "/index.html", ("/", 301) },  // Permanent redirect

        //    // Middleware for applying redirection
        //    app.Use(async (context, next) =>
        // removed for SonarQube testing


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
