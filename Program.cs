using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles =
    {
        "Administrador",
        "Docente",
        "Estudiante"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new IdentityRole(role));
        }
    }

    var carreras = new[]
    {
        "Ingeniería de Sistemas",
        "Derecho",
        "Ingeniería Comercial",
        "Administración de Empresas",
        "Contaduría Pública"
    };

    foreach (var nombreCarrera in carreras)
    {
        var existe = await context.Carreras
            .AnyAsync(c => c.Nombre == nombreCarrera);

        if (!existe)
        {
            context.Carreras.Add(new Carrera
            {
                Nombre = nombreCarrera,
                Activa = true
            });
        }
    }

    await context.SaveChangesAsync();

    var adminEmail =
        Environment.GetEnvironmentVariable(
            "EXAMSECURE_ADMIN_EMAIL");

    var adminPassword =
        Environment.GetEnvironmentVariable(
            "EXAMSECURE_ADMIN_PASSWORD");

    var adminName =
        Environment.GetEnvironmentVariable(
            "EXAMSECURE_ADMIN_NAME");

    if (!string.IsNullOrWhiteSpace(adminEmail) &&
        !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin =
            await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            var nombreCompleto =
                string.IsNullOrWhiteSpace(adminName)
                    ? "Administrador General"
                    : adminName;

            var partes = nombreCompleto
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            var nombres =
                partes.Length > 0
                    ? partes[0]
                    : "Administrador";

            var apellido =
                partes.Length > 1
                    ? partes[^1]
                    : "General";

            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombres = nombres,
                ApellidoPaterno = apellido,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await userManager.CreateAsync(
                    admin,
                    adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(e => e.Description));

                throw new Exception(
                    $"No se pudo crear el administrador: {errors}");
            }

            await userManager.AddToRoleAsync(
                admin,
                "Administrador");

            Console.WriteLine(
                $"Administrador creado: {adminEmail}");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(admin.Nombres))
                admin.Nombres = "Administrador";

            if (string.IsNullOrWhiteSpace(admin.ApellidoPaterno))
                admin.ApellidoPaterno = "General";

            await userManager.UpdateAsync(admin);

            if (!await userManager.IsInRoleAsync(
                    admin,
                    "Administrador"))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Administrador");
            }

            Console.WriteLine(
                $"Administrador existente: {adminEmail}");
        }
    }
    else
    {
        Console.WriteLine(
            "Variables EXAMSECURE_ADMIN_EMAIL y EXAMSECURE_ADMIN_PASSWORD no configuradas.");
    }
}

if (args.Contains("--seed-demo"))
{
    using var demoScope = app.Services.CreateScope();

    var demoContext =
        demoScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    var demoUserManager =
        demoScope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    await DemoDataSeeder.SeedAsync(
        demoContext,
        demoUserManager);
}
app.Run();



