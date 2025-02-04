using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
public class SentimentAnalysisService
{
    private readonly HttpClient _httpClient;

    public SentimentAnalysisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Seteaza URL-ul serviciului IBM Watson
        _httpClient.BaseAddress = new Uri("https://api.eu-de.natural-language-understanding.watson.cloud.ibm.com");
        var apiKey = "G7MurUy5O1O0RnCiw9x5LB7Y5fkOfBqbvkkMs7xPZlKv";
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"apikey:{apiKey}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

    }

    public async Task<string> AnalyzeSentimentAsync(string content)
    {
        // Construiește corpul cererii pentru NLU
        var requestBody = new
        {
            text = content, // Textul recenziei
            features = new
            {
                sentiment = new { } // pentru ca dorim analiza sentimentului 
            },
            language = "en" // Specifică limba textului
        };

        // Trimite cererea POST către endpoint-ul corect
        var response = await _httpClient.PostAsJsonAsync("https://api.eu-de.natural-language-understanding.watson.cloud.ibm.com/v1/analyze?version=2022-04-07", requestBody);
        
        response.EnsureSuccessStatusCode();

        // Parsează răspunsul
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine("responseContent: \n");
        Console.WriteLine(responseContent);
        var result = JsonSerializer.Deserialize<SentimentResponse>(responseContent);

        return result?.Sentiment?.Document?.Label ?? "Neutru";
    }

    //Aceasta clasa mapeaza structura raspunsului JSON de la Watson
    private class SentimentResponse
    {
        [JsonPropertyName("sentiment")]
        public SentimentResult Sentiment { get; set; }

        public class SentimentResult
        {
            [JsonPropertyName("document")]
            public DocumentSentiment Document { get; set; }

            public class DocumentSentiment
            {
                [JsonPropertyName("label")]
                public string Label { get; set; } // Poate fi "positive", "neutral", sau "negative"
                [JsonPropertyName("score")]
                public double Score { get; set; } //scor intre -1 si 1 care arata cat de sigur este modelul de rezultat
            }
        }
    }
}
