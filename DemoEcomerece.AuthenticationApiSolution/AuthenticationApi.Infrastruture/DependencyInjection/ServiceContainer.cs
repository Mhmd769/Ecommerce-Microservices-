using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Infrastruture.Data;
using AuthenticationApi.Infrastruture.Repositories;
using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApi.Infrastruture.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services , IConfiguration config)
        {
            //add database connectivity 
            //JWT add authentication scheme
            SharedServiceContainer.AddSharedServices<AuthenticationDBContext>(services, config, config["MySerilog:FileName"]!);
            services.AddScoped<IUser,UserRepository>();
            return services;
        }

        public static IApplicationBuilder UserIntrastructurePolicy(this IApplicationBuilder app)
        {
            //register middelware such as:
            //Global Excepetion : Handel external errors
            //Listen only to api gateway : Block all external outsider call.
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}
