using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Datos.Inicializacion;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Repositorios;
using ElectronicaVallarta.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso";
        options.AccessDeniedPath = "/acceso";
        options.Cookie.Name = "ElectronicaVallarta.Admin";
        options.SlidingExpiration = true;
    });
builder.Services.AddDbContext<ContextoAplicacion>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexionSqlServer"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IRepositorioPais, RepositorioPais>();
builder.Services.AddScoped<IRepositorioSucursal, RepositorioSucursal>();
builder.Services.AddScoped<IRepositorioTasaCambio, RepositorioTasaCambio>();
builder.Services.AddScoped<IRepositorioUsuarioAdministrador, RepositorioUsuarioAdministrador>();
builder.Services.AddScoped<IServicioPais, ServicioPais>();
builder.Services.AddScoped<IServicioSucursal, ServicioSucursal>();
builder.Services.AddScoped<IServicioTasaCambio, ServicioTasaCambio>();
builder.Services.AddScoped<IServicioCalculadora, ServicioCalculadora>();
builder.Services.AddScoped<IServicioAutenticacionAdministrador, ServicioAutenticacionAdministrador>();
builder.Services.AddScoped<InicializadorDatos>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var alcance = app.Services.CreateScope())
{
    var inicializador = alcance.ServiceProvider.GetRequiredService<InicializadorDatos>();
    await inicializador.InicializarAsync();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();
