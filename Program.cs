using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //ValidAudience = builder.Configuration["Jwt:Audience"],
            //ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.SaveToken = true;
    }); */

//builder.Services.AddAuthentication().AddJwtBearer();



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        //ValidateIssuerSigningKey = true,
        //TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["jwtkey"] ?? "")),
        TokenDecryptionKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["jwtkey"] ?? "")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RequireSignedTokens = false,
    };
});

//Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler

// This will make all apis require authentication, not sure if this is a good pattern?
// since there is e.g. the swagger api that does not require authentication?
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//var key = Convert.FromBase64String(builder.Configuration["Jwt:Key"] ?? "");
var key = Convert.FromBase64String(builder.Configuration["jwtkey"] ?? "");
//app.Logger.LogInformation("Key: {key}", key);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

IdentityModelEventSource.ShowPII = true;

app.MapGet("/weatherforecast", (HttpContext context, ILogger<Program> logger) =>
{
    // Access the request headers
    var headers = context.Request.Headers;

    // Iterate over the headers and print them
    foreach (var (key, value) in headers)
    {
        // Print each header key and value
        foreach (var headerValue in value)
        {
            logger.LogInformation($"{key}: {headerValue}");
        }
    }

    //print claims
    var identity = context.User.Identity as ClaimsIdentity;
    if (identity != null)
    {
        IEnumerable<Claim> claims = identity.Claims;
        // or
        var claim = identity.FindFirst("name")?.Value;
        logger.LogInformation("name: {claim}", claim);

    }
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

//app.MapControllers().RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
