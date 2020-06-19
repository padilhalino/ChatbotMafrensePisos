using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BotMafrense
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}


/*

BotMafrenseChannel
a6d9a3b5-58f2-4d59-805c-bf2944e224c3
btHK8=ucgafCCESO0531$?(

# Skype
<span class="skype-button rounded" data-bot-id="a6d9a3b5-58f2-4d59-805c-bf2944e224c3" data-color="#8cd32c"></span>
<span class="skype-chat" data-color-message="#85f20d"></span>
<script src="https://swc.cdn.skype.com/sdk/v1/sdk.min.js"></script>

# Facebook
1684023688323574
538585413207879
b3a54736c734397bbb7901791313812c
EAAHp1zA8R0cBABYRjuFpZAP66YdAXh8cO7j7RZAMx5v913oPgmihNoqQwB09fgZCZBYXk7u3z91tt0y0V2uMtgyQoX362butf6ZB7npJK6H7n0w1hjZCKvVWgNzJRVtt40ZBatl2DQU1ULjX8rGpihaPCFYGiaAzknDtgesIUPXZCF4yZCOUveUoj
 
 */
