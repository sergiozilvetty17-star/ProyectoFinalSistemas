using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;

namespace EcommerceApp.Data.Seed
{
    public static class DemoDataSeeder
    {
        private const string DemoPrefix = "DEMO | ";
        private const string DemoEmailDomain = "@examsecure.test";
        private const string DemoPassword = "Demo1234";

        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("  EXAMSECURE - CARGA DE DATOS DEMO");
            Console.WriteLine("==============================================");

            var carreras = await context.Carreras
                .Where(c => c.Activa)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (carreras.Count == 0)
            {
                throw new InvalidOperationException(
                    "No existen carreras activas.");
            }

            var materias = await context.Materias
                .Include(m => m.Docente)
                    .ThenInclude(d => d!.Usuario)
                .Where(m =>
                    m.Activa &&
                    m.DocenteId.HasValue &&
                    m.Docente != null &&
                    m.Docente.Activo &&
                    m.Docente.Usuario != null)
                .OrderBy(m => m.Id)
                .ToListAsync();

            if (materias.Count == 0)
            {
                throw new InvalidOperationException(
                    "No existen materias activas con docentes asignados. " +
                    "Primero crea/asigna docentes a las materias.");
            }

            Console.WriteLine(
                $"Carreras encontradas: {carreras.Count}");

            Console.WriteLine(
                $"Materias con docente: {materias.Count}");

            var estudiantes =
                await CrearEstudiantesAsync(
                    context,
                    userManager,
                    carreras);

            await CrearInscripcionesAsync(
                context,
                estudiantes,
                materias);

            await CrearExamenesAsync(
                context,
                materias,
                estudiantes);

            await CrearIntentosDemoAsync(
                context,
                estudiantes);

            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("  DATOS DEMO CARGADOS CORRECTAMENTE");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine(
                $"Estudiantes demo: {estudiantes.Count}");
            Console.WriteLine(
                "Contraseña demo: Demo1234");
            Console.WriteLine();
        }

        private static async Task<List<Estudiante>>
            CrearEstudiantesAsync(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                List<Carrera> carreras)
        {
            var estudiantes =
                new List<Estudiante>();

            var nombres = new[]
            {
                "Sergio",
                "Daniel",
                "Andrés",
                "Carlos",
                "Luis",
                "Mateo",
                "Gabriel",
                "Diego",
                "Jorge",
                "Miguel",
                "Alejandro",
                "Fernando",
                "Ricardo",
                "Marco",
                "Pablo",
                "José",
                "Valeria",
                "Camila",
                "María",
                "Andrea",
                "Daniela",
                "Sofía",
                "Gabriela",
                "Natalia",
                "Lucía",
                "Paola",
                "Carla",
                "Fernanda",
                "Julieta",
                "Victoria",
                "Mariana",
                "Nicole",
                "Laura",
                "Ana",
                "Isabela",
                "Renata",
                "Mónica",
                "Patricia",
                "Alejandra",
                "Verónica"
            };

            var apellidos = new[]
            {
                "Zilvetty",
                "García",
                "Mamani",
                "Fernández",
                "Rojas",
                "Pérez",
                "Quispe",
                "Torrez",
                "Vargas",
                "Flores"
            };

            for (var i = 0; i < 40; i++)
            {
                var numero = i + 1;

                var email =
                    $"demo.estudiante{numero:00}" +
                    DemoEmailDomain;

                var user =
                    await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        Nombres = nombres[i],
                        ApellidoPaterno =
                            apellidos[i % apellidos.Length],
                        ApellidoMaterno =
                            apellidos[(i + 3) % apellidos.Length],
                        CedulaIdentidad =
                            $"DEMO{numero:0000}",
                        Telefono =
                            $"700{numero:000000}",
                        CreatedAt =
                            DateTime.UtcNow
                    };

                    var result =
                        await userManager.CreateAsync(
                            user,
                            DemoPassword);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            " | ",
                            result.Errors.Select(
                                e => e.Description));

                        throw new InvalidOperationException(
                            $"No se pudo crear {email}: {errors}");
                    }

                    Console.WriteLine(
                        $"  Usuario creado: {email}");
                }

                if (!await userManager.IsInRoleAsync(
                        user,
                        "Estudiante"))
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Estudiante");
                }

                var estudiante =
                    await context.Estudiantes
                        .FirstOrDefaultAsync(e =>
                            e.ApplicationUserId == user.Id);

                if (estudiante == null)
                {
                    estudiante = new Estudiante
                    {
                        ApplicationUserId = user.Id,
                        CarreraId =
                            carreras[i % carreras.Count].Id,
                        Semestre =
                            (i % 10) + 1,
                        Activo = true
                    };

                    context.Estudiantes.Add(estudiante);

                    await context.SaveChangesAsync();
                }

                estudiantes.Add(estudiante);
            }

            return estudiantes;
        }

        private static async Task CrearInscripcionesAsync(
            ApplicationDbContext context,
            List<Estudiante> estudiantes,
            List<Materia> materias)
        {
            Console.WriteLine();
            Console.WriteLine("Creando inscripciones...");

            var materiasParaCadaEstudiante =
                Math.Min(4, materias.Count);

            var creadas = 0;

            foreach (var estudiante in estudiantes)
            {
                for (var j = 0;
                     j < materiasParaCadaEstudiante;
                     j++)
                {
                    var materia =
                        materias[
                            (estudiante.Id + j) %
                            materias.Count];

                    var existe =
                        await context.Inscripciones
                            .AnyAsync(i =>
                                i.EstudianteId ==
                                    estudiante.Id &&
                                i.MateriaId ==
                                    materia.Id);

                    if (existe)
                        continue;

                    context.Inscripciones.Add(
                        new Inscripcion
                        {
                            EstudianteId =
                                estudiante.Id,
                            MateriaId =
                                materia.Id,
                            FechaInscripcion =
                                DateTime.UtcNow.AddDays(
                                    -(estudiante.Id % 30)),
                            Activa = true
                        });

                    creadas++;
                }
            }

            await context.SaveChangesAsync();

            Console.WriteLine(
                $"  Inscripciones creadas: {creadas}");
        }

        private static async Task CrearExamenesAsync(
            ApplicationDbContext context,
            List<Materia> materias,
            List<Estudiante> estudiantes)
        {
            Console.WriteLine();
            Console.WriteLine("Creando exámenes demo...");

            var ahora = DateTime.UtcNow;

            for (var i = 0; i < 12; i++)
            {
                var numero = i + 1;

                var materia =
                    materias[i % materias.Count];

                var titulo =
                    $"{DemoPrefix}" +
                    $"Examen {numero:00} - {materia.Nombre}";

                var examen =
                    await context.Examenes
                        .Include(e => e.Preguntas)
                            .ThenInclude(p => p.Opciones)
                        .FirstOrDefaultAsync(e =>
                            e.Titulo == titulo);

                if (examen == null)
                {
                    var docenteUserId =
                        materia.Docente!.ApplicationUserId;

                    DateTime fechaInicio;
                    DateTime fechaFin;
                    EstadoExamen estado;

                    switch (i)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            fechaInicio =
                                ahora.AddHours(-2);
                            fechaFin =
                                ahora.AddDays(7);
                            estado =
                                EstadoExamen.Publicado;
                            break;

                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            fechaInicio =
                                ahora.AddDays(2);
                            fechaFin =
                                ahora.AddDays(9);
                            estado =
                                EstadoExamen.Borrador;
                            break;

                        default:
                            fechaInicio =
                                ahora.AddDays(-14);
                            fechaFin =
                                ahora.AddDays(-7);
                            estado =
                                EstadoExamen.Cerrado;
                            break;
                    }

                    examen = new Examen
                    {
                        MateriaId =
                            materia.Id,

                        DocenteId =
                            docenteUserId,

                        Titulo =
                            titulo,

                        Descripcion =
                            $"Examen de demostración para " +
                            $"{materia.Nombre}. " +
                            $"Generado automáticamente " +
                            $"para pruebas del sistema.",

                        FechaInicio =
                            fechaInicio,

                        FechaFin =
                            fechaFin,

                        DuracionMinutos =
                            45 + ((i % 3) * 15),

                        Estado =
                            estado,

                        RequiereVerificacionFacial =
                            i % 2 == 0,

                        Activo = true
                    };

                    context.Examenes.Add(examen);

                    await context.SaveChangesAsync();

                    await CrearPreguntasAsync(
                        context,
                        examen);

                    Console.WriteLine(
                        $"  Examen creado: {titulo}");
                }
            }
        }

        private static async Task CrearPreguntasAsync(
            ApplicationDbContext context,
            Examen examen)
        {
            var existentes =
                await context.Preguntas
                    .Where(p =>
                        p.ExamenId == examen.Id)
                    .AnyAsync();

            if (existentes)
                return;

            var preguntas = new List<Pregunta>
            {
                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "¿Cuál es el propósito principal " +
                        "de una base de datos?",
                    Tipo =
                        TipoPregunta.OpcionMultiple,
                    Puntaje = 2,
                    Orden = 1
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "¿Cuál de las siguientes opciones " +
                        "representa un concepto fundamental " +
                        "de programación?",
                    Tipo =
                        TipoPregunta.OpcionMultiple,
                    Puntaje = 2,
                    Orden = 2
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "Verdadero o falso: una clave primaria " +
                        "identifica de manera única un registro " +
                        "dentro de una tabla.",
                    Tipo =
                        TipoPregunta.VerdaderoFalso,
                    Puntaje = 1,
                    Orden = 3
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "¿Cuál es una característica importante " +
                        "de la programación orientada a objetos?",
                    Tipo =
                        TipoPregunta.OpcionMultiple,
                    Puntaje = 2,
                    Orden = 4
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "Verdadero o falso: HTTP es un protocolo " +
                        "utilizado para la comunicación entre " +
                        "clientes y servidores web.",
                    Tipo =
                        TipoPregunta.VerdaderoFalso,
                    Puntaje = 1,
                    Orden = 5
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "¿Qué estructura se utiliza normalmente " +
                        "para representar una colección ordenada " +
                        "de elementos?",
                    Tipo =
                        TipoPregunta.OpcionMultiple,
                    Puntaje = 2,
                    Orden = 6
                },

                new Pregunta
                {
                    ExamenId = examen.Id,
                    Enunciado =
                        "Explique brevemente qué importancia " +
                        "tiene la seguridad en un sistema " +
                        "informático.",
                    Tipo =
                        TipoPregunta.RespuestaAbierta,
                    Puntaje = 5,
                    Orden = 7
                }
            };

            context.Preguntas.AddRange(preguntas);

            await context.SaveChangesAsync();

            foreach (var pregunta in preguntas)
            {
                if (pregunta.Tipo ==
                    TipoPregunta.RespuestaAbierta)
                {
                    continue;
                }

                if (pregunta.Tipo ==
                    TipoPregunta.VerdaderoFalso)
                {
                    context.Opciones.AddRange(
                        new Opcion
                        {
                            PreguntaId =
                                pregunta.Id,
                            Texto = "Verdadero",
                            EsCorrecta = true,
                            Orden = 1
                        },
                        new Opcion
                        {
                            PreguntaId =
                                pregunta.Id,
                            Texto = "Falso",
                            EsCorrecta = false,
                            Orden = 2
                        });

                    continue;
                }

                context.Opciones.AddRange(
                    new Opcion
                    {
                        PreguntaId =
                            pregunta.Id,
                        Texto =
                            "Almacenar y organizar información",
                        EsCorrecta = true,
                        Orden = 1
                    },
                    new Opcion
                    {
                        PreguntaId =
                            pregunta.Id,
                        Texto =
                            "Eliminar todos los datos",
                        EsCorrecta = false,
                        Orden = 2
                    },
                    new Opcion
                    {
                        PreguntaId =
                            pregunta.Id,
                        Texto =
                            "Apagar automáticamente el servidor",
                        EsCorrecta = false,
                        Orden = 3
                    },
                    new Opcion
                    {
                        PreguntaId =
                            pregunta.Id,
                        Texto =
                            "Evitar cualquier tipo de conexión",
                        EsCorrecta = false,
                        Orden = 4
                    });
            }

            await context.SaveChangesAsync();
        }

        private static async Task CrearIntentosDemoAsync(
            ApplicationDbContext context,
            List<Estudiante> estudiantes)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Creando intentos demo para Resultados...");

            var examenesCerrados =
                await context.Examenes
                    .Where(e =>
                        e.Titulo.StartsWith(DemoPrefix) &&
                        e.Estado ==
                            EstadoExamen.Cerrado)
                    .OrderBy(e => e.Id)
                    .Take(2)
                    .ToListAsync();

            if (examenesCerrados.Count == 0)
                return;

            var estudiantesDemo =
                estudiantes.Take(5).ToList();

            var creados = 0;

            foreach (var estudiante in estudiantesDemo)
            {
                foreach (var examen in examenesCerrados)
                {
                    var existe =
                        await context.IntentosExamen
                            .AnyAsync(i =>
                                i.ExamenId ==
                                    examen.Id &&
                                i.EstudianteId ==
                                    estudiante.Id);

                    if (existe)
                        continue;

                    var preguntas =
                        await context.Preguntas
                            .Include(p => p.Opciones)
                            .Where(p =>
                                p.ExamenId ==
                                examen.Id)
                            .OrderBy(p => p.Orden)
                            .ToListAsync();

                    var intento =
                        new IntentoExamen
                        {
                            ExamenId =
                                examen.Id,

                            EstudianteId =
                                estudiante.Id,

                            FechaInicio =
                                examen.FechaInicio,

                            FechaFin =
                                examen.FechaFin,

                            IdentidadVerificada =
                                examen.RequiereVerificacionFacial,

                            Finalizado = true,

                            Anulado = false,

                            Calificacion = 0
                        };

                    context.IntentosExamen.Add(intento);

                    await context.SaveChangesAsync();

                    decimal calificacion = 0;

                    foreach (var pregunta in preguntas)
                    {
                        Opcion? opcionSeleccionada = null;

                        if (pregunta.Opciones.Any())
                        {
                            var correctas =
                                pregunta.Opciones
                                    .Where(o => o.EsCorrecta)
                                    .ToList();

                            var incorrectas =
                                pregunta.Opciones
                                    .Where(o => !o.EsCorrecta)
                                    .ToList();

                            opcionSeleccionada =
                                (pregunta.Orden +
                                 estudiante.Id) % 3 == 0 &&
                                incorrectas.Any()
                                    ? incorrectas[
                                        estudiante.Id %
                                        incorrectas.Count]
                                    : correctas.FirstOrDefault();
                        }

                        var correcta =
                            opcionSeleccionada?.EsCorrecta ??
                            false;

                        var puntaje =
                            correcta
                                ? pregunta.Puntaje
                                : 0;

                        calificacion += puntaje;

                        context.Respuestas.Add(
                            new Respuesta
                            {
                                IntentoExamenId =
                                    intento.Id,

                                PreguntaId =
                                    pregunta.Id,

                                OpcionId =
                                    opcionSeleccionada?.Id,

                                RespuestaTexto =
                                    pregunta.Tipo ==
                                    TipoPregunta.RespuestaAbierta
                                        ? "Respuesta demo del estudiante."
                                        : null,

                                PuntajeObtenido =
                                    puntaje,

                                Correcta =
                                    correcta
                            });
                    }

                    intento.Calificacion =
                        calificacion;

                    await context.SaveChangesAsync();

                    creados++;
                }
            }

            Console.WriteLine(
                $"  Intentos finalizados creados: {creados}");
        }
    }
}
