using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotMafrense.Model
{
    public class Preco
    {
        [JsonProperty("Produto")]
        public string Produto { get; set; }
        [JsonProperty("Valor")]
        public float Valor { get; set; }
    }
}