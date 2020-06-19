using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Threading;

namespace BotMafrense.Formulario
{
    [Serializable]
    public class FormOrcamento
    {
        [Prompt("Em qual de nossos produtos você está interessado(a)? {||}")]
        [Describe("Tipo Produto")]
        public TipoProduto TipoProduto { get; set; }
        //public MarcaPiso Marca { get; set; }
        //public LinhaEucafloor Eucafloor { get; set; }
        //public LinhaDurafloor Durafloor { get; set; }

        [Prompt("Por favor, informe seu {&}")]
        [Describe("Nome")]
        public string Nome { get; set; }
        [Prompt("Por favor, informe seu {&}")]
        [Describe("Telefone")]
        public string Telefone { get; set; }
        [Prompt("Por favor, informe seu {&} (opcional)")]
        [Describe("Endereço")]
        [Optional]
        public string Endereco { get; set; }

        public static IForm<FormOrcamento> BuildForm()
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt-BR");

            var form = new FormBuilder<FormOrcamento>();
            form.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.Buttons;
            form.Configuration.Yes = new string[] { "sim", "s", "yes", "y", "yep" };
            form.Configuration.No = new string[] { "não", "nao", "n", "no", "not" };
            form.Message("Para agendar uma visita, precisamos dos dados a seguir:");
            //form.Confirm(async (state) =>
            //{
            //    return new PromptAttribute("Teste ok?  {||}");
            //});
            form.OnCompletion(async (context, pedido) =>
            {
                // Salvar na base de dados
                // Gerar pedido de Orçamento
                // Integrar com serviço xpto.
                await context.PostAsync("Seu pedido de orçamento número 123456 foi gerado. Aguarde nosso contato para agendarmos uma visita.");
            });

            

            return form.Build();

            //TemplateConfirmation

            //return form.Message("Para agendar uma visita, precisamos dos dados a seguir:")
            //    .Field(nameof(TipoProduto))
            //    .Field(nameof(Nome))
            //    .Field(nameof(Telefone))
            //    .Field(nameof(Endereco))
            //    .OnCompletion(async (context, pedido) =>
            //    {
            //        // Salvar na base de dados
            //        // Gerar pedido de Orçamento
            //        // Integrar com serviço xpto.
            //        await context.PostAsync("Seu pedido de orçamento número 123456 foi gerado. Aguarde nosso contato para agendarmos uma visita.");
            //    }).Build();
        }
    }

    public enum TipoProduto
    {
        [Terms("Piso Laminado", "PisoLaminado", "Laminado")]
        [Describe("Piso Laminado")]
        PisoLaminado = 1,
        [Terms("Piso Vinílico", "Piso Vinilico", "PisoVinílico", "PisoVinilico", "Vinílico", "Vinilico")]
        [Describe("Piso Vinílico")]
        PisoVinilico,
        [Terms("Carpete", "Carpe", "Carpê", "Carpet", "DeCarpett", "DeCarpetts", "DeCarpet", "DeCarpete")]
        Carpete,
        [Terms("Papel de Parede", "PapeldeParede", "Papel", "Parede")]
        [Describe("Papel de Parede")]
        PapelDeParede,
        [Terms("outros", "varios", "vários")]
        [Describe("Vários ou Outros")]
        Outros
    }

    public enum MarcaPiso
    {
        [Terms("Duraflor", "Duraflór")]
        Durafloor = 1,
        [Terms("Eucafloor", "Eucaflor", "Eucaflór")]
        Eucafloor,
        [Terms("Quick Step", "Quic Step", "Quik Step", "QuikStep", "kick Step", "Quik Stép", "Quic Istepi")]
        [Describe("Quick Step")]
        QuickStep
    }

    public enum LinhaEucafloor
    {
        [Terms("Elegance", "Elegante", "Elegancia", "Elegância")]
        Elegance = 1,
        [Terms("Evidence", "Evidencia")]
        Evidence,
        [Terms("Prime", "Praime")]
        Prime,
        [Terms("Gran Elegance", "GranElegance", "gram elegance", "gran elegancia", "gran elegância")]
        [Describe("Gran Elegance")]
        GranElegance
    }

    public enum LinhaDurafloor
    {
        [Terms("Unique", "Unike", "Iunique")]
        Unique = 1,
        [Terms("Sense", "Sence", "cense", "cence")]
        Sense,
        [Terms("Studio", "istudio", "estudio", "estídio")]
        Studio,
        [Terms("Ritz", "Rits", "Ritiz", "Ritis")]
        Ritz,
        [Terms("New Way", "NewWay", "New uay", "niu uei")]
        [Describe("New Way")]
        NewWay,
        [Terms("Marcas do Tempo", "MarcasdoTempo")]
        [Describe("Marcas do Tempo")]
        MarcasDoTempo,
        [Terms("Nature", "Natura")]
        Nature
    }
}