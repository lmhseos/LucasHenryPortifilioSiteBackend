using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalSiteBackend.Data;
using RAGSystemAPI.Services;
using System;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("_myAllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        services.AddControllers();

        // Log the connection string
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Using connection string: {connectionString}");

        // Configure DbContext with connection string
        services.AddDbContext<RagDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<RagService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Ensure the database is created and apply migrations
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();

            var ragService = scope.ServiceProvider.GetRequiredService<RagService>();
            ragService.LoadAllDataAsync().Wait();
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("_myAllowSpecificOrigins");
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
