using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure auth
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder().AddCurrentUserHandler();

// Configure identity
builder.Services.AddIdentityCore<TodoUser>()
                .AddEntityFrameworkStores<TodoDbContext>()
                .AddApiEndpoints();

// Configure the database
var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=.db/Todos.db";
builder.Services.AddSqlite<TodoDbContext>(connectionString);

// State that represents the current user from the database *and* the request
builder.Services.AddCurrentUser();

builder.Services.AddApplicationInsightsTelemetry();

// Configure Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.AddOpenApiSecurity());

// Configure rate limiting
builder.Services.AddRateLimiting();

builder.Services.AddHttpLogging(o =>
{
    if (builder.Environment.IsDevelopment())
    {
        o.CombineLogs = true;
        o.LoggingFields = HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    }
});

var app = builder.Build();

app.UseHttpLogging();
app.UseRateLimiter();

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();

app.Map("/", () => Results.Redirect("/swagger"));

// Configure the APIs
app.MapTodos();
app.MapUsers();

app.Run();
