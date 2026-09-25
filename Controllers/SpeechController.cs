using EcommerceApp.Services.Speech;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Administrador,Docente")]
    public class SpeechController : Controller
    {
        private readonly GoogleSpeechService _speechService;

        public SpeechController(GoogleSpeechService speechService)
        {
            _speechService = speechService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transcribe(
            IFormFile audio,
            CancellationToken cancellationToken)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se recibió audio."
                });
            }

            if (audio.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El audio es demasiado grande."
                });
            }

            try
            {
                await using var stream = audio.OpenReadStream();

                var text = await _speechService.TranscribirAsync(
                    stream,
                    cancellationToken);

                return Json(new
                {
                    success = true,
                    text
                });
            }
           catch (Exception ex)
{
    Console.WriteLine("========================================");
    Console.WriteLine("ERROR GOOGLE SPEECH-TO-TEXT");
    Console.WriteLine($"Tipo: {ex.GetType().FullName}");
    Console.WriteLine($"Mensaje: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine("---- INNER EXCEPTION ----");
        Console.WriteLine($"Tipo: {ex.InnerException.GetType().FullName}");
        Console.WriteLine($"Mensaje: {ex.InnerException.Message}");
        Console.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
    }

    Console.WriteLine("========================================");

    return StatusCode(500, new
    {
        success = false,
        message = "No fue posible procesar el audio.",
        detail = ex.Message
    });
}
        }
    }
}
