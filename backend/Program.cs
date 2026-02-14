using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// 1. CONFIGURACIÓN DE BASE DE DATOS (ANTES DE HACER BUILD)
// -------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
var renderUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(renderUrl))
{
    // --- ESTAMOS EN RENDER (NUBE) ---
    try 
    {
        var databaseUri = new Uri(renderUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        // CORRECCIÓN DEL PUERTO: Si no viene puerto (-1), usamos el 5432
        int puerto = databaseUri.Port > 0 ? databaseUri.Port : 5432;

        string connectionStrRender = $"Host={databaseUri.Host};Port={puerto};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionStrRender));
            
        Console.WriteLine($"--> Usando PostgreSQL en Nube. Host: {databaseUri.Host}, Puerto: {puerto}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configurando Postgres: {ex.Message}");
    }
}
else
{
    // --- ESTAMOS EN LOCAL (TU PC) ---
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    Console.WriteLine("--> Usando SQL Server Local");
}

// -------------------------------------------------------------------------
// 2. OTRAS CONFIGURACIONES (CORS, SWAGGER)
// -------------------------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------------------------
// 3. CONSTRUIR LA APP (EL HORNO) 🍳
// A partir de aquí ya no se puede usar "builder.Services"
// -------------------------------------------------------------------------
var app = builder.Build();

// -------------------------------------------------------------------------
// 4. CREACIÓN AUTOMÁTICA DE TABLAS
// Esto se ejecuta justo al arrancar
// -------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // <--- ESTO CREA LAS TABLAS SI NO EXISTEN
        Console.WriteLine("--> Base de datos verificada/creada correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error creando tablas: {ex.Message}");
    }
}

// -------------------------------------------------------------------------
// 5. MIDDLEWARES Y ENDPOINTS
// -------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");

// --- ENDPOINTS ---

app.MapGet("/tareas", async (AppDbContext db, int usuarioId) => {
    return await db.Tareas
    .Where(t => t.UsuarioId == usuarioId)
    .ToListAsync();
});

app.MapPost("/tareas", async (AppDbContext db, Tarea nuevaTarea) => {
    await db.Tareas.AddAsync(nuevaTarea);
    await db.SaveChangesAsync();
    return Results.Ok(nuevaTarea);
});

app.MapPut("/tareas/{id}", async (int id, int usuarioId, AppDbContext db, Tarea tareaActualizada) => {
    var tarea = await db.Tareas
        .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);
    
    if (tarea == null) return Results.NotFound();

    tarea.Nombre = tareaActualizada.Nombre;
    tarea.Completada = tareaActualizada.Completada;

    await db.SaveChangesAsync();
    return Results.Ok(tarea);
});

app.MapDelete("/tareas/{id}", async (int id, int usuarioId, AppDbContext db) => {
    var tarea = await db.Tareas
    .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);

    if (tarea == null) return Results.NotFound();

    db.Tareas.Remove(tarea);
    await db.SaveChangesAsync();
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

app.Run(); // ¡SOLO UNO AL FINAL!

// --- MODELOS ---

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

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Tarea> Tareas {get; set;}
    public DbSet<Usuario> Usuarios {get; set;}
}