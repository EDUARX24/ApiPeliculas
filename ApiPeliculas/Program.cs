using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));


//configuracion cache global
builder.Services.AddControllers(options =>
{
    //cache profile global, para no agregarla en cada uno de los controladores
    options.CacheProfiles.Add("Cache20segundos", new CacheProfile()
    {
        Duration = 20
    });
});


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
            //doc api version
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ApiPeliculas V1", 
                Version = "v1",
                Description = "Api de Peliculas",
                Contact = new OpenApiContact()
                {
                    Name = "Eduar",
                    Email = "eduarfrancis11@gmail.com"

                }
            });
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "ApiPeliculas V2",
                Version = "v2",
                Description = "Api de Peliculas",
                Contact = new OpenApiContact()
                {
                    Name = "Eduar",
                    Email = "eduarfrancis11@gmail.com"

                }
            });
        }
    );

//config para los dominios
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("https://localhost:7140").AllowAnyMethod().AllowAnyHeader();
}));

//soporte para las cache
builder.Services.AddResponseCaching();

//agregar el repositorio
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepostorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//soporte para las versiones
var apiVersion = builder.Services.AddApiVersioning(opcion =>
{
    opcion.AssumeDefaultVersionWhenUnspecified = true;
    opcion.DefaultApiVersion = new ApiVersion(1, 0);
    opcion.ReportApiVersions = true;
    //opcion.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
});

apiVersion.AddApiExplorer(opcion =>
{
    opcion.GroupNameFormat = "'v'VVV";
    opcion.SubstituteApiVersionInUrl = true;
});



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
    app.UseSwaggerUI(opticiones =>
    {
        opticiones.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiPeliculas v1");
        opticiones.SwaggerEndpoint("/swagger/v2/swagger.json", "ApiPeliculas v2");
    });
}

app.UseHttpsRedirection();

//soporte para cors
app.UseCors("PoliticaCors");

//soporte para autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
