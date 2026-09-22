using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Docente")]
    public class ExamenesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = userManager.GetUserId(User);

            var examenes = await context.Examenes
                .Include(e => e.Materia)
                .Where(e => e.DocenteId == usuarioId)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            return View(examenes);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarMaterias();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Examen model)
        {
            var usuarioId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Challenge();

            var materiaValida = await context.Materias
                .AnyAsync(m =>
                    m.Id == model.MateriaId &&
                    m.DocenteId.HasValue &&
                    m.Docente!.ApplicationUserId == usuarioId);

            if (!materiaValida)
            {
                ModelState.AddModelError(
                    "MateriaId",
                    "La materia seleccionada no pertenece al docente.");
            }

            if (model.FechaFin <= model.FechaInicio)
            {
                ModelState.AddModelError(
                    "FechaFin",
                    "La fecha de finalización debe ser posterior a la fecha de inicio.");
            }

            model.DocenteId = usuarioId;
            ModelState.Remove(nameof(Examen.DocenteId));

            if (!ModelState.IsValid)
            {
                await CargarMaterias();
                return View(model);
            }

            model.Titulo = model.Titulo.Trim();

            if (!string.IsNullOrWhiteSpace(model.Descripcion))
                model.Descripcion = model.Descripcion.Trim();

            model.Estado = EstadoExamen.Borrador;
            model.Activo = true;

            context.Examenes.Add(model);
            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Examen \"{model.Titulo}\" creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuarioId = userManager.GetUserId(User);

            var examen = await context.Examenes
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.DocenteId == usuarioId);

            if (examen == null)
            {
                TempData["Error"] = "Examen no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            await CargarMaterias();

            return View(examen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Examen model)
        {
            var usuarioId = userManager.GetUserId(User);

            var examen = await context.Examenes
                .FirstOrDefaultAsync(e =>
                    e.Id == model.Id &&
                    e.DocenteId == usuarioId);

            if (examen == null)
            {
                TempData["Error"] = "Examen no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var materiaValida = await context.Materias
                .AnyAsync(m =>
                    m.Id == model.MateriaId &&
                    m.DocenteId.HasValue &&
                    m.Docente!.ApplicationUserId == usuarioId);

            if (!materiaValida)
            {
                ModelState.AddModelError(
                    "MateriaId",
                    "La materia seleccionada no pertenece al docente.");
            }

            if (model.FechaFin <= model.FechaInicio)
            {
                ModelState.AddModelError(
                    "FechaFin",
                    "La fecha de finalización debe ser posterior a la fecha de inicio.");
            }

            ModelState.Remove(nameof(Examen.DocenteId));

            if (!ModelState.IsValid)
            {
                await CargarMaterias();
                return View(model);
            }

            examen.MateriaId = model.MateriaId;
            examen.Titulo = model.Titulo.Trim();
            examen.Descripcion =
                string.IsNullOrWhiteSpace(model.Descripcion)
                    ? null
                    : model.Descripcion.Trim();

            examen.FechaInicio = model.FechaInicio;
            examen.FechaFin = model.FechaFin;
            examen.DuracionMinutos = model.DuracionMinutos;
            examen.RequiereVerificacionFacial =
                model.RequiereVerificacionFacial;
            examen.Activo = model.Activo;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Examen \"{examen.Titulo}\" actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publicar(int id)
        {
            var usuarioId = userManager.GetUserId(User);

            var examen = await context.Examenes
                .Include(e => e.Preguntas)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.DocenteId == usuarioId);

            if (examen == null)
            {
                TempData["Error"] = "Examen no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (!examen.Preguntas.Any())
            {
                TempData["Error"] =
                    "No puedes publicar un examen sin preguntas.";

                return RedirectToAction(nameof(Index));
            }

            examen.Estado = EstadoExamen.Publicado;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Examen \"{examen.Titulo}\" publicado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(int id)
        {
            var usuarioId = userManager.GetUserId(User);

            var examen = await context.Examenes
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.DocenteId == usuarioId);

            if (examen == null)
            {
                TempData["Error"] = "Examen no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            examen.Estado = EstadoExamen.Cerrado;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Examen \"{examen.Titulo}\" cerrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var usuarioId = userManager.GetUserId(User);

            var examen = await context.Examenes
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.DocenteId == usuarioId);

            if (examen == null)
            {
                TempData["Error"] = "Examen no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            examen.Activo = !examen.Activo;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Examen \"{examen.Titulo}\" " +
                (examen.Activo
                    ? "activado correctamente."
                    : "desactivado correctamente.");

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarMaterias()
        {
            var usuarioId = userManager.GetUserId(User);

            ViewBag.Materias = await context.Materias
                .Where(m =>
                    m.Activa &&
                    m.DocenteId.HasValue &&
                    m.Docente!.ApplicationUserId == usuarioId)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }
    }
}

