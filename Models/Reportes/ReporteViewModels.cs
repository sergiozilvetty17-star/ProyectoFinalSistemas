namespace EcommerceApp.Models.Reportes
{
    public class ReporteUsuariosViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalDocentes { get; set; }
        public int TotalAdministradores { get; set; }

        public List<ReporteUsuarioItemViewModel> Usuarios { get; set; }
            = new();
    }

    public class ReporteUsuarioItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string CreadoPor { get; set; } = "Registro anterior";

        public string? CreadoPorEmail { get; set; }
    }

    public class ReporteExamenesViewModel
    {
        public int TotalExamenes { get; set; }
        public int Publicados { get; set; }
        public int Borradores { get; set; }
        public int Cerrados { get; set; }

        public int TotalIntentos { get; set; }
        public int EstudiantesEvaluados { get; set; }

        public List<ReporteExamenItemViewModel> Examenes { get; set; }
            = new();
    }

    public class ReporteExamenItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Docente { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int Preguntas { get; set; }
        public int Intentos { get; set; }
        public int Finalizados { get; set; }

        public decimal? Promedio { get; set; }
    }

    public class ReporteRendimientoViewModel
    {
        public decimal PromedioGeneral { get; set; }
        public int TotalEvaluaciones { get; set; }
        public int Aprobados { get; set; }
        public int NoAprobados { get; set; }

        public decimal NotaMaxima { get; set; }
        public decimal NotaMinima { get; set; }

        public List<ReporteRendimientoItemViewModel> Resultados { get; set; }
            = new();
    }

    public class ReporteRendimientoItemViewModel
    {
        public string Estudiante { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Examen { get; set; } = string.Empty;
        public string Docente { get; set; } = string.Empty;

        public decimal Calificacion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class ReporteSeguridadViewModel
    {
        public int TotalEventos { get; set; }
        public int AutenticacionesFaciales { get; set; }
        public int RostrosNoCoinciden { get; set; }
        public int ActividadesSospechosas { get; set; }
        public int IntentosAnulados { get; set; }
        public int CuentasCreadas { get; set; }

        public List<ReporteSeguridadItemViewModel> Eventos { get; set; }
            = new();
    }

    public class ReporteSeguridadItemViewModel
    {
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string IP { get; set; } = "No registrada";
    }

    public class ReporteDocenteViewModel
    {
        public string Docente { get; set; } = string.Empty;

        public int TotalMaterias { get; set; }
        public int TotalExamenes { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalEvaluaciones { get; set; }

        public decimal PromedioGeneral { get; set; }

        public List<ReporteExamenItemViewModel> Examenes { get; set; }
            = new();

        public List<ReporteRendimientoItemViewModel> Resultados { get; set; }
            = new();

        public List<ReporteSeguridadItemViewModel> EventosSeguridad { get; set; }
            = new();
    }
}
