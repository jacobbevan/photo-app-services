using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using photo_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Amazon.DynamoDBv2;

namespace photo_api
{
    public class Startup
    {
        private ILogger _log;

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _log = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddIdentity<ApplicationUser, IdentityRole>()
            //     .AddDefaultTokenProviders();
            
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "800258054711-f2cg53urd7rdphbn75i05eic3g2mlr0r.apps.googleusercontent.com";
                googleOptions.ClientSecret = "Gg7mptB8qRySDj-96Yp-UVSa";// Configuration["Authentication:Google:ClientSecret"];
            });            

            services.AddCors();
            services.AddOptions();
            services.Configure<BucketOptions>(Configuration.GetSection("Buckets"));
            services.AddMvc();
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.Configure<MvcOptions>(options => 
            {
                //options.Filters.Add(new RequireHttpsAttribute());
            });

            switch(Enum.Parse(typeof(ImageProviderType), Configuration.GetValue("ImageProviderType", "File")))
            {
                case ImageProviderType.File:
                    _log.LogInformation("Creating File based image provider");
                    services.AddFileBasedImageProvider();
                    break;
                case ImageProviderType.AWS:
                    _log.LogInformation("Creating AWS image provider");
                    services.AddAWSBasedImageProvider();
                    break;

            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //var rewriteOptions = new RewriteOptions()
            //    .AddRedirectToHttps();

            app.UseAuthentication();
            //app.UseRewriter();
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
            app.UseMvc();
    
        }
    }
}
