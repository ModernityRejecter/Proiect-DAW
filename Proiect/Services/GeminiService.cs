using System.Text;
using Google.GenAI;
using Proiect.Models;

namespace Proiect.Services
{
    public class GeminiService
    {
        private readonly IConfiguration _configuration;

        public GeminiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAnswerAsync(Product product, List<ProductFAQs> solvedFaqs, string userQuestion)
        {
            string apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("YOUR_API_KEY"))
            {
                return "Eroare configurare API Key";
            }

            var client = new Client(apiKey: apiKey);

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Role: Helpful assistant for an online shop.");
            promptBuilder.AppendLine("Task: Answer the user question strictly using the provided context.");

            promptBuilder.AppendLine("\n--- PRODUCT INFO ---");
            promptBuilder.AppendLine($"Name: {product.Name}");
            promptBuilder.AppendLine($"Description: {product.Description}");
            promptBuilder.AppendLine($"Price: {product.Price} RON");

            promptBuilder.AppendLine("\n--- FAQ HISTORY ---");
            if (solvedFaqs.Any())
            {
                foreach (var faq in solvedFaqs)
                {
                    promptBuilder.AppendLine($"Q: {faq.Question} | A: {faq.Answer}");
                }
            }
            else
            {
                promptBuilder.AppendLine("No history available.");
            }

            promptBuilder.AppendLine("\n--- USER QUESTION ---");
            promptBuilder.AppendLine(userQuestion);

            promptBuilder.AppendLine("\n--- STRICT RULES ---");
            promptBuilder.AppendLine("1. First check FAQ HISTORY. If a similar question exists, rephrase that answer.");
            promptBuilder.AppendLine("2. If not found, check PRODUCT INFO. If found there, answer politely.");
            promptBuilder.AppendLine("3. If the information is NOT in these sources, output EXACTLY: NU_STIU");
            promptBuilder.AppendLine("4. Do NOT invent information.");

            try
            {
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash-lite",
                    contents: promptBuilder.ToString()
                );

                string text = response.Candidates[0].Content.Parts[0].Text;
                return text?.Trim() ?? "NU_STIU";
            }
            catch
            {
                return "NU_STIU";
            }
        }
    }
}