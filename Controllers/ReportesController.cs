using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Models.Reportes;
using EcommerceApp.Services.Reportes;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador,Docente")]
    public class ReportesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Rol = "Administrador";
                return View("Index");
            }

            var usuarioId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
                return Forbid();

            ViewBag.Rol = "Docente";

            return View("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await context.Users
                .Include(u => u.CreatedByUser)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var resultado = new ReporteUsuariosViewModel
            {
                TotalUsuarios = usuarios.Count
            };

            foreach (var usuario in usuarios)
            {
                var roles = await userManager.GetRolesAsync(usuario);
                var rol = roles.FirstOrDefault() ?? "Sin rol";

                if (rol == "Estudiante")
                    resultado.TotalEstudiantes++;

                else if (rol == "Docente")
                    resultado.TotalDocentes++;

                else if (rol == "Administrador")
                    resultado.TotalAdministradores++;

                var activo = true;

                if (rol == "Estudiante")
                {
                    var estudiante = await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == usuario.Id);

                    activo = estudiante?.Activo ?? false;
                }
                else if (rol == "Docente")
                {
                    var docente = await context.Docentes
                        .FirstOrDefaultAsync(d =>
                            d.ApplicationUserId == usuario.Id);

                    activo = docente?.Activo ?? false;
                }

                resultado.Usuarios.Add(new ReporteUsuarioItemViewModel
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email ?? string.Empty,
                    Rol = rol,
                    Estado = activo ? "Activo" : "Inactivo",
                    FechaCreacion = usuario.CreatedAt,
                    CreadoPor = usuario.CreatedByUser?.NombreCompleto
                        ?? "Registro anterior",
                    CreadoPorEmail = usuario.CreatedByUser?.Email
                });
            }

            return View(resultado);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Examenes()
        {
            var examenes = await context.Examenes
                .Include(e => e.Materia)
                .Include(e => e.Docente)
                .Include(e => e.Preguntas)
                .Include(e => e.Intentos)
                .ToListAsync();

            var resultado = new ReporteExamenesViewModel
            {
                TotalExamenes = examenes.Count,
                Publicados = examenes.Count(e =>
                    e.Estado == EstadoExamen.Publicado),
                Borradores = examenes.Count(e =>
                    e.Estado == EstadoExamen.Borrador),
                Cerrados = examenes.Count(e =>
                    e.Estado == EstadoExamen.Cerrado),
                TotalIntentos = examenes
                    .SelectMany(e => e.Intentos)
                    .Count(),
                EstudiantesEvaluados = examenes
                    .SelectMany(e => e.Intentos)
                    .Where(i => i.Finalizado && !i.Anulado)
                    .Select(i => i.EstudianteId)
                    .Distinct()
                    .Count()
            };

            foreach (var examen in examenes.OrderByDescending(e => e.FechaInicio))
            {
                var intentosValidos = examen.Intentos
                    .Where(i => !i.Anulado)
                    .ToList();

                resultado.Examenes.Add(new ReporteExamenItemViewModel
                {
                    Id = examen.Id,
                    Titulo = examen.Titulo,
                    Materia = examen.Materia?.Nombre ?? "Sin materia",
                    Docente = examen.Docente?.NombreCompleto ?? "Sin docente",
                    FechaInicio = examen.FechaInicio,
                    FechaFin = examen.FechaFin,
                    Estado = examen.Estado.ToString(),
                    Preguntas = examen.Preguntas.Count,
                    Intentos = intentosValidos.Count,
                    Finalizados = intentosValidos.Count(i => i.Finalizado),
                    Promedio = intentosValidos
                        .Where(i => i.Finalizado && i.Calificacion.HasValue)
                        .Select(i => i.Calificacion!.Value)
                        .DefaultIfEmpty()
                        .Average()
                });
            }

            return View(resultado);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Rendimiento()
        {
            var intentos = await context.IntentosExamen
                .Include(i => i.Examen!)
                    .ThenInclude(e => e.Materia)
                .Include(i => i.Examen!)
                    .ThenInclude(e => e.Docente)
                .Include(i => i.Estudiante!)
                    .ThenInclude(e => e.Usuario)
                .Include(i => i.Estudiante!)
                    .ThenInclude(e => e.Carrera)
                .Where(i =>
                    i.Finalizado &&
                    !i.Anulado &&
                    i.Calificacion.HasValue)
                .OrderByDescending(i => i.FechaFin)
                .ToListAsync();

            return View(CrearReporteRendimiento(intentos));
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Seguridad()
        {
            var eventos = await context.EventosSeguridad
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.FechaHora)
                .ToListAsync();

            var resultado = CrearReporteSeguridad(eventos);

            resultado.IntentosAnulados =
                await context.IntentosExamen
                    .CountAsync(i => i.Anulado);

            return View(resultado);
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> MisExamenes()
        {
            var usuarioId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
                return Forbid();

            var examenes = await context.Examenes
                .Where(e => e.DocenteId == usuarioId)
                .Include(e => e.Materia)
                .Include(e => e.Preguntas)
                .Include(e => e.Intentos)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            var resultado = new ReporteExamenesViewModel
            {
                TotalExamenes = examenes.Count,
                Publicados = examenes.Count(e =>
                    e.Estado == EstadoExamen.Publicado),
                Borradores = examenes.Count(e =>
                    e.Estado == EstadoExamen.Borrador),
                Cerrados = examenes.Count(e =>
                    e.Estado == EstadoExamen.Cerrado),
                TotalIntentos = examenes
                    .SelectMany(e => e.Intentos)
                    .Count(),
                EstudiantesEvaluados = examenes
                    .SelectMany(e => e.Intentos)
                    .Where(i => i.Finalizado && !i.Anulado)
                    .Select(i => i.EstudianteId)
                    .Distinct()
                    .Count()
            };

            foreach (var examen in examenes)
            {
                var intentos = examen.Intentos
                    .Where(i => !i.Anulado)
                    .ToList();

                resultado.Examenes.Add(new ReporteExamenItemViewModel
                {
                    Id = examen.Id,
                    Titulo = examen.Titulo,
                    Materia = examen.Materia?.Nombre ?? "Sin materia",
                    Docente = User.Identity?.Name ?? "Docente",
                    FechaInicio = examen.FechaInicio,
                    FechaFin = examen.FechaFin,
                    Estado = examen.Estado.ToString(),
                    Preguntas = examen.Preguntas.Count,
                    Intentos = intentos.Count,
                    Finalizados = intentos.Count(i => i.Finalizado),
                    Promedio = intentos
                        .Where(i => i.Finalizado && i.Calificacion.HasValue)
                        .Select(i => i.Calificacion!.Value)
                        .DefaultIfEmpty()
                        .Average()
                });
            }

            return View("MisExamenes", resultado);
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> MiRendimiento()
        {
            var usuarioId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
                return Forbid();

            var intentos = await context.IntentosExamen
                .Include(i => i.Examen!)
                    .ThenInclude(e => e.Materia)
                .Include(i => i.Examen!)
                    .ThenInclude(e => e.Docente)
                .Include(i => i.Estudiante!)
                    .ThenInclude(e => e.Usuario)
                .Include(i => i.Estudiante!)
                    .ThenInclude(e => e.Carrera)
                .Where(i =>
                    i.Examen!.DocenteId == usuarioId &&
                    i.Finalizado &&
                    !i.Anulado &&
                    i.Calificacion.HasValue)
                .OrderByDescending(i => i.FechaFin)
                .ToListAsync();

            return View("MiRendimiento", CrearReporteRendimiento(intentos));
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> MiSeguridad()
        {
            var usuarioId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
                return Forbid();

            var eventos = await context.EventosSeguridad
                .Include(e => e.Usuario)
                .Include(e => e.IntentoExamen!)
                    .ThenInclude(i => i.Examen)
                .Where(e =>
                    e.IntentoExamen != null &&
                    e.IntentoExamen.Examen!.DocenteId == usuarioId)
                .OrderByDescending(e => e.FechaHora)
                .ToListAsync();

            return View("MiSeguridad", CrearReporteSeguridad(eventos));
        }

        [HttpGet]
        public async Task<IActionResult> UsuariosExcel()
        {
            if (!User.IsInRole("Administrador"))
                return Forbid();

            var reporte = await ConstruirUsuarios();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Usuarios");

            sheet.Cell(1, 1).Value = "REPORTE DE USUARIOS";
            sheet.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            var headers = new[]
            {
                "Nombre completo",
                "Correo",
                "Rol",
                "Estado",
                "Fecha de creación",
                "Creado por",
                "Correo del creador"
            };

            for (var i = 0; i < headers.Length; i++)
                sheet.Cell(4, i + 1).Value = headers[i];

            for (var row = 0; row < reporte.Usuarios.Count; row++)
            {
                var item = reporte.Usuarios[row];
                var excelRow = row + 5;

                sheet.Cell(excelRow, 1).Value = item.NombreCompleto;
                sheet.Cell(excelRow, 2).Value = item.Email;
                sheet.Cell(excelRow, 3).Value = item.Rol;
                sheet.Cell(excelRow, 4).Value = item.Estado;
                sheet.Cell(excelRow, 5).Value = item.FechaCreacion.ToLocalTime();
                sheet.Cell(excelRow, 6).Value = item.CreadoPor;
                sheet.Cell(excelRow, 7).Value = item.CreadoPorEmail ?? "";
            }

            sheet.Columns().AdjustToContents();
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(4).Style.Font.Bold = true;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> UsuariosPdf()
        {
            if (!User.IsInRole("Administrador"))
                return Forbid();

            var reporte = await ConstruirUsuarios();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    PdfReportStyle.ConfigurePage(
                        page,
                        "Reporte de usuarios",
                        "Auditoría de cuentas y trazabilidad de creación");

                    page.Content()
                        .PaddingTop(12)
                        .Column(content =>
                        {
                            content.Spacing(10);

                            content.Item().Row(row =>
                            {
                                var card1 = row.RelativeItem();
                                PdfReportStyle.StatCard(
                                    card1,
                                    "Usuarios registrados",
                                    reporte.TotalUsuarios.ToString(),
                                    PdfReportStyle.Blue);

                                row.ConstantItem(12);

                                var card2 = row.RelativeItem();
                                PdfReportStyle.StatCard(
                                    card2,
                                    "Estudiantes",
                                    reporte.TotalEstudiantes.ToString(),
                                    PdfReportStyle.Green);

                                row.ConstantItem(12);

                                var card3 = row.RelativeItem();
                                PdfReportStyle.StatCard(
                                    card3,
                                    "Docentes",
                                    reporte.TotalDocentes.ToString(),
                                    PdfReportStyle.BlueLight);

                                row.ConstantItem(12);

                                var card4 = row.RelativeItem();
                                PdfReportStyle.StatCard(
                                    card4,
                                    "Administradores",
                                    reporte.TotalAdministradores.ToString(),
                                    PdfReportStyle.Yellow);
                            });

                            content.Item().Element(c =>
                            {
                                PdfReportStyle.SectionTitle(
                                    c,
                                    "Trazabilidad de cuentas");
                            });

                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.4f);
                                    columns.RelativeColumn(2.3f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(2.4f);
                                });

                                table.Header(header =>
                                {
                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "USUARIO");

                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "CORREO");

                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "ROL");

                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "ESTADO");

                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "CREACIÓN");

                                    PdfReportStyle.TableHeader(
                                        header.Cell(),
                                        "CREADO POR");
                                });

                                for (var index = 0;
                                     index < reporte.Usuarios.Count;
                                     index++)
                                {
                                    var item = reporte.Usuarios[index];
                                    var alternate = index % 2 == 1;

                                    PdfReportStyle.TableCell(
                                        table.Cell(),
                                        item.NombreCompleto,
                                        alternate);

                                    PdfReportStyle.TableCell(
                                        table.Cell(),
                                        item.Email,
                                        alternate);

                                    PdfReportStyle.TableCell(
                                        table.Cell(),
                                        item.Rol,
                                        alternate);

                                    var estadoCell = table.Cell();

                                    estadoCell.Background(
                                        alternate
                                            ? PdfReportStyle.Background
                                            : PdfReportStyle.White);

                                    estadoCell.BorderBottom(1);
                                    estadoCell.BorderColor(
                                        PdfReportStyle.LightGray);
                                    estadoCell.Padding(6);

                                    estadoCell.Text(item.Estado)
                                        .FontSize(7.5f)
                                        .Bold()
                                        .FontColor(
                                            PdfReportStyle.EstadoColor(
                                                item.Estado));

                                    PdfReportStyle.TableCell(
                                        table.Cell(),
                                        item.FechaCreacion
                                            .ToLocalTime()
                                            .ToString("dd/MM/yyyy HH:mm"),
                                        alternate);

                                    PdfReportStyle.TableCell(
                                        table.Cell(),
                                        item.CreadoPor,
                                        alternate);
                                }
                            });

                            content.Item()
                                .Text(
                                    "Nota: las cuentas históricas que no disponen " +
                                    "de información de auditoría se identifican " +
                                    "como «Registro anterior».")
                                .FontSize(7)
                                .Italic()
                                .FontColor(PdfReportStyle.Gray);
                        });
                });
            });

            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Reporte_Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        private async Task<ReporteUsuariosViewModel> ConstruirUsuarios()
        {
            var usuarios = await context.Users
                .Include(u => u.CreatedByUser)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var reporte = new ReporteUsuariosViewModel
            {
                TotalUsuarios = usuarios.Count
            };

            foreach (var usuario in usuarios)
            {
                var roles = await userManager.GetRolesAsync(usuario);
                var rol = roles.FirstOrDefault() ?? "Sin rol";

                var activo = true;

                if (rol == "Estudiante")
                {
                    var estudiante = await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == usuario.Id);

                    activo = estudiante?.Activo ?? false;
                    reporte.TotalEstudiantes++;
                }
                else if (rol == "Docente")
                {
                    var docente = await context.Docentes
                        .FirstOrDefaultAsync(d =>
                            d.ApplicationUserId == usuario.Id);

                    activo = docente?.Activo ?? false;
                    reporte.TotalDocentes++;
                }
                else if (rol == "Administrador")
                {
                    reporte.TotalAdministradores++;
                }

                reporte.Usuarios.Add(new ReporteUsuarioItemViewModel
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email ?? "",
                    Rol = rol,
                    Estado = activo ? "Activo" : "Inactivo",
                    FechaCreacion = usuario.CreatedAt,
                    CreadoPor = usuario.CreatedByUser?.NombreCompleto
                        ?? "Registro anterior",
                    CreadoPorEmail = usuario.CreatedByUser?.Email
                });
            }

            return reporte;
        }

        private static ReporteRendimientoViewModel CrearReporteRendimiento(
            List<IntentoExamen> intentos)
        {
            var calificaciones = intentos
                .Where(i => i.Calificacion.HasValue)
                .Select(i => i.Calificacion!.Value)
                .ToList();

            var reporte = new ReporteRendimientoViewModel
            {
                TotalEvaluaciones = intentos.Count,
                PromedioGeneral = calificaciones.Count > 0
                    ? calificaciones.Average()
                    : 0,
                NotaMaxima = calificaciones.Count > 0
                    ? calificaciones.Max()
                    : 0,
                NotaMinima = calificaciones.Count > 0
                    ? calificaciones.Min()
                    : 0
            };

            reporte.Aprobados = calificaciones.Count(n => n >= 51);
            reporte.NoAprobados = calificaciones.Count(n => n < 51);

            reporte.Resultados = intentos.Select(i =>
                new ReporteRendimientoItemViewModel
                {
                    Estudiante = i.Estudiante?.Usuario?.NombreCompleto
                        ?? "Estudiante",
                    Carrera = i.Estudiante?.Carrera?.Nombre
                        ?? "Sin carrera",
                    Materia = i.Examen?.Materia?.Nombre
                        ?? "Sin materia",
                    Examen = i.Examen?.Titulo
                        ?? "Sin examen",
                    Docente = i.Examen?.Docente?.NombreCompleto
                        ?? "Sin docente",
                    Calificacion = i.Calificacion ?? 0,
                    Fecha = i.FechaFin ?? i.FechaInicio
                }).ToList();

            return reporte;
        }

        private static ReporteSeguridadViewModel CrearReporteSeguridad(
            List<EventoSeguridad> eventos)
        {
            return new ReporteSeguridadViewModel
            {
                TotalEventos = eventos.Count,
                AutenticacionesFaciales = eventos.Count(e =>
                    e.Tipo == TipoEvento.AutenticacionFacial),
                RostrosNoCoinciden = eventos.Count(e =>
                    e.Tipo == TipoEvento.RostroNoCoincide),
                ActividadesSospechosas = eventos.Count(e =>
                    e.Tipo == TipoEvento.ActividadSospechosa),
                CuentasCreadas = eventos.Count(e =>
                    e.Tipo == TipoEvento.CuentaCreada),
                Eventos = eventos.Select(e =>
                    new ReporteSeguridadItemViewModel
                    {
                        Fecha = e.FechaHora,
                        Usuario = e.Usuario?.NombreCompleto
                            ?? "Usuario",
                        Tipo = e.Tipo.ToString(),
                        Descripcion = e.Descripcion ?? "",
                        IP = e.DireccionIP ?? "No registrada"
                    }).ToList()
            };
        }
    }
}




