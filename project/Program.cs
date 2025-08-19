using System;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using project.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

// ADO.NET de la carpeta DATA
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Registrar el repositorio para poder resolverlo en la prueba
builder.Services.AddTransient<project.Models.Interfaces.IPropietarioRepository, project.Models.Repos.PropietarioRepository>();

var app = builder.Build();

// Asegurar que la BD exista al arrancar
try
{
    await DbInitializer.CreateDatabaseIfNotExistsAsync(builder.Configuration);
    app.Logger.LogInformation("Database ensured/created if it did not exist.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error al crear/verificar la base de datos. Revisa permisos y cadena de conexión.");
    throw;
}

// --- BLOQUE DE PRUEBA (solo para desarrollo) ---
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<project.Models.Interfaces.IPropietarioRepository>();
    var prueba = new project.Models.Propietario
    {
        Nombre = "Prueba",
        Apellido = "Repo",
        Dni = 99999999,
        Telefono = "00000000",
        Direccion = "Calle Test 123",
        Email = "prueba@local",
        LogicoProp = true
    };

    try
    {
        var id = repo.Alta(prueba); // Alta de prueba
        app.Logger.LogInformation("Alta de prueba creada. IdPropietario={Id}", id);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al crear propietario de prueba");
    }
}
// --- FIN BLOQUE DE PRUEBA ---

// Config de HTTP REQ
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

namespace project.Data
{
    public static class DbInitializer
    {
        public static async Task CreateDatabaseIfNotExistsAsync(IConfiguration configuration)
        {
            try
            {
                var cs = configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string missing.");

                var builder = new MySqlConnectionStringBuilder(cs);

                // Guarda el nombre de BD y conecta al servidor
                var databaseName = builder.Database;
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new InvalidOperationException("La cadena de conexión debe contener Database.");

                builder.Database = ""; // conecta al servidor sin DB
                var serverConnectionString = builder.ConnectionString;

                await using var conn = new MySqlConnection(serverConnectionString);
                await conn.OpenAsync();

                // HAcemos un "ESCAPEADO" al nombre y, finalmente, creamos la DB si no existe, gracias GPT 5,
                // se me estaba derritiendo el cerebro.
                // si quisiéramos crear las tablas de la base de datos de manera automatica al iniciar, debemos
                // escribir todo el chorizo de SQL que requeriria crearla a mano en mysql en PHPMyAdmin. JEJE.
                var escaped = $"`{databaseName.Replace("`", "``")}`";
                var createSql = $"CREATE DATABASE IF NOT EXISTS {escaped} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";

                await using var cmd = new MySqlCommand(createSql, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("No se pudo Crear inicializar la base de datos");
                Console.WriteLine(ex);
                Console.WriteLine("Mensaje de error: ", ex.Message);
            }
            
        }
    }
}
