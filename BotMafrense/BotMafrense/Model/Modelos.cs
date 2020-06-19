using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotMafrense.Model
{
    [Serializable]
    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public double Probability { get; set; }
    }

    [Serializable]
    public class CustomVisionResult
    {
        public List<Prediction> Predictions { get; set; }
    }
}