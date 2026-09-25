using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;

namespace EcommerceApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<Docente> Docentes => Set<Docente>();
        public DbSet<Carrera> Carreras => Set<Carrera>();
        public DbSet<Materia> Materias => Set<Materia>();
        public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
        public DbSet<RegistroFacial> RegistrosFaciales => Set<RegistroFacial>();
        public DbSet<Autenticacion> Autenticaciones => Set<Autenticacion>();
        public DbSet<EventoSeguridad> EventosSeguridad => Set<EventoSeguridad>();
        public DbSet<Examen> Examenes => Set<Examen>();
        public DbSet<Pregunta> Preguntas => Set<Pregunta>();
        public DbSet<Opcion> Opciones => Set<Opcion>();
        public DbSet<IntentoExamen> IntentosExamen => Set<IntentoExamen>();
        public DbSet<Respuesta> Respuestas => Set<Respuesta>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.CedulaIdentidad)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.CreatedByUser)
                .WithMany(u => u.CreatedUsers)
                .HasForeignKey(u => u.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Estudiante>()
                .HasIndex(e => e.ApplicationUserId)
                .IsUnique();

            builder.Entity<Docente>()
                .HasIndex(d => d.ApplicationUserId)
                .IsUnique();

            builder.Entity<Estudiante>()
                .HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Estudiante>(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Docente>()
                .HasOne(d => d.Usuario)
                .WithOne()
                .HasForeignKey<Docente>(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Carrera>()
                .HasMany(c => c.Estudiantes)
                .WithOne(e => e.Carrera)
                .HasForeignKey(e => e.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Docente>()
                .HasMany(d => d.Materias)
                .WithOne(m => m.Docente)
                .HasForeignKey(m => m.DocenteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Estudiante>()
                .HasMany(e => e.Inscripciones)
                .WithOne(i => i.Estudiante)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Materia>()
                .HasMany(m => m.Inscripciones)
                .WithOne(i => i.Materia)
                .HasForeignKey(i => i.MateriaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RegistroFacial>()
                .HasOne(r => r.Estudiante)
                .WithOne()
                .HasForeignKey<RegistroFacial>(r => r.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany<Autenticacion>()
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Materia>()
                .HasMany(m => m.Inscripciones)
                .WithOne(i => i.Materia)
                .HasForeignKey(i => i.MateriaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Materia>()
                .HasMany<Examen>()
                .WithOne(e => e.Materia)
                .HasForeignKey(e => e.MateriaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany<Examen>()
                .WithOne(e => e.Docente)
                .HasForeignKey(e => e.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Examen>()
                .HasMany(e => e.Preguntas)
                .WithOne(p => p.Examen)
                .HasForeignKey(p => p.ExamenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Pregunta>()
                .HasMany(p => p.Opciones)
                .WithOne(o => o.Pregunta)
                .HasForeignKey(o => o.PreguntaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Estudiante>()
                .HasMany<IntentoExamen>()
                .WithOne(i => i.Estudiante)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Examen>()
                .HasMany(e => e.Intentos)
                .WithOne(i => i.Examen)
                .HasForeignKey(i => i.ExamenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<IntentoExamen>()
                .HasMany(i => i.Respuestas)
                .WithOne(r => r.IntentoExamen)
                .HasForeignKey(r => r.IntentoExamenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Pregunta>()
                .HasMany(p => p.Respuestas)
                .WithOne(r => r.Pregunta)
                .HasForeignKey(r => r.PreguntaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Opcion>()
                .HasMany(o => o.Respuestas)
                .WithOne(r => r.Opcion)
                .HasForeignKey(r => r.OpcionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<IntentoExamen>()
                .HasMany(i => i.EventosSeguridad)
                .WithOne(e => e.IntentoExamen)
                .HasForeignKey(e => e.IntentoExamenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany<EventoSeguridad>()
                .WithOne(e => e.Usuario)
                .HasForeignKey(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Estudiante>()
                .HasIndex(e => e.CarreraId);

            builder.Entity<Materia>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            builder.Entity<Inscripcion>()
                .HasIndex(i => new
                {
                    i.EstudianteId,
                    i.MateriaId
                })
                .IsUnique();

            builder.Entity<RegistroFacial>()
                .HasIndex(r => r.EstudianteId)
                .IsUnique();

            builder.Entity<Pregunta>()
                .Property(p => p.Puntaje)
                .HasPrecision(10, 2);

            builder.Entity<Respuesta>()
                .Property(r => r.PuntajeObtenido)
                .HasPrecision(10, 2);

            builder.Entity<IntentoExamen>()
                .Property(i => i.Calificacion)
                .HasPrecision(10, 2);
        }
    }
}

