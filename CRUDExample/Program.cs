using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;
using Serilog;
using Serilog.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Logging
//builder.Host.ConfigureLogging(logging =>
//{
//    logging.ClearProviders();
//    logging.AddConsole();
//    logging.AddConsole();
//    logging.AddEventLog();
//});

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) //Read from configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); //Read from current app's services and make them available for serilog
});

builder.Services.AddControllersWithViews();

//Add services into IoC container
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

//Add Db context
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False

builder.Services.AddHttpLogging(options => {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpLogging();
//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("info-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

if (builder.Environment.IsDevelopment())
{ 
    app.UseDeveloperExceptionPage();
}

if(!builder.Environment.IsEnvironment("Test"))
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.MapControllers();
app.UseRouting();

app.Run();

public partial class Program { } //make the auto-generated Program accessed programmaticaly
