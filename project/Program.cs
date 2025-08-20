using System;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using project.Data;
using project.Services;
using project.Models.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

// ADO.NET de la carpeta DATA
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<IInquilinoService, InquilinoService>();
builder.Services.AddSingleton<IPersonaService, PersonaService>();
builder.Services.AddSingleton<IPropietarioService, PropietarioService>();


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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

