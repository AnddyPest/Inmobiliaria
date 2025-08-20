using System;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using project.Data;
using project.Services;
using project.Models.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

// ADO.NET de la carpeta DATA
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<IInquilinoService,InquilinoService>();


// Registrar el repositorio para poder resolverlo en la prueba
builder.Services.AddTransient<project.Models.Interfaces.IPropietarioRepository, project.Models.Repos.PropietarioRepository>();

var app = builder.Build();

// Asegurar que la BD exista al arrancar
//try
//{
//    await DbInitializer.CreateDatabaseIfNotExistsAsync(builder.Configuration);
//    app.Logger.LogInformation("Database ensured/created if it did not exist.");
//}
//catch (Exception ex)
//{
//    app.Logger.LogError(ex, "Error al crear/verificar la base de datos. Revisa permisos y cadena de conexión.");
//    throw;
//}

// --- BLOQUE DE PRUEBA (solo para desarrollo) ---
//using (var scope = app.Services.CreateScope())
//{
//    var repo = scope.ServiceProvider.GetRequiredService<project.Models.Interfaces.IPropietarioRepository>();
//    var rnd = new Random();
//    var unique = rnd.Next(10000, 99999);
//    var testDni = 70000000 + unique;
//    var testEmail = $"test{unique}@local";

//    var prueba = new project.Models.Propietario
//    {
//        Nombre = "Prueba",
//        Apellido = "Repo",
//        Dni = testDni,
//        Telefono = "00000000",
//        Direccion = "Calle Test 123",
//        Email = testEmail,
//        LogicoProp = true
//    };

//    try
//    {
//        app.Logger.LogInformation("=== TEST START ===");

//        // Alta
//        int id = -1;
//        try
//        {
//            id = repo.Alta(prueba);
//            app.Logger.LogInformation("Alta OK -> IdPropietario={Id}", id);
//        }
//        catch (NotImplementedException)
//        {
//            app.Logger.LogWarning("Alta no implementado.");
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "Alta fallo.");
//        }

//        // ObtenerPorDni
//        try
//        {
//            var pByDni = repo.ObtenerPorDni(testDni);
//            app.Logger.LogInformation("ObtenerPorDni -> {Result}", pByDni != null ? $"Found IdPropietario={pByDni.IdPropietario}" : "null");
//        }
//        catch (NotImplementedException)
//        {
//            app.Logger.LogWarning("ObtenerPorDni no implementado.");
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "ObtenerPorDni fallo.");
//        }

//        // ObtenerTodos
//        try
//        {
//            var todos = repo.ObtenerTodos();
//            app.Logger.LogInformation("ObtenerTodos -> count={Count}", todos?.Count ?? 0);
//        }
//        catch (NotImplementedException)
//        {
//            app.Logger.LogWarning("ObtenerTodos no implementado.");
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "ObtenerTodos fallo.");
//        }

//        // ObtenerPorNombre
//        try
//        {
//            var porNombre = repo.ObtenerPorNombre("Prueba");
//            app.Logger.LogInformation("ObtenerPorNombre('Prueba') -> count={Count}", porNombre?.Count ?? 0);
//        }
//        catch (NotImplementedException)
//        {
//            app.Logger.LogWarning("ObtenerPorNombre no implementado.");
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "ObtenerPorNombre fallo.");
//        }

//        // ObtenerCantidad
//        try
//        {
//            var cant = repo.ObtenerCantidad();
//            app.Logger.LogInformation("ObtenerCantidad -> {Count}", cant);
//        }
//        catch (NotImplementedException)
//        {
//            app.Logger.LogWarning("ObtenerCantidad no implementado.");
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "ObtenerCantidad fallo.");
//        }

//        // Editar (si Alta devolvió id)
//        if (id > 0)
//        {
//            try
//            {
//                prueba.IdPropietario = id;
//                prueba.Nombre = "PruebaMod";
//                var filas = repo.Editar(prueba);
//                app.Logger.LogInformation("Editar -> filas afectadas = {Rows}", filas);
//            }
//            catch (NotImplementedException)
//            {
//                app.Logger.LogWarning("Editar no implementado.");
//            }
//            catch (Exception ex)
//            {
//                app.Logger.LogError(ex, "Editar fallo.");
//            }
//        }

//        // Baja lógica
//        if (id > 0)
//        {
//            try
//            {
//                var filas = repo.Baja(id);
//                app.Logger.LogInformation("Baja -> filas afectadas = {Rows}", filas);
//            }
//            catch (NotImplementedException)
//            {
//                app.Logger.LogWarning("Baja no implementado.");
//            }
//            catch (Exception ex)
//            {
//                app.Logger.LogError(ex, "Baja fallo.");
//            }

//            // Reestablecer
//            try
//            {
//                var filas = repo.Reestablecer(id);
//                app.Logger.LogInformation("Reestablecer -> filas afectadas = {Rows}", filas);
//            }
//            catch (NotImplementedException)
//            {
//                app.Logger.LogWarning("Reestablecer no implementado.");
//            }
//            catch (Exception ex)
//            {
//                app.Logger.LogError(ex, "Reestablecer fallo.");
//            }
//        }

//        app.Logger.LogInformation("=== TEST END ===");
//    }
//    catch (Exception ex)
//    {
//        app.Logger.LogError(ex, "Error en el bloque de pruebas.");
//    }
//}
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
