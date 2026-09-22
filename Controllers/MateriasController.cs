using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MateriasController(
        ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var materias = await context.Materias
                .Include(m => m.Docente!)
                .ThenInclude(d => d.Usuario)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(materias);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarDocentes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Materia model)
        {
            if (await context.Materias
                .AnyAsync(m => m.Codigo == model.Codigo))
            {
                ModelState.AddModelError(
                    "Codigo",
                    "Ya existe una materia con ese código.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDocentes();
                return View(model);
            }

            model.Codigo = model.Codigo.Trim().ToUpperInvariant();
            model.Nombre = model.Nombre.Trim();

            if (!string.IsNullOrWhiteSpace(model.Descripcion))
                model.Descripcion = model.Descripcion.Trim();

            context.Materias.Add(model);
            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Materia {model.Nombre} creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var materia = await context.Materias
                .FindAsync(id);

            if (materia == null)
            {
                TempData["Error"] = "Materia no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            await CargarDocentes();
            return View(materia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Materia model)
        {
            var materia = await context.Materias
                .FindAsync(model.Id);

            if (materia == null)
            {
                TempData["Error"] = "Materia no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var codigoExiste = await context.Materias
                .AnyAsync(m =>
                    m.Codigo == model.Codigo &&
                    m.Id != model.Id);

            if (codigoExiste)
            {
                ModelState.AddModelError(
                    "Codigo",
                    "Ya existe otra materia con ese código.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDocentes();
                return View(model);
            }

            materia.Codigo =
                model.Codigo.Trim().ToUpperInvariant();

            materia.Nombre =
                model.Nombre.Trim();

            materia.Descripcion =
                string.IsNullOrWhiteSpace(model.Descripcion)
                    ? null
                    : model.Descripcion.Trim();

            materia.DocenteId = model.DocenteId;
            materia.Activa = model.Activa;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Materia {materia.Nombre} actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var materia = await context.Materias
                .FindAsync(id);

            if (materia == null)
            {
                TempData["Error"] = "Materia no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            materia.Activa = !materia.Activa;

            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Materia {materia.Nombre} " +
                (materia.Activa
                    ? "activada correctamente."
                    : "desactivada correctamente.");

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarDocentes()
        {
            var docentes = await context.Docentes
                .Include(d => d.Usuario)
                .Where(d => d.Activo)
                .OrderBy(d => d.Usuario!.ApellidoPaterno)
                .ThenBy(d => d.Usuario!.Nombres)
                .ToListAsync();

            ViewBag.Docentes = docentes;
        }
    }
}


