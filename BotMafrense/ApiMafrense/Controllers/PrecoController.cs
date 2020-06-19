using ApiMafrense.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace ApiMafrense.Controllers
{
    [System.Web.Mvc.RoutePrefix("api/preco")]
    public class PrecoController : ApiController
    {
        // GET: api/Preco/5
        [System.Web.Mvc.Route("{produto}")]
        public Preco GetPreco(string produto)
        {
            string RetornaFiltro()
            {
                var dicionario = new Dictionary<string, List<string>>
                {
                    ["EUCAFLOOR"] = new List<string> { "eucafloor", "eucaflor", "eucaflór" },
                    ["DURAFLOOR"] = new List<string> { "durafloor", "duraflor", "duraflór" },
                    ["QUICKSTEP"] = new List<string> { "quickstep", "quick step", "quickstép", "quikstep", "quicstep" },
                };

                if (string.IsNullOrEmpty(produto))
                    return null;
                //    return dicionario.Select(d => d.Key).ToList();
                
                var keyvalue = dicionario.FirstOrDefault(d => d.Value.Contains(produto.ToLower()));
                if (!string.IsNullOrEmpty(keyvalue.Key))
                    return keyvalue.Key;

                return null;
            }

            decimal ObterValor(string p)
            {
                if (p == "EUCAFLOOR")
                    return 29.99M;
                else if (p == "DURAFLOOR")
                    return 34.99M;
                else if (p == "QUICKSTEP")
                    return 27.99M;
                else
                    return 0M;
            }

            var prodChave = RetornaFiltro();
            var valor = ObterValor(prodChave);
            var culture = CultureInfo.CreateSpecificCulture("pt-BR");
            return new Preco() { Produto = prodChave, Valor = valor };// valor.ToString("G", culture);
        }

        //// GET: api/Preco
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        //// GET: api/Preco/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Preco
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Preco/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Preco/5
        public void Delete(int id)
        {
        }
    }
}
