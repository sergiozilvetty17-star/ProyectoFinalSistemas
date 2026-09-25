using Google.Cloud.Speech.V2;

namespace EcommerceApp.Services.Speech
{
    public class GoogleSpeechService
    {
        private readonly SpeechClient _client;
        private readonly string _projectId;

        public GoogleSpeechService(SpeechClient client)
        {
            _client = client;

            _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
                ?? throw new InvalidOperationException(
                    "No se encontró la variable GOOGLE_CLOUD_PROJECT.");
        }

        public async Task<string> TranscribirAsync(
            Stream audioStream,
            CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();

            await audioStream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var audioBytes = memoryStream.ToArray();

            if (audioBytes.Length == 0)
                return string.Empty;

            var config = new RecognitionConfig
            {
                AutoDecodingConfig = new AutoDetectDecodingConfig(),
                Model = "chirp_3"
            };

            config.LanguageCodes.Add("es-BO");

            var request = new RecognizeRequest
            {
                Recognizer =
                    $"projects/{_projectId}/locations/global/recognizers/_",

                Config = config,

                Content = Google.Protobuf.ByteString.CopyFrom(audioBytes)
            };

            var response = await _client.RecognizeAsync(request);

            return string.Join(
                " ",
                response.Results
                    .Where(result => result.Alternatives.Count > 0)
                    .Select(result => result.Alternatives[0].Transcript))
                .Trim();
        }
    }
}
