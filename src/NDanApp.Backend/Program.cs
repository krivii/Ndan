using Microsoft.EntityFrameworkCore;
using Npgsql;
using NDanApp.Backend.Data;
using NDanApp.Backend.Repositories;
using NDanApp.Backend.Services;
using NDanApp.Backend.Middleware;
using NDanApp.Backend.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL enum mapping
NpgsqlConnection.GlobalTypeMapper.MapEnum<MediaType>("media_type_enum");

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

// Services
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ILikeService, LikeService>();

// HTTP Client
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        
        // Ignore null values in responses
        options.JsonSerializerOptions.DefaultIgnoreCondition = 
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        
        // Use camelCase for JSON properties
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "NDanApp API", 
        Version = "v1",
        Description = "Wedding photo sharing platform API"
    });
    
    // Enable XML comments (optional)
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NDanApp API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();