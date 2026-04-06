using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Datos.Inicializacion;
using ElectronicaVallarta.Extensiones;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso";
        options.AccessDeniedPath = "/acceso";
        options.Cookie.Name = "ElectronicaVallarta.Admin";
        options.SlidingExpiration = true;
    });
builder.Services.AddDbContext<ContextoAplicacion>(options =>
    options
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .UseSqlServer(
            builder.Configuration.GetConnectionString("ConexionSqlServer"),
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AgregarDependenciasAplicacion();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

using (var alcance = app.Services.CreateScope())
{
    var inicializador = alcance.ServiceProvider.GetRequiredService<InicializadorDatos>();
    await inicializador.InicializarAsync();
}

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();
