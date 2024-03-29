
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spend.Data;
using Spend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spend
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
            services.AddControllersWithViews();

            services.AddSingleton(new DatabaseConfig { Name = Configuration["DatabaseName"] });
            services.AddSingleton(new SpendSettings { 
                ReplyToText = Configuration["ReplyToText"],
                ToNumber = Configuration["ToNumber"],
                VonageApiKey = Configuration["VonageApiKey"],
                VonageApiSecret = Configuration["VonageApiSecret"],
                VonageBrandName = Configuration["VonageBrandName"]
            });

            services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();
            services.AddSingleton<ISpendRepository, SpendRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            var dbBoostrap = (IDatabaseBootstrap)serviceProvider.GetService(typeof(IDatabaseBootstrap));
            dbBoostrap.Setup();

        }
    }
}
