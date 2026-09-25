using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Docente")]
    public class DocenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocenteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> MisMaterias()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d =>
                    d.ApplicationUserId == userId);

            if (docente == null)
            {
                return NotFound("No se encontró el perfil de docente.");
            }

            var materias = await _context.Materias
                .Include(m => m.Inscripciones)
                .Where(m =>
                    m.DocenteId == docente.Id &&
                    m.Activa)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.Materias = materias;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Resultados()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var resultados = await _context.IntentosExamen
                .Include(i => i.Examen)
                    .ThenInclude(e => e!.Materia)
                .Include(i => i.Estudiante)
                    .ThenInclude(e => e!.Usuario)
                .Where(i =>
                    i.Examen != null &&
                    i.Examen.DocenteId == userId &&
                    i.Finalizado &&
                    !i.Anulado &&
                    i.Calificacion.HasValue)
                .OrderByDescending(i => i.FechaFin)
                .ToListAsync();

            ViewBag.Resultados = resultados;

            return View();
        }
    }
}
