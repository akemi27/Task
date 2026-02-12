using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);

//Conexión 
var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
var renderUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(renderUrl))
{
    try 
    {
        var databaseUri = new Uri(renderUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
            
        Console.WriteLine("--> Usando Base de Datos PostgreSQL (Nube)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configurando Postgres: {ex.Message}");
    }
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
}


//cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 1. CONFIGURACIÓN DEL VISUALIZADOR (SWAGGER)
// Esto prepara la documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. ACTIVAR EL VISUALIZADOR
// Esto hace que la página azul sea accesible
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Activar nueva politica
app.UseCors("NuevaPolitica");

// (Opcional) Redirección HTTPS desactivada para evitar problemas locales
// app.UseHttpsRedirection(); 

// --- TUS ENDPOINTS (Tus "ventanillas" de atención) ---

// --- ENDPOINTS CONECTADOS A BASE DE DATOS ---

// 1. GET: Obtener tareas desde SQL
// Inyectamos "AppDbContext db" para poder usar la base de datos
app.MapGet("/tareas", async (AppDbContext db, int usuarioId) => {
    return await db.Tareas
    .Where(t => t.UsuarioId == usuarioId)
    .ToListAsync();
});

// 2. POST: Guardar en SQL
app.MapPost("/tareas", async (AppDbContext db, Tarea nuevaTarea) => {
    // Ya no calculamos ID, SQL Server lo hace solo
    await db.Tareas.AddAsync(nuevaTarea);
    await db.SaveChangesAsync(); // ¡IMPORTANTE! Esto confirma el guardado
    return Results.Ok(nuevaTarea);
});

// 3. PUT: Actualizar en SQL
app.MapPut("/tareas/{id}", async (int id, int usuarioId, AppDbContext db, Tarea tareaActualizada) => {
    var tarea = await db.Tareas
        .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);
    
    if (tarea == null) return Results.NotFound();

    tarea.Nombre = tareaActualizada.Nombre;
    tarea.Completada = tareaActualizada.Completada;

    await db.SaveChangesAsync(); // Guardamos cambios
    return Results.Ok(tarea);
});

// 4. DELETE: Borrar de SQL
app.MapDelete("/tareas/{id}", async (int id, int usuarioId, AppDbContext db) => {
    var tarea = await db.Tareas
    .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);

    if (tarea == null) return Results.NotFound();

    db.Tareas.Remove(tarea);
    await db.SaveChangesAsync(); // Confirmamos borrado
    return Results.Ok("Eliminado");
});

app.MapPost("/login", async(AppDbContext db, Usuario usuarioLogin) =>
{
   var usuarioEncontrado = await db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuarioLogin.NombreUsuario);

    if(usuarioEncontrado == null) return Results.Unauthorized();

    bool passOK = BCrypt.Net.BCrypt.Verify(usuarioLogin.Password, usuarioEncontrado.Password);

    if (passOK)
    {
        return Results.Ok(new { 
            Mensaje = "Login exitoso", 
            Usuario = usuarioEncontrado.NombreUsuario,
            Id = usuarioEncontrado.Id 
        });
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/registro", async(AppDbContext db, Usuario nuevoUsuario) =>
{
    var usuarioExiste = await db.Usuarios.AnyAsync(u => u.NombreUsuario == nuevoUsuario.NombreUsuario);
    if (usuarioExiste) return Results.BadRequest("El usuario existe");
    
    string hashPassword = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Password);
    nuevoUsuario.Password = hashPassword;

    db.Usuarios.Add(nuevoUsuario);
    await db.SaveChangesAsync();
    return Results.Ok("Usuario creado");
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var databaseUri = new Uri(renderUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        // --- CORRECCIÓN AQUÍ ---
        // Si el puerto es -1 (no especificado), forzamos el 5432.
        int puerto = databaseUri.Port > 0 ? databaseUri.Port : 5432;

        connectionString = $"Host={databaseUri.Host};Port={puerto};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
            
        Console.WriteLine($"--> Usando PostgreSQL en Nube. Host: {databaseUri.Host}, Puerto: {puerto}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configurando Postgres: {ex.Message}");
    }
}
// --- FIN ---

app.Run(); // Esta línea ya la tenías

app.Run();

// --- TUS MODELOS (Clases) ---

public class Tarea
{
    public int Id {get; set;}
    public string Nombre {get; set;}
    public bool Completada {get; set;}
    public int UsuarioId {get; set;}
    public DateTime? FechaVencimiento {get; set;}
}

public class Usuario
{
    public int Id {get; set;}
    public string NombreUsuario {get;set;}
    public string Password {get;set;}
}

//Puente con la BD

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Tarea> Tareas {get; set;}
    public DbSet<Usuario> Usuarios {get; set;}
}