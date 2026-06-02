using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Planeta_New.Extensions;
using Planeta.Infrastructure.Extensions;
using Planeta.Application.ApplicationExtensions;
using Planeta.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();



builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllers();
builder.Services.AddPlanetaSwagger();

builder.Services.AddPlanetaCors();

//builder.Services.AddSwaggerExtension();


var app = builder.Build();


/*using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    try
    {
        var context = service.GetRequiredService<PlanetaDbContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = service.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}*/




if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("PlanetaOpenCorsPolicy");
/*app.UseSwagger();
app.UseSwaggerUI();*/

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseSwaggerExtension();
app.Run();