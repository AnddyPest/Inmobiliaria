using System;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

using project.Services;
using project.Models.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

// ADO.NET de la carpeta DATA

builder.Services.AddSingleton<IInquilinoService, InquilinoService>();
builder.Services.AddSingleton<IPersonaService, PersonaService>();
builder.Services.AddSingleton<IPropietarioService, PropietarioService>();
builder.Services.AddSingleton<IContratoService, ContratoService>();
builder.Services.AddSingleton<IInmuebleService, InmuebleService>();
builder.Services.AddSingleton<ITipo_InmuebleService, Tipo_InmuebleService>();
builder.Services.AddSingleton<IEmpleadoService, EmpleadoService>();
builder.Services.AddSingleton<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/unauthorized";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});
builder.Services.AddAuthorization(options =>
    options.AddPolicy("SoloAdministrador", policy => policy.RequireRole("Administrador"))
);
// Registrar PagosService para IPagosService
builder.Services.AddSingleton<IPagosService, PagosService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Config de HTTP REQ
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

