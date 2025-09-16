using DeviceAPI.Manager.Business.Interfaces;
using DeviceAPI.Manager.Business.Services;
using DeviceAPI.Manager.Data.Interfaces;
using DeviceAPI.Manager.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Device Manager API",
        Version = "v1",
        Description = "Una API para interactuar con un microcontrolador",
    });
});

// allow cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// // Add services to the container.
builder.Services.AddControllers();

// Register dependencies for IDeviceService and IDeviceRepository
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Device Manager API V1");
        c.RoutePrefix = string.Empty;
    });
}

if( !app.Environment.IsEnvironment("Docker")) {
    app.UseHttpsRedirection();
}

app.UseCors();
app.MapControllers();

app.Run();