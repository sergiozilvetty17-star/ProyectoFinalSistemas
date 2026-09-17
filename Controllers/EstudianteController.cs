using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EstudianteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.Usuario)
                .Include(e => e.Carrera)
                .Include(e => e.Inscripciones)
                    .ThenInclude(i => i.Materia)
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId);

            if (estudiante == null)
            {
                return NotFound("No se encontró el perfil de estudiante.");
            }

            var materiaIds = estudiante.Inscripciones
                .Where(i => i.Activa && i.Materia != null && i.Materia.Activa)
                .Select(i => i.MateriaId)
                .ToList();

            var ahora = DateTime.UtcNow;

            var examenes = await _context.Examenes
                .Include(e => e.Materia)
                .Include(e => e.Preguntas)
                    .ThenInclude(p => p.Opciones)
                .Where(e =>
                    e.Activo &&
                    materiaIds.Contains(e.MateriaId) &&
                    e.Estado == EstadoExamen.Publicado &&
                    e.FechaInicio <= ahora &&
                    e.FechaFin >= ahora)
                .OrderBy(e => e.FechaFin)
                .ToListAsync();

            ViewBag.Estudiante = estudiante;
            ViewBag.Examenes = examenes;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> IniciarExamen(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId);

            if (estudiante == null)
            {
                return NotFound("No se encontró el perfil de estudiante.");
            }

            if (!estudiante.Activo)
            {
                TempData["Error"] = "Tu cuenta de estudiante está inactiva.";
                return RedirectToAction(nameof(Index));
            }

            var examen = await _context.Examenes
                .Include(e => e.Materia)
                .Include(e => e.Preguntas)
                    .ThenInclude(p => p.Opciones)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (examen == null)
            {
                return NotFound("El examen no existe.");
            }

            var inscrito = await _context.Inscripciones
                .AnyAsync(i =>
                    i.EstudianteId == estudiante.Id &&
                    i.MateriaId == examen.MateriaId &&
                    i.Activa);

            if (!inscrito)
            {
                TempData["Error"] = "No estás inscrito en la materia de este examen.";
                return RedirectToAction(nameof(Index));
            }

            var ahora = DateTime.UtcNow;

            if (!examen.Activo)
            {
                TempData["Error"] = "Este examen no está activo.";
                return RedirectToAction(nameof(Index));
            }

            if (examen.Estado != EstadoExamen.Publicado)
            {
                TempData["Error"] = "Este examen todavía no está publicado.";
                return RedirectToAction(nameof(Index));
            }

            if (ahora < examen.FechaInicio)
            {
                TempData["Error"] = "El examen todavía no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            if (ahora > examen.FechaFin)
            {
                TempData["Error"] = "El período para realizar este examen ha terminado.";
                return RedirectToAction(nameof(Index));
            }

            var intentoExistente = await _context.IntentosExamen
                .Where(i =>
                    i.ExamenId == examen.Id &&
                    i.EstudianteId == estudiante.Id &&
                    !i.Anulado)
                .OrderByDescending(i => i.FechaInicio)
                .FirstOrDefaultAsync();

            if (intentoExistente != null)
            {
                if (intentoExistente.Finalizado)
                {
                    TempData["Error"] = "Ya has finalizado este examen.";
                    return RedirectToAction(nameof(Index));
                }

                if (examen.RequiereVerificacionFacial &&
                    !intentoExistente.IdentidadVerificada)
                {
                    return RedirectToAction(
                        nameof(VerificarIdentidad),
                        new { id = intentoExistente.Id });
                }

                return RedirectToAction(
                    nameof(ResolverExamen),
                    new { id = intentoExistente.Id });
            }

            var intento = new IntentoExamen
            {
                ExamenId = examen.Id,
                EstudianteId = estudiante.Id,
                FechaInicio = DateTime.UtcNow,
                IdentidadVerificada = false,
                Finalizado = false,
                Anulado = false
            };

            _context.IntentosExamen.Add(intento);
            await _context.SaveChangesAsync();

            if (examen.RequiereVerificacionFacial)
            {
                return RedirectToAction(
                    nameof(VerificarIdentidad),
                    new { id = intento.Id });
            }

            intento.IdentidadVerificada = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(ResolverExamen),
                new { id = intento.Id });
        }

        [HttpGet]
        public async Task<IActionResult> VerificarIdentidad(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var intento = await _context.IntentosExamen
                .Include(i => i.Examen)
                    .ThenInclude(e => e!.Materia)
                .Include(i => i.Estudiante)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intento == null)
            {
                return NotFound("El intento no existe.");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e =>
                    e.Id == intento.EstudianteId &&
                    e.ApplicationUserId == userId);

            if (estudiante == null)
            {
                return Forbid();
            }

            if (intento.Finalizado || intento.Anulado)
            {
                TempData["Error"] = "Este intento ya no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            return View(intento);
        }

        [HttpGet]
        public async Task<IActionResult> ResolverExamen(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var intento = await _context.IntentosExamen
                .Include(i => i.Examen)
                    .ThenInclude(e => e!.Materia)
                .Include(i => i.Examen)
                    .ThenInclude(e => e!.Preguntas)
                        .ThenInclude(p => p.Opciones)
                .Include(i => i.Respuestas)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intento == null)
            {
                return NotFound("El intento no existe.");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e =>
                    e.Id == intento.EstudianteId &&
                    e.ApplicationUserId == userId);

            if (estudiante == null)
            {
                return Forbid();
            }

            if (intento.Finalizado || intento.Anulado)
            {
                TempData["Error"] = "Este intento ya no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            if (intento.Examen == null)
            {
                return NotFound("No se encontró el examen.");
            }

            if (intento.Examen.RequiereVerificacionFacial &&
                !intento.IdentidadVerificada)
            {
                return RedirectToAction(
                    nameof(VerificarIdentidad),
                    new { id = intento.Id });
            }

            var ahora = DateTime.UtcNow;

            if (ahora > intento.Examen.FechaFin)
            {
                TempData["Error"] = "El período del examen ha terminado.";
                return RedirectToAction(nameof(Index));
            }

            return View(intento);
        }
    }
}
