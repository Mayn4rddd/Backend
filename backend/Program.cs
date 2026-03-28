using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ ADD THIS (Database connection) with longer command timeout and retries
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // Increase command timeout to 60 seconds (default is 30)
            sqlOptions.CommandTimeout(60);
            // Optional: enable automatic retries for transient failures
            sqlOptions.EnableRetryOnFailure();
        }
    ));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ⚠️ Keep this (we’ll upgrade to JWT later)
app.UseAuthorization();

app.MapControllers();

app.Run();