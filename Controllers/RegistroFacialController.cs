using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RegistroFacialController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Registrar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "Estudiante no especificado.";
                return RedirectToAction("Index", "Usuarios");
            }

            var estudiante = await context.Estudiantes
                .Include(e => e.Usuario)
                .Include(e => e.Carrera)
                .FirstOrDefaultAsync(e => e.Usuario != null &&
                                          e.Usuario.Id == id);

            if (estudiante == null || estudiante.Usuario == null)
            {
                TempData["Error"] = "Estudiante no encontrado.";
                return RedirectToAction("Index", "Usuarios");
            }

            var registroExistente = await context.RegistrosFaciales
                .FirstOrDefaultAsync(r => r.EstudianteId == estudiante.Id);

            ViewBag.TieneRegistroFacial =
                registroExistente != null &&
                registroExistente.Activo;

            return View(estudiante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
            string estudianteId,
            string descriptor)
        {
            if (string.IsNullOrWhiteSpace(estudianteId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Estudiante no especificado."
                });
            }

            if (string.IsNullOrWhiteSpace(descriptor))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se recibió el descriptor facial."
                });
            }

            var estudiante = await context.Estudiantes
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e =>
                    e.Usuario != null &&
                    e.Usuario.Id == estudianteId);

            if (estudiante == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Estudiante no encontrado."
                });
            }

            if (estudiante.Usuario == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El estudiante no tiene usuario asociado."
                });
            }

            var roles = await userManager.GetRolesAsync(
                estudiante.Usuario);

            if (!roles.Contains("Estudiante"))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El usuario seleccionado no es un estudiante."
                });
            }

            float[] valores;

            try
            {
                valores = descriptor
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(valor => float.Parse(
                        valor,
                        System.Globalization.CultureInfo.InvariantCulture))
                    .ToArray();
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El descriptor facial tiene un formato inválido."
                });
            }

            if (valores.Length != 128)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"El descriptor debe contener 128 valores. Se recibieron {valores.Length}."
                });
            }

            var embedding = new byte[valores.Length * sizeof(float)];

            Buffer.BlockCopy(
                valores,
                0,
                embedding,
                0,
                embedding.Length);

            var registro = await context.RegistrosFaciales
                .FirstOrDefaultAsync(r =>
                    r.EstudianteId == estudiante.Id);

            if (registro == null)
            {
                registro = new RegistroFacial
                {
                    EstudianteId = estudiante.Id,
                    FaceEmbedding = embedding,
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                };

                context.RegistrosFaciales.Add(registro);
            }
            else
            {
                registro.FaceEmbedding = embedding;
                registro.FechaRegistro = DateTime.UtcNow;
                registro.Activo = true;
            }

            await context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Registro facial guardado correctamente."
            });
        }
    }
}

