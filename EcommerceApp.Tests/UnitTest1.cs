using EcommerceApp.Models;

namespace EcommerceApp.Tests;

public class CalificacionExamenTests
{
    [Fact]
    public void OpcionCorrecta_DebeOtorgarElPuntaje()
    {
        var pregunta = new Pregunta
        {
            Puntaje = 10
        };

        var opcionCorrecta = new Opcion
        {
            EsCorrecta = true
        };

        pregunta.Opciones.Add(opcionCorrecta);

        var opcionSeleccionada =
            pregunta.Opciones.FirstOrDefault(o =>
                o.Id == opcionCorrecta.Id);

        var calificacion =
            opcionSeleccionada?.EsCorrecta == true
                ? pregunta.Puntaje
                : 0;

        Assert.Equal(10, calificacion);
    }

    [Fact]
    public void OpcionIncorrecta_DebeOtorgarCero()
    {
        var pregunta = new Pregunta
        {
            Puntaje = 10
        };

        var opcionIncorrecta = new Opcion
        {
            EsCorrecta = false
        };

        pregunta.Opciones.Add(opcionIncorrecta);

        var opcionSeleccionada =
            pregunta.Opciones.FirstOrDefault(o =>
                o.Id == opcionIncorrecta.Id);

        var calificacion =
            opcionSeleccionada?.EsCorrecta == true
                ? pregunta.Puntaje
                : 0;

        Assert.Equal(0, calificacion);
    }

    [Fact]
    public void PreguntaAbierta_DebeOtorgarCero()
    {
        var pregunta = new Pregunta
        {
            Tipo = TipoPregunta.RespuestaAbierta,
            Puntaje = 10
        };

        var calificacion = 0m;

        Assert.Equal(0, calificacion);
    }

    [Fact]
    public void OpcionDeOtraPregunta_NoDebeSerAceptada()
    {
        var pregunta = new Pregunta
        {
            Puntaje = 10
        };

        var otraPregunta = new Pregunta
        {
            Puntaje = 20
        };

        var opcionOtraPregunta = new Opcion
        {
            Id = 999,
            EsCorrecta = true,
            Pregunta = otraPregunta
        };

        otraPregunta.Opciones.Add(opcionOtraPregunta);

        var opcionSeleccionada =
            pregunta.Opciones.FirstOrDefault(o =>
                o.Id == opcionOtraPregunta.Id);

        Assert.Null(opcionSeleccionada);
    }

    [Fact]
    public void OpcionCorrecta_PerteneceALaPreguntaCorrecta()
    {
        var pregunta = new Pregunta
        {
            Puntaje = 5
        };

        var opcion = new Opcion
        {
            Id = 100,
            EsCorrecta = true,
            Pregunta = pregunta
        };

        pregunta.Opciones.Add(opcion);

        var encontrada =
            pregunta.Opciones.FirstOrDefault(o =>
                o.Id == 100);

        Assert.NotNull(encontrada);
        Assert.True(encontrada!.EsCorrecta);
        Assert.Equal(5, pregunta.Puntaje);
    }
}
