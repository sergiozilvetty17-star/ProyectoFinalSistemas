using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await context.Users
                .OrderBy(u => u.ApellidoPaterno)
                .ThenBy(u => u.Nombres)
                .ToListAsync();

            var rolesPorUsuario = new Dictionary<string, string>();

            foreach (var usuario in usuarios)
            {
                var roles = await userManager.GetRolesAsync(usuario);

                rolesPorUsuario[usuario.Id] =
                    roles.FirstOrDefault() ?? "Sin rol";
            }

            ViewBag.RolesPorUsuario = rolesPorUsuario;

            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarCarreras();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            UsuarioCreateViewModel model)
        {
            await ValidarDatos(model);

            if (!ModelState.IsValid)
            {
                await CargarCarreras();
                return View(model);
            }

            var usuarioExistente =
                await userManager.FindByEmailAsync(model.Email);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Ya existe un usuario con ese correo.");

                await CargarCarreras();
                return View(model);
            }

            var ciNormalizada =
                NormalizarCI(model.CedulaIdentidad);

            var ciExiste = await context.Users
                .AnyAsync(u =>
                    u.CedulaIdentidad == ciNormalizada);

            if (ciExiste)
            {
                ModelState.AddModelError(
                    "CedulaIdentidad",
                    "Ya existe un usuario con esa C.I.");

                await CargarCarreras();
                return View(model);
            }

            var usuario = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombres = model.Nombres.Trim(),
                ApellidoPaterno = model.ApellidoPaterno.Trim(),
                ApellidoMaterno =
                    string.IsNullOrWhiteSpace(model.ApellidoMaterno)
                        ? null
                        : model.ApellidoMaterno.Trim(),
                CedulaIdentidad = ciNormalizada,
                PhoneNumber =
                    string.IsNullOrWhiteSpace(model.Telefono)
                        ? null
                        : model.Telefono.Trim(),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var resultado = await userManager.CreateAsync(
                usuario,
                model.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                await CargarCarreras();
                return View(model);
            }

            var rolResultado =
                await userManager.AddToRoleAsync(
                    usuario,
                    model.Rol);

            if (!rolResultado.Succeeded)
            {
                await userManager.DeleteAsync(usuario);

                foreach (var error in rolResultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                await CargarCarreras();
                return View(model);
            }

            if (model.Rol == "Estudiante")
            {
                context.Estudiantes.Add(new Estudiante
                {
                    ApplicationUserId = usuario.Id,
                    CarreraId = model.CarreraId!.Value,
                    Semestre = model.Semestre,
                    Activo = true
                });
            }
            else
            {
                context.Docentes.Add(new Docente
                {
                    ApplicationUserId = usuario.Id,
                    Especialidad = string.IsNullOrWhiteSpace(
                        model.Especialidad)
                            ? null
                            : model.Especialidad.Trim(),
                    Activo = true
                });
            }

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Usuario {usuario.NombreCompleto} creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index));

            var usuario =
                await userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                TempData["Error"] =
                    "Usuario no encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var roles =
                await userManager.GetRolesAsync(usuario);

            var rol = roles.FirstOrDefault();

            if (rol == "Administrador")
            {
                TempData["Error"] =
                    "Los datos del administrador no se pueden editar desde este módulo.";

                return RedirectToAction(nameof(Index));
            }

            var model = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                CedulaIdentidad = usuario.CedulaIdentidad ?? string.Empty,
                Telefono = usuario.PhoneNumber,
                Email = usuario.Email ?? string.Empty,
                Rol = rol ?? string.Empty
            };

            if (rol == "Estudiante")
            {
                var estudiante =
                    await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == usuario.Id);

                if (estudiante != null)
                {
                    model.CarreraId =
                        estudiante.CarreraId;

                    model.Semestre =
                        estudiante.Semestre;

                    model.Activo =
                        estudiante.Activo;
                }
            }
            else if (rol == "Docente")
            {
                var docente =
                    await context.Docentes
                        .FirstOrDefaultAsync(d =>
                            d.ApplicationUserId == usuario.Id);

                if (docente != null)
                {
                    model.Especialidad =
                        docente.Especialidad;

                    model.Activo =
                        docente.Activo;
                }
            }

            await CargarCarreras();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UsuarioEditViewModel model)
        {
            if (model.Rol == "Estudiante" &&
                !model.CarreraId.HasValue)
            {
                ModelState.AddModelError(
                    "CarreraId",
                    "Seleccione una carrera.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCarreras();
                return View(model);
            }

            var usuario =
                await userManager.FindByIdAsync(model.Id);

            if (usuario == null)
            {
                TempData["Error"] =
                    "Usuario no encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var roles =
                await userManager.GetRolesAsync(usuario);

            var rol = roles.FirstOrDefault();

            if (rol == "Administrador")
            {
                TempData["Error"] =
                    "No se puede modificar un administrador.";

                return RedirectToAction(nameof(Index));
            }

            var usuarioConMismoCorreo =
                await userManager.FindByEmailAsync(model.Email);

            if (usuarioConMismoCorreo != null &&
                usuarioConMismoCorreo.Id != usuario.Id)
            {
                ModelState.AddModelError(
                    "Email",
                    "Ya existe otro usuario con ese correo.");

                await CargarCarreras();
                return View(model);
            }

            var ciNormalizada =
                NormalizarCI(model.CedulaIdentidad);

            var ciExiste = await context.Users
                .AnyAsync(u =>
                    u.CedulaIdentidad == ciNormalizada &&
                    u.Id != usuario.Id);

            if (ciExiste)
            {
                ModelState.AddModelError(
                    "CedulaIdentidad",
                    "Ya existe otro usuario con esa C.I.");

                await CargarCarreras();
                return View(model);
            }

            usuario.Nombres =
                model.Nombres.Trim();

            usuario.ApellidoPaterno =
                model.ApellidoPaterno.Trim();

            usuario.ApellidoMaterno =
                string.IsNullOrWhiteSpace(model.ApellidoMaterno)
                    ? null
                    : model.ApellidoMaterno.Trim();

            usuario.CedulaIdentidad =
                ciNormalizada;

            usuario.PhoneNumber =
                string.IsNullOrWhiteSpace(model.Telefono)
                    ? null
                    : model.Telefono.Trim();

            usuario.Email =
                model.Email.Trim();

            usuario.UserName =
                model.Email.Trim();

            var updateResult =
                await userManager.UpdateAsync(usuario);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                await CargarCarreras();
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(
                    model.NewPassword))
            {
                var token =
                    await userManager
                        .GeneratePasswordResetTokenAsync(usuario);

                var passwordResult =
                    await userManager.ResetPasswordAsync(
                        usuario,
                        token,
                        model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    await CargarCarreras();
                    return View(model);
                }
            }

            if (rol == "Estudiante")
            {
                var estudiante =
                    await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == usuario.Id);

                if (estudiante != null)
                {
                    estudiante.CarreraId =
                        model.CarreraId!.Value;

                    estudiante.Semestre =
                        model.Semestre;

                    estudiante.Activo =
                        model.Activo;
                }
            }
            else if (rol == "Docente")
            {
                var docente =
                    await context.Docentes
                        .FirstOrDefaultAsync(d =>
                            d.ApplicationUserId == usuario.Id);

                if (docente != null)
                {
                    docente.Especialidad =
                        string.IsNullOrWhiteSpace(
                            model.Especialidad)
                                ? null
                                : model.Especialidad.Trim();

                    docente.Activo =
                        model.Activo;
                }
            }

            usuario.LockoutEnabled = true;

            usuario.LockoutEnd = model.Activo
                ? null
                : DateTimeOffset.UtcNow.AddYears(100);

            await userManager.UpdateAsync(usuario);

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Usuario {usuario.NombreCompleto} actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(string id)
        {
            var usuario =
                await userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                TempData["Error"] =
                    "Usuario no encontrado.";

                return RedirectToAction(nameof(Index));
            }

            var roles =
                await userManager.GetRolesAsync(usuario);

            if (roles.Contains("Administrador"))
            {
                TempData["Error"] =
                    "No se puede desactivar un administrador.";

                return RedirectToAction(nameof(Index));
            }

            if (roles.Contains("Estudiante"))
            {
                var estudiante =
                    await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == usuario.Id);

                if (estudiante != null)
                {
                    estudiante.Activo =
                        !estudiante.Activo;

                    usuario.LockoutEnabled = true;

                    usuario.LockoutEnd =
                        estudiante.Activo
                            ? null
                            : DateTimeOffset.UtcNow.AddYears(100);
                }
            }
            else if (roles.Contains("Docente"))
            {
                var docente =
                    await context.Docentes
                        .FirstOrDefaultAsync(d =>
                            d.ApplicationUserId == usuario.Id);

                if (docente != null)
                {
                    docente.Activo =
                        !docente.Activo;

                    usuario.LockoutEnabled = true;

                    usuario.LockoutEnd =
                        docente.Activo
                            ? null
                            : DateTimeOffset.UtcNow.AddYears(100);
                }
            }

            await userManager.UpdateAsync(usuario);

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Usuario {usuario.NombreCompleto} actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCarreras()
        {
            ViewBag.Carreras =
                await context.Carreras
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
        }

        private async Task ValidarDatos(
            UsuarioCreateViewModel model)
        {
            if (model.Rol != "Estudiante" &&
                model.Rol != "Docente")
            {
                ModelState.AddModelError(
                    "Rol",
                    "Seleccione un rol válido.");

                return;
            }

            if (model.Rol == "Estudiante")
            {
                if (!model.CarreraId.HasValue)
                {
                    ModelState.AddModelError(
                        "CarreraId",
                        "Seleccione una carrera.");
                }

                if (!model.Semestre.HasValue)
                {
                    ModelState.AddModelError(
                        "Semestre",
                        "Seleccione un semestre.");
                }
            }
        }

        private static string NormalizarCI(string ci)
        {
            return ci
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "");
        }
    }
}
