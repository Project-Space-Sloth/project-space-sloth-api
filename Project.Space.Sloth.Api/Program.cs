using Project.Space.Sloth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Project.Space.Sloth.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

string authority = builder.Configuration["Auth0:Authority"] ??
    throw new ArgumentNullException("Auth0:Authority");

string audience = builder.Configuration["Auth0:Audience"] ??
    throw new ArgumentNullException("Auth0:Audience");

string storeConnectionString = builder.Configuration.GetConnectionString("StoreConnection") ??
    throw new ArgumentNullException("ConnectionString:StoreConnection");

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.Authority = authority;
    options.Audience = audience;
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("delete:catalog", policy => 
        policy.RequireAuthenticatedUser().RequireClaim("scope", "delete:catalog"));
});

builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(storeConnectionString, 
    b => b.MigrationsAssembly("Project.Space.Sloth.Api")));

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder.WithOrigins("http://localhost:3000", 
            "https://ambitious-meadow-0bb7cf50f.1.azurestaticapps.net")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
