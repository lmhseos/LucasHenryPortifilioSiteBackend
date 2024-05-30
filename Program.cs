using Microsoft.EntityFrameworkCore;
using PersonalSiteBackend.Data;
using RAGSystemAPI.Services;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Include AllowCredentials if necessary
        });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<RagDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RagService>();

var app = builder.Build();

// Ensure the database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();
    dbContext.Database.EnsureDeleted(); // Delete the database if it exists
    dbContext.Database.EnsureCreated(); // Create the database and __EFMigrationsHistory table
    dbContext.Database.Migrate(); // Apply migrations
    
    var ragService = scope.ServiceProvider.GetRequiredService<RagService>();
    await ragService.LoadAllDataAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
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

// Enable CORS
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();