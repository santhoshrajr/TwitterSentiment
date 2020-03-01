using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterSentiment
{
    public class Sentiment
    {
        public DateTime CreatedAt { get; set; }

        public string Text { get; set; }

        public float SentimentPct { get; set; }

        public string Location { get; set; }

        public string ImageUrl { get; set; }
    }
}
