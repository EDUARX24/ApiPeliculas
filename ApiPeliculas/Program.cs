using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//METODO CREAR LOGIN DOCUMENTACION
builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \r\n\r\n" +
                              "Ingresa la palabra 'Bearer' [Espacio] y luego tu token en el campo de texto debajo.\r\n\r\n" +
                              "Ejemplo: \"Bearer 123SDS\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "Bearer",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        }
    );

//config para los dominios
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("https://localhost:7140").AllowAnyMethod().AllowAnyHeader();
}));

//agregar el repositorio
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepostorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//agregar automapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

//configuracion de autenticacion
builder.Services.AddAuthentication(
    x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//soporte para cors
app.UseCors("PoliticaCors");

//soporte para autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
