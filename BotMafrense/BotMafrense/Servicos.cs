using BotMafrense.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.Cognitive.CustomVision.Prediction;
using System.IO;

namespace BotMafrense
{
    public class VisaoComputacional
    {
        private readonly string _customApiKey = ConfigurationManager.AppSettings["CustomVisionKey"];
        private readonly string _customVisionUri = ConfigurationManager.AppSettings["CustomVisionUri"];

        public async Task<List<string>> ClassificacaoCustomizadaAsync(Uri query)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", _customApiKey);

            HttpResponseMessage response = null;

            var byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(_customVisionUri, content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var results = JsonConvert.DeserializeObject<CustomVisionResult>(responseString);

            var result = results.Predictions.OrderByDescending(c => c.Probability).FirstOrDefault();

            if (result != null)
            {
                List<string> retorno = new List<string>();
                int count = 1;
                foreach (var r in results.Predictions.OrderByDescending(c => c.Probability).ToList())
                {
                    retorno.Add(r.Tag);

                    count++;
                    if (count > 2)
                        break;
                }

                return retorno;
            }
            else
                return null;
        }

        public async Task<string> ClassificacaoCustomizadaImagemAsync(Stream image)
        {
            // Now there is a trained endpoint, it can be used to make a prediction

            // Add your prediction key from the settings page of the portal
            // The prediction key is used in place of the training key when making predictions
            string predictionKey = _customApiKey;

            // Create a prediction endpoint, passing in the obtained prediction key
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = predictionKey };

            // Make a prediction against the new project
            Console.WriteLine("Making a prediction:");
            var result = await endpoint.PredictImageAsync(new Guid("fd6f899b-1987-41b7-8067-efe0d262d319"), image);

            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                return $"Eu identifiquei um objeto do tipo **{c?.Tag}** na imagem, com " +
                       $"**{c?.Probability}**% de acertividade.";
            }

            return "Nenhum resultado encontrado.";
        }
    }
}