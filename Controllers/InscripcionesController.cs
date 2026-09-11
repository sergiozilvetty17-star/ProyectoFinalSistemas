using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class InscripcionesController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var inscripciones = await context.Inscripciones
                .Include(i => i.Estudiante)
                    .ThenInclude(e => e!.Usuario)
                .Include(i => i.Estudiante)
                    .ThenInclude(e => e!.Carrera)
                .Include(i => i.Materia)
                    .ThenInclude(m => m!.Docente)
                        .ThenInclude(d => d!.Usuario)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();

            return View(inscripciones);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarDatosFormularioAsync();

            return View(new InscripcionCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InscripcionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosFormularioAsync(model.EstudianteId, model.MateriaId);
                return View(model);
            }

            var estudiante = await context.Estudiantes
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e =>
                    e.Id == model.EstudianteId &&
                    e.Activo &&
                    e.Usuario != null);

            if (estudiante == null)
            {
                ModelState.AddModelError(
                    nameof(model.EstudianteId),
                    "El estudiante seleccionado no existe o está inactivo.");

                await CargarDatosFormularioAsync(
                    model.EstudianteId,
                    model.MateriaId);

                return View(model);
            }

            var materia = await context.Materias
                .FirstOrDefaultAsync(m =>
                    m.Id == model.MateriaId &&
                    m.Activa);

            if (materia == null)
            {
                ModelState.AddModelError(
                    nameof(model.MateriaId),
                    "La materia seleccionada no existe o está inactiva.");

                await CargarDatosFormularioAsync(
                    model.EstudianteId,
                    model.MateriaId);

                return View(model);
            }

            var existe = await context.Inscripciones
                .AnyAsync(i =>
                    i.EstudianteId == model.EstudianteId &&
                    i.MateriaId == model.MateriaId);

            if (existe)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El estudiante ya está inscrito en esta materia.");

                await CargarDatosFormularioAsync(
                    model.EstudianteId,
                    model.MateriaId);

                return View(model);
            }

            var inscripcion = new Inscripcion
            {
                EstudianteId = model.EstudianteId,
                MateriaId = model.MateriaId,
                FechaInscripcion = DateTime.UtcNow,
                Activa = true
            };

            context.Inscripciones.Add(inscripcion);

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Inscripción registrada correctamente para {estudiante.Usuario!.NombreCompleto}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var inscripcion = await context.Inscripciones
                .Include(i => i.Estudiante)
                    .ThenInclude(e => e!.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscripcion == null)
            {
                TempData["Error"] = "La inscripción no existe.";
                return RedirectToAction(nameof(Index));
            }

            inscripcion.Activa = !inscripcion.Activa;

            await context.SaveChangesAsync();

            TempData["Success"] = inscripcion.Activa
                ? "Inscripción activada correctamente."
                : "Inscripción desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inscripcion = await context.Inscripciones
                .Include(i => i.Estudiante)
                    .ThenInclude(e => e!.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscripcion == null)
            {
                TempData["Error"] = "La inscripción no existe.";
                return RedirectToAction(nameof(Index));
            }

            context.Inscripciones.Remove(inscripcion);

            await context.SaveChangesAsync();

            TempData["Success"] = "Inscripción eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDatosFormularioAsync(
            int? estudianteId = null,
            int? materiaId = null)
        {
            var estudiantes = await context.Estudiantes
                .AsNoTracking()
                .Include(e => e.Usuario)
                .Include(e => e.Carrera)
                .Where(e =>
                    e.Activo &&
                    e.Usuario != null)
                .OrderBy(e => e.Usuario!.ApellidoPaterno)
                .ThenBy(e => e.Usuario!.Nombres)
                .ToListAsync();

            var materias = await context.Materias
                .AsNoTracking()
                .Include(m => m.Docente)
                    .ThenInclude(d => d!.Usuario)
                .Where(m => m.Activa)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.Estudiantes = new SelectList(
                estudiantes.Select(e => new
                {
                    Id = e.Id,
                    Nombre = $"{e.Usuario!.NombreCompleto} — {e.Carrera!.Nombre}"
                }),
                "Id",
                "Nombre",
                estudianteId);

            ViewBag.Materias = new SelectList(
                materias.Select(m => new
                {
                    Id = m.Id,
                    Nombre = $"{m.Codigo} — {m.Nombre}" +
                             (m.Docente?.Usuario != null
                                 ? $" — Doc. {m.Docente.Usuario.NombreCompleto}"
                                 : "")
                }),
                "Id",
                "Nombre",
                materiaId);
        }
    }
}
