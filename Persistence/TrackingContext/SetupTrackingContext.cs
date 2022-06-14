﻿using Domain.Repositories;
using EFCore.AutomaticMigrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.TrackingContext
{
    public static class SetupTrackingContext
    {
        public static void AddTrackingContext(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration["Database:TrackingConnectionString"];
           
            if (!string.IsNullOrWhiteSpace(cs))
            {
                services.AddScoped<ITrackingDbContext, TrackingDbContext>();
                services.AddScoped(typeof(IGenericRepository<>), typeof(TrackingDbRepository<>));
               
                services.AddDbContext<TrackingDbContext>(options =>
                {
                    options.UseSqlServer(cs, providerOptions =>
                    {
                        providerOptions.EnableRetryOnFailure();
                    });
                }
              );
            }
        }

        public static async Task MigrateTrackingDbToLatestVersionAsync(this IServiceScopeFactory scopeFactory)
        {
            await using var serviceScope = scopeFactory.CreateAsyncScope();
            await using var trackingDbContext = serviceScope.ServiceProvider.GetRequiredService<TrackingDbContext>();
            await trackingDbContext.MigrateToLatestVersionAsync();
        }
    }
}
