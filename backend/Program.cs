using SnowflakeBot.API.Data;
using SnowflakeBot.API.Services;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// // Add configuration from environment variables
// builder.Configuration
//     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//     .AddEnvironmentVariables();

// // Bind environment variables to configuration
// builder.Configuration["Slack:SigningSecret"] = Environment.GetEnvironmentVariable("SLACK_SIGNING_SECRET");
// builder.Configuration["Snowflake:Account"] = Environment.GetEnvironmentVariable("SNOWFLAKE_ACCOUNT");
// builder.Configuration["Snowflake:User"] = Environment.GetEnvironmentVariable("SNOWFLAKE_USER");
// builder.Configuration["Snowflake:Password"] = Environment.GetEnvironmentVariable("SNOWFLAKE_PASSWORD");
// builder.Configuration["Snowflake:Database"] = Environment.GetEnvironmentVariable("SNOWFLAKE_DATABASE");
// builder.Configuration["Snowflake:Schema"] = Environment.GetEnvironmentVariable("SNOWFLAKE_SCHEMA");
// builder.Configuration["Snowflake:Warehouse"] = Environment.GetEnvironmentVariable("SNOWFLAKE_WAREHOUSE");

// var authorizedUsers = Environment.GetEnvironmentVariable("AUTHORIZED_SLACK_USER");
// if (!string.IsNullOrEmpty(authorizedUsers))
// {
//     builder.Configuration["AuthorizedSlackUsers:0"] = authorizedUsers;
// }

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register services
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<AuditService>();
builder.Services.AddSingleton<SnowflakeService>();
builder.Services.AddHttpClient<SlackService>();
builder.Services.AddSingleton<SlackSignatureService>();
var app = builder.Build();

// Initialize SQLite DB
app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();