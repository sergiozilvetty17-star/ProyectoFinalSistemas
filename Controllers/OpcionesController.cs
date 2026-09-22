using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Docente")]
    public class OpcionesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int preguntaId)
        {
            var pregunta = await ObtenerPreguntaDelDocenteAsync(preguntaId);

            if (pregunta == null)
                return NotFound();

            var opciones = await context.Opciones
                .Where(o => o.PreguntaId == preguntaId)
                .OrderBy(o => o.Orden)
                .ThenBy(o => o.Id)
                .ToListAsync();

            ViewBag.Pregunta = pregunta;

            return View(opciones);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int preguntaId)
        {
            var pregunta = await ObtenerPreguntaDelDocenteAsync(preguntaId);

            if (pregunta == null)
                return NotFound();

            if (pregunta.Tipo == TipoPregunta.RespuestaAbierta)
            {
                TempData["Error"] =
                    "Las preguntas de respuesta abierta no utilizan opciones.";

                return RedirectToAction(
                    "Index",
                    "Preguntas",
                    new { examenId = pregunta.ExamenId });
            }

            var siguienteOrden = await context.Opciones
                .Where(o => o.PreguntaId == preguntaId)
                .Select(o => (int?)o.Orden)
                .MaxAsync() ?? 0;

            ViewBag.Pregunta = pregunta;

            return View(new OpcionCreateViewModel
            {
                PreguntaId = preguntaId,
                Orden = siguienteOrden + 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OpcionCreateViewModel model)
        {
            var pregunta = await ObtenerPreguntaDelDocenteAsync(
                model.PreguntaId);

            if (pregunta == null)
                return NotFound();

            if (pregunta.Tipo == TipoPregunta.RespuestaAbierta)
            {
                TempData["Error"] =
                    "Las preguntas de respuesta abierta no utilizan opciones.";

                return RedirectToAction(
                    "Index",
                    "Preguntas",
                    new { examenId = pregunta.ExamenId });
            }

            if (pregunta.Tipo == TipoPregunta.VerdaderoFalso)
            {
                var cantidad = await context.Opciones
                    .CountAsync(o => o.PreguntaId == pregunta.Id);

                if (cantidad >= 2)
                {
                    TempData["Error"] =
                        "Una pregunta de Verdadero/Falso solo puede tener dos opciones.";

                    return RedirectToAction(
                        nameof(Index),
                        new { preguntaId = pregunta.Id });
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pregunta = pregunta;
                return View(model);
            }

            var opcion = new Opcion
            {
                PreguntaId = model.PreguntaId,
                Texto = model.Texto.Trim(),
                EsCorrecta = model.EsCorrecta,
                Orden = model.Orden
            };

            context.Opciones.Add(opcion);

            await context.SaveChangesAsync();

            if (model.EsCorrecta)
            {
                await DesmarcarOtrasCorrectasAsync(
                    opcion.PreguntaId,
                    opcion.Id);
            }

            TempData["Success"] =
                "Opción creada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { preguntaId = model.PreguntaId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var opcion = await context.Opciones
                .Include(o => o.Pregunta)
                    .ThenInclude(p => p!.Examen)
                        .ThenInclude(e => e!.Materia)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opcion?.Pregunta == null)
                return NotFound();

            var pregunta = await ObtenerPreguntaDelDocenteAsync(
                opcion.PreguntaId);

            if (pregunta == null)
                return NotFound();

            ViewBag.Pregunta = pregunta;

            return View(new OpcionCreateViewModel
            {
                PreguntaId = opcion.PreguntaId,
                Texto = opcion.Texto,
                EsCorrecta = opcion.EsCorrecta,
                Orden = opcion.Orden
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            OpcionCreateViewModel model)
        {
            var opcion = await context.Opciones
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opcion == null)
                return NotFound();

            var pregunta = await ObtenerPreguntaDelDocenteAsync(
                opcion.PreguntaId);

            if (pregunta == null)
                return NotFound();

            if (opcion.PreguntaId != model.PreguntaId)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La pregunta no coincide con la opción.");

                model.PreguntaId = opcion.PreguntaId;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pregunta = pregunta;
                return View(model);
            }

            opcion.Texto = model.Texto.Trim();
            opcion.EsCorrecta = model.EsCorrecta;
            opcion.Orden = model.Orden;

            await context.SaveChangesAsync();

            if (opcion.EsCorrecta)
            {
                await DesmarcarOtrasCorrectasAsync(
                    opcion.PreguntaId,
                    opcion.Id);
            }

            TempData["Success"] =
                "Opción actualizada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { preguntaId = opcion.PreguntaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var opcion = await context.Opciones
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opcion == null)
            {
                TempData["Error"] =
                    "La opción no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new { preguntaId = 0 });
            }

            var pregunta = await ObtenerPreguntaDelDocenteAsync(
                opcion.PreguntaId);

            if (pregunta == null)
                return NotFound();

            var preguntaId = opcion.PreguntaId;

            context.Opciones.Remove(opcion);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Opción eliminada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { preguntaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarCorrecta(int id)
        {
            var opcion = await context.Opciones
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opcion == null)
                return NotFound();

            var pregunta = await ObtenerPreguntaDelDocenteAsync(
                opcion.PreguntaId);

            if (pregunta == null)
                return NotFound();

            await DesmarcarOtrasCorrectasAsync(
                opcion.PreguntaId,
                opcion.Id);

            opcion.EsCorrecta = true;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Respuesta correcta actualizada.";

            return RedirectToAction(
                nameof(Index),
                new { preguntaId = opcion.PreguntaId });
        }

        private async Task DesmarcarOtrasCorrectasAsync(
            int preguntaId,
            int opcionCorrectaId)
        {
            var otrasOpciones = await context.Opciones
                .Where(o =>
                    o.PreguntaId == preguntaId &&
                    o.Id != opcionCorrectaId &&
                    o.EsCorrecta)
                .ToListAsync();

            foreach (var opcion in otrasOpciones)
            {
                opcion.EsCorrecta = false;
            }

            await context.SaveChangesAsync();
        }

        private async Task<Pregunta?> ObtenerPreguntaDelDocenteAsync(
            int preguntaId)
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await context.Preguntas
                .Include(p => p.Examen)
                    .ThenInclude(e => e!.Materia)
                .FirstOrDefaultAsync(p =>
                    p.Id == preguntaId &&
                    p.Examen != null &&
                    p.Examen.DocenteId == userId);
        }
    }
}
