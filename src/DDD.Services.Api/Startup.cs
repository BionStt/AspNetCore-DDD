using DDD.Infra.CrossCutting.IoC;
using DDD.Services.Api.Configurations;
using DDD.Services.Api.StartupExtensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DDD.Services.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // ----- Database -----
            services.AddCustomizedDatabase(Configuration);

            // ----- Auth -----
            services.AddCustomizedAuth(Configuration);

            // ----- Http -----
            services.AddCustomizedHttp(Configuration);

            // ----- AutoMapper -----
            services.AddAutoMapperSetup();

            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));

            // .NET Native DI Abstraction
            RegisterServices(services);

            // ----- Swagger UI -----
            services.AddCustomizedSwagger(_env);

            // ----- Health check -----
            services.AddCustomizedHealthCheck(Configuration, _env);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            // ----- Error Handling -----
            app.UseCustomizedErrorHandling(_env);

            // ----- CORS -----
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseRouting();

            // ----- Auth -----
            app.UseCustomizedAuth();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // ----- Health check -----
                HealthCheckExtensions.UseCustomizedHealthCheck(endpoints, _env);
            });

            // ----- Swagger UI -----
            app.UseCustomizedSwagger(_env);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Adding dependencies from another layers (isolated from Presentation)
            NativeInjectorBootStrapper.RegisterServices(services);
        }
    }
}
