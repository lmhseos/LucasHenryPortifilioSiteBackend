﻿using Microsoft.EntityFrameworkCore;
using Microsoft.KernelMemory;
using PersonalSiteBackend.Data;
using RAGSystemAPI.Services;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<RagDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
        
        services.AddSingleton<MemoryServerless>(); 
        services.AddSingleton<RagService>();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
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

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();
            dbContext.Database.Migrate();
            
            var ragService = scope.ServiceProvider.GetRequiredService<RagService>();
            ragService.LoadAllDataAsync().Wait();
            
        }
    }
}