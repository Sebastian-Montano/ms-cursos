using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ms_cursos.Exceptions;
using ms_cursos.Infrastructure.Database;
using ms_cursos.Repositories;
using ms_cursos.Services;

var builder = WebApplication.CreateBuilder(args);

// Lee Host y Port desde appsettings.json.
var host = builder.Configuration["Host"] ?? "127.0.0.1";
var port = builder.Configuration.GetValue<int?>("Port") ?? 3000;

var secret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Secret");

builder.WebHost.UseUrls($"http://{host}:{port}");

// Controladores, nombres JSON en snake_case y formato de errores de validación.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.SnakeCaseLower;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(new
            {
                error = "La solicitud no es válida",
                detalles = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => JsonNamingPolicy.SnakeCaseLower
                            .ConvertName(entry.Key),
                        entry => entry.Value!.Errors
                            .Select(error => error.ErrorMessage)
                            .ToArray())
            });
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret)),
            ValidAlgorithms = new[]
            {
                SecurityAlgorithms.HmacSha256
            },
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "nombre",
            RoleClaimType = "rol"
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Falta el token de acceso (cabecera Authorization: Bearer <token>)"
                });
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var rol = context.HttpContext.User
                    .FindFirst("rol")?.Value;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = $"El rol {rol ?? "desconocido"} no tiene permiso para esta operación."
                });
            }
        };
    });

builder.Services.AddAuthorization();

// Inyección de dependencias.
builder.Services.AddSingleton<
    IMySqlConnectionFactory,
    MySqlConnectionFactory>();

builder.Services.AddSingleton<
    ICursosRepository,
    CursosRepository>();

builder.Services.AddScoped<
    ICursosService,
    CursosService>();

var app = builder.Build();

// Registra cada petición con código de respuesta y duración.
app.Use(async (context, next) =>
{
    var inicio = DateTime.Now;
    await next();

    var ms = (int)(DateTime.Now - inicio).TotalMilliseconds;

    Console.WriteLine(
        $"{inicio:HH:mm:ss} {context.Request.Method} " +
        $"{context.Request.Path}{context.Request.QueryString} → " +
        $"{context.Response.StatusCode} ({ms} ms)");
});

// Captura excepciones y responde con JSON.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error no controlado");

        if (context.Response.HasStarted)
            throw;

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        var statusCode = ex is ApiException apiException
            ? apiException.StatusCode
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;

        var error = new Dictionary<string, object?>
        {
            ["error"] =
                statusCode == StatusCodes.Status500InternalServerError
                    ? "Error interno del servidor"
                    : ex.Message
        };

        if (ex is ApiException { Details: not null } apiError)
            error["detalles"] = apiError.Details;

        await context.Response.WriteAsJsonAsync(error);
    }
});

// Respuesta JSON para rutas inexistentes.
app.UseStatusCodePages(async statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;

    if (context.Response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = $"Ruta no encontrada: {context.Request.Method} " +
                    $"{context.Request.Path}{context.Request.QueryString}"
        });
    }
});

// Endpoint de salud.
app.MapGet("/health", () => Results.Ok(new
{
    servicio = "ms-cursos",
    estado = "ok"
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/* 
appsettings
{
  "Servicios": {
    "EstudiantesBaseUrl": "http://127.0.0.1:3012/"
  }
}

program.cs
builder.Services.AddHttpClient<EstudiantesClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Servicios:EstudiantesBaseUrl"]!
    );
});

using System.Net.Http.Json;

public class EstudiantesClient(HttpClient httpClient)
{
    public async Task<EstudianteDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/estudiantes/{id}",
            cancellationToken
        );

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EstudianteDto>(
            cancellationToken: cancellationToken
        );
    }
}

public record EstudianteDto(int Id, string Nombre);

*/