using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using BotMafrense.Formulario;
using BotMafrense.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace BotMafrense.Dialogs
{
    /// <summary>
    /// Usando LUIS (https://www.luis.ai)
    /// </summary>
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        public LuisDialog(ILuisService service) : base(service) { }

        [LuisIntent("")]
        [LuisIntent("None")]
        [LuisIntent("Qna")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.Forward(new QnaDialog(), ResumeAfter, context.Activity, CancellationToken.None);
        }

        [LuisIntent("Duvida")]
        public async Task Duvida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Por favor, fique a vontate para fazer perguntas.");
        }

        [LuisIntent("AgendarVisita")]
        public async Task AgendarVisita(IDialogContext context, LuisResult result)
        {
            await context.Forward(MakeRoot(), ResumeAfter, context.Activity, CancellationToken.None);
        }

        internal static IDialog<FormOrcamento> MakeRoot()
        {
            return Chain.From(() => FormDialog.FromForm(FormOrcamento.BuildForm));
        }

        [LuisIntent("Cumprimento")]
        public async Task Cumprimento(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Olá! Bem vindo ao Pisos Mafrense! \n" +
                         "- Tem alguma dúvida? Fique a vontade para perguntar.\n" +
                         "- Ou deseja reconhecer um Piso através de uma imagem?\n" +
                         "- Ou agendar uma visita (para tirar medidas e realizar um orçamento)?");
        }

        [LuisIntent("Cotacao")]
        public async Task Cotacao(IDialogContext context, LuisResult result)
        {
            //quanto está o metro do piso Eucafloor Prime?
            var entidades = result.Entities?.Where(x => x.Type == "MarcaPiso").Select(e => e.Entity);
            if (!entidades.Any())
            {
                await context.PostAsync("Desculpe. Não foi possível encontrar o valor.");
                return;
            }
            var filtro = entidades.First();
            var endpoint = $"http://apimafrenseprecos.azurewebsites.net/api/preco?produto={filtro}";

            await context.PostAsync("Deixa eu ver aqui...");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await context.PostAsync("Ocorreu algum erro... tente mais tarde");
                    return;
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<Preco>(json);
                    if (resultado.Produto == null || resultado.Valor == 0)
                    {
                        await context.PostAsync("Desculpe. Não foi possível encontrar o valor.");
                        return;
                    }
                    var culture = CultureInfo.CreateSpecificCulture("pt-BR");
                    await context.PostAsync($"O " + filtro + " está custando R$ " + resultado.Valor.ToString("G", culture) + " o m².");
                }
            }
        }

        [LuisIntent("DescobrirMarcaPiso")]
        public async Task DescobrirMarcaPiso(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Me envia uma imagem e eu tentarei encontrar o Modelo correspondente!");
            context.Wait((c, a) => ProcessarImagemAsync(c, a));
        }

        private async Task ProcessarImagemAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // retrieve the Custom Vision credentials:
            string predictionKey = System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionKey"];
            //Guid projectGuid = new Guid(WebConfigurationManager.AppSettings["visionProjectGuid"]);
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionImage"];

            try
            {
                // Retrieve user message, and ensure it has an attachment:
                var message = await result;
                if (message.Attachments != null && message.Attachments.Any())
                {
                    var attachment = message.Attachments.First();
                    using (HttpClient httpClient = new HttpClient())
                    {
                        // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                        if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) ||
                            message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                            && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                        {
                            var token = await new MicrosoftAppCredentials().GetTokenAsync();
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }

                        // retrieve our image, and create a byte array:
                        byte[] imageByteArray = await httpClient.GetByteArrayAsync(attachment.ContentUrl);

                        // Create our request client:
                        var client = new HttpClient();

                        // Add a request header with our Custom Vision Prediction Key:
                        client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);


                        // Construct Prediction URL, using Project GUID:
                        //string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/"
                        //    + projectGuid.ToString() + "/image?";

                        // Instantiate response:
                        HttpResponseMessage response;

                        using (var content = new ByteArrayContent(imageByteArray))
                        {
                            // Set content type, and retrieve response:
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            //response = client.PostAsync(url, content).Result;
                            response = await client.PostAsync(url, content).ConfigureAwait(false);

                            #region comment
                            //using (var stream = response.Content.ReadAsStreamAsync().Result)
                            //{
                            //    // Serialize our response with data contract:
                            //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CustomVision.JSON.Response));
                            //    CustomVision.JSON.Response visionJsonResponse = ser.ReadObject(stream) as CustomVision.JSON.Response;

                            //    // Ensure we have some results from the Custom Vision Service:
                            //    if (visionJsonResponse != null && visionJsonResponse.Predictions != null && visionJsonResponse.Predictions.Length > 0)
                            //    {
                            //        // Build a D365 Web API request to retrieve a matching product from the catalog:
                            //        HttpResponseMessage productResponse = await Utilities.CRMWebAPIRequest("api/data/v8.2/products?" +
                            //            "$select=name,productnumber,producturl,description,new_externalimageurl&" +
                            //            "$top=1&$filter=name eq '" + visionJsonResponse.Predictions[0].Tag + "'",
                            //            null, "retrieve");

                            //        // Check to make sure our request was successful:
                            //        if (productResponse.IsSuccessStatusCode)
                            //        {

                            //            // Read our response into a JSON Array:
                            //            string myString = productResponse.Content.ReadAsStringAsync().Result;
                            //            JObject productResults =
                            //                JObject.Parse(productResponse.Content.ReadAsStringAsync().Result);
                            //            JArray items = (JArray)productResults["value"];


                            //            // Ensure we have some results:
                            //            if (items.Count > 0)
                            //            {

                            //                // format probability for presentation:
                            //                string formattedProbability = Math.Round((visionJsonResponse.Predictions[0].Probability * 100), 1).ToString() + "%";

                            //                // prepare message back to user:    
                            //                IMessageActivity msgProductReply = context.MakeMessage();
                            //                msgProductReply.Type = ActivityTypes.Message;
                            //                msgProductReply.TextFormat = TextFormatTypes.Markdown;

                            //                // prepare rich card, showing product image, and button to see specs:
                            //                if ((string)items[0]["producturl"] != null && (string)items[0]["new_externalimageurl"] != null)
                            //                {

                            //                    List<CardImage> cardImages = new List<CardImage>();
                            //                    cardImages.Add(new CardImage(url: (string)items[0]["new_externalimageurl"]));
                            //                    HeroCard actionCard = new HeroCard()
                            //                    {

                            //                        Title = visionJsonResponse.Predictions[0].Tag + " (" + (string)items[0]["productnumber"] + ")",
                            //                        Subtitle = (string)items[0]["description"] + "Identified with probability of " + formattedProbability,
                            //                        Images = cardImages,
                            //                        Buttons = {
                            //                            new CardAction() { Title = "View Product Specs", Type = ActionTypes.OpenUrl, Value = (string)items[0]["producturl"] },
                            //                            new CardAction() { Title = "Done", Type = ActionTypes.ImBack, Value = "Done" }
                            //                        }
                            //                    };

                            //                    Attachment actionAttachment = actionCard.ToAttachment();
                            //                    msgProductReply.Attachments.Add(actionAttachment);

                            //                }

                            //                // Post message::
                            //                await context.PostAsync(msgProductReply);
                            //                context.Wait(MessageReceivedAsync);
                            //            }
                            //            else
                            //            {
                            //                // Inform user and prompt them to try again:
                            //                await context.PostAsync("I wasn't able to identify a product.");
                            //                StartingOptionsMessage(context);
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        // Inform user and prompt them to try again:
                            //        await context.PostAsync("I wasn't able to identify a product.");
                            //        StartingOptionsMessage(context);
                            //    }
                            //}
                            #endregion comment
                        }

                        var responseString = await response.Content.ReadAsStringAsync();
                        var resultados = JsonConvert.DeserializeObject<CustomVisionResult>(responseString);
                        var resultado = resultados.Predictions.OrderByDescending(c => c.Probability).FirstOrDefault();

                        if (resultado != null)
                        {
                            List<string> retorno = new List<string>();
                            int count = 1;
                            foreach (var r in resultados.Predictions.OrderByDescending(c => c.Probability).ToList())
                            {
                                retorno.Add(r.Tag);

                                count++;
                                if (count > 2)
                                    break;
                            }

                            string caminhoImagens = "http://apimafrenseprecos.azurewebsites.net/Images/";
                            foreach (var tag in retorno)
                            {
                                Activity resposta = ((Activity)context.Activity).CreateReply();

                                HeroCard card = new HeroCard()
                                {
                                    Title = tag.Replace("-", " ").Replace("_", " ")
                                    //,Subtitle = caminhoImagens + tag + ".png"
                                };

                                card.Images = new List<CardImage>
                                {
                                    new CardImage(caminhoImagens + tag + ".png")
                                };

                                resposta.Attachments.Add(card.ToAttachment());

                                await context.PostAsync(resposta);
                            }

                            await context.PostAsync("São os mais parecidos que encontrei.");
                        }
                        else
                            await context.PostAsync("Não encontramos nenhum resutado.");
                    }
                }
                else
                {
                    // Prompt user to take a photo:
                    //await context.PostAsync("Por favor envie uma imagem.");
                    //context.Wait(ProcessarImagemAsync);
                    await context.PostAsync("Não foi possível verificar a imagem.");
                }
            }
            catch (Exception e)
            {
                string reply = "Desculpa. Algo deu errado. \nDetalhes: " + e.ToString();
                await context.PostAsync(reply);
                context.Done<bool>(true);
            }

        }

        private async Task ProcessarImagemUrlAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument;

            var uri = activity.Attachments?.Any() == true ?
                new Uri(activity.Attachments[0].ContentUrl) :
                new Uri(activity.Text);

            try
            {
                List<string> reply;
                reply = await new VisaoComputacional().ClassificacaoCustomizadaAsync(uri);
                if (reply != null)
                {
                    string caminhoImagens = "http://apimafrenseprecos.azurewebsites.net/Images/";
                    foreach (var tag in reply)
                    {
                        Activity resposta = ((Activity)context.Activity).CreateReply();

                        HeroCard card = new HeroCard()
                        {
                            Title = tag.Replace("-", " ").Replace("_", " ")
                            ,
                            Subtitle = caminhoImagens + tag + ".png"
                        };

                        //card.Buttons = new List<CardAction>
                        //{
                        //    new CardAction(ActionTypes.OpenUrl, "Compre Agora", value:url)
                        //};

                        card.Images = new List<CardImage>
                        {
                            new CardImage(caminhoImagens + tag + ".png")
                        };

                        resposta.Attachments.Add(card.ToAttachment());

                        await context.PostAsync(resposta);
                    }

                    await context.PostAsync("São os mais parecidos que encontrei.");
                }
                else
                    await context.PostAsync("Nenhum resultado encontrado");
            }
            catch (Exception e)
            {
                await context.PostAsync("Ops! Deu algo errado na hora de analisar sua imagem! \nDetalhes: " + e.ToString());
            }

            context.Wait(MessageReceived);
        }



        private static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }

        private async Task ResumeAfter(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}