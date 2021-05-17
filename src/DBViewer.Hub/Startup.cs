using DbViewer.Hub.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbViewer.Hub
{
    public class Startup
    {
        private const string CurrentDbScanner_ConfigKey = "CurrentDbScanner";
        private readonly string _scannerTypeString;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _scannerTypeString = Configuration[CurrentDbScanner_ConfigKey];
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DbViewer.Hub", Version = "v1" });
            });
            services.AddLogging(logs=>logs.AddConsole());

            var scannerType = Type.GetType(_scannerTypeString);
            services.AddSingleton(typeof(IDbScanner), scannerType);
            //services.AddSingleton<IDbScanner, IOSSimulatorDbScanner>();//TODO: Use config value to configure which service to inject
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DbViewer.Hub v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
