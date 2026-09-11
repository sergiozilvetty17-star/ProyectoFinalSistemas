using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Docente")]
    public class PreguntasController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        public async Task<IActionResult> Index(int examenId)
        {
            var examen = await ObtenerExamenDelDocenteAsync(examenId);

            if (examen == null)
                return NotFound();

            var preguntas = await context.Preguntas
                .Include(p => p.Opciones)
                .Where(p => p.ExamenId == examenId)
                .OrderBy(p => p.Orden)
                .ThenBy(p => p.Id)
                .ToListAsync();

            ViewBag.Examen = examen;

            return View(preguntas);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int examenId)
        {
            var examen = await ObtenerExamenDelDocenteAsync(examenId);

            if (examen == null)
                return NotFound();

            var siguienteOrden = await context.Preguntas
                .Where(p => p.ExamenId == examenId)
                .Select(p => (int?)p.Orden)
                .MaxAsync() ?? 0;

            ViewBag.Examen = examen;

            return View(new PreguntaCreateViewModel
            {
                ExamenId = examenId,
                Orden = siguienteOrden + 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PreguntaCreateViewModel model)
        {
            var examen = await ObtenerExamenDelDocenteAsync(model.ExamenId);

            if (examen == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Examen = examen;
                return View(model);
            }

            var pregunta = new Pregunta
            {
                ExamenId = model.ExamenId,
                Enunciado = model.Enunciado.Trim(),
                Tipo = model.Tipo,
                Puntaje = model.Puntaje,
                Orden = model.Orden
            };

            context.Preguntas.Add(pregunta);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Pregunta creada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { examenId = model.ExamenId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var pregunta = await context.Preguntas
                .Include(p => p.Examen)
                    .ThenInclude(e => e!.Materia)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta?.Examen == null)
                return NotFound();

            var examen = await ObtenerExamenDelDocenteAsync(
                pregunta.ExamenId);

            if (examen == null)
                return NotFound();

            ViewBag.Examen = examen;

            return View(new PreguntaCreateViewModel
            {
                ExamenId = pregunta.ExamenId,
                Enunciado = pregunta.Enunciado,
                Tipo = pregunta.Tipo,
                Puntaje = pregunta.Puntaje,
                Orden = pregunta.Orden
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PreguntaCreateViewModel model)
        {
            var pregunta = await context.Preguntas
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            var examen = await ObtenerExamenDelDocenteAsync(
                pregunta.ExamenId);

            if (examen == null)
                return NotFound();

            if (pregunta.ExamenId != model.ExamenId)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El examen no coincide con la pregunta.");

                model.ExamenId = pregunta.ExamenId;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Examen = examen;
                return View(model);
            }

            pregunta.Enunciado = model.Enunciado.Trim();
            pregunta.Tipo = model.Tipo;
            pregunta.Puntaje = model.Puntaje;
            pregunta.Orden = model.Orden;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Pregunta actualizada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { examenId = pregunta.ExamenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var pregunta = await context.Preguntas
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
            {
                TempData["Error"] =
                    "La pregunta no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new { examenId = 0 });
            }

            var examen = await ObtenerExamenDelDocenteAsync(
                pregunta.ExamenId);

            if (examen == null)
                return NotFound();

            var examenId = pregunta.ExamenId;

            context.Preguntas.Remove(pregunta);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Pregunta eliminada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { examenId });
        }

        private async Task<Examen?> ObtenerExamenDelDocenteAsync(
            int examenId)
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await context.Examenes
                .Include(e => e.Materia)
                .FirstOrDefaultAsync(e =>
                    e.Id == examenId &&
                    e.DocenteId == userId);
        }
    }
}
