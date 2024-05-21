using Microsoft.EntityFrameworkCore;
using PersonalSiteBackend.Data;
using RAGSystemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<RagDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RagService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();
    dbContext.Database.Migrate();
    var ragService = scope.ServiceProvider.GetRequiredService<RagService>();
    await ragService.LoadAllDataAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();