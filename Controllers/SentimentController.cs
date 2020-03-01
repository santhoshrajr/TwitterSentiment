using System;
using LinqToTwitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterSentimentML.Model;

namespace TwitterSentiment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SentimentController : ControllerBase
    {
        List<Status> Statuses = new List<Status>();
        static readonly int MaxStatusCount = 500;
        private readonly ILogger<SentimentController> _logger;
        public SentimentController(ILogger<SentimentController> logger)
        {
            _logger = logger;
        }
        private async Task<ApplicationOnlyAuthorizer> SetAuthentication()
        {
            var auth = new ApplicationOnlyAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = "6XoTsN2mhm19tsEtfmDIJD9XZ",
                    ConsumerSecret = "n25WEZn538y0csa37w8La2V13KMwcouSO5mYDGF7jJSxordMDw"
                    //OAuthToken = "173770592-AGfE1rXqL4nGMcbNKvzTwT2BamJIeMh4FEyOfZpZ",
                    //OAuthTokenSecret = "FlMnHWIhb99T9dCre0Kw3c6As5IyodXlYIyST6sICc1Oq"
                }
            };
            await auth.AuthorizeAsync();
            return auth;
        }

        [HttpGet("{searchTerm}")]
        public async Task<string> GetTweetsAsync(string searchTerm)
        {
            var auth = await SetAuthentication();
            var context = new TwitterContext(auth);
            if (searchTerm.ToLower().Contains("biden"))
            {
                searchTerm = "#joebiden OR #biden2020 OR #joe2020 OR #TeamJoe OR #JoeMentum";
            }
            else
            {
                searchTerm = "#BernieSanders OR #Sanders2020 OR #NotMeUs OR #Bernie2020 OR #TeamBernie";
            }
            Search(context, $"#californiaprimary AND {searchTerm}");
            var model = new ConsumeModel();
            var outSentiment = new List<Sentiment>();
            foreach (var item in Statuses)
            {
                var input = new ModelInput() { TextToAnalyze = item.FullText };
                var response = model.Predict(input);
                outSentiment.Add(new Sentiment
                {
                    CreatedAt = item.CreatedAt,
                    Text = item.FullText,
                    Location = item.User.Location,
                    SentimentPct = response.Probability,
                    ImageUrl = item.Entities.MediaEntities.Any() ? item.Entities.MediaEntities[0].MediaUrlHttps : ""
                });
            }
            return JsonConvert.SerializeObject(outSentiment);
        }

        private void Search(TwitterContext context, string searchTerm, ulong sinceId = 1, ulong maxId = ulong.MaxValue)
        {
            var response = context.Search.Where(s => s.Type == SearchType.Search &&
                                                     s.Query == searchTerm &&
                                                     s.TweetMode == TweetMode.Extended &&
                                                     s.IncludeEntities == true &&
                                                     s.SinceID == sinceId &&
                                                     s.MaxID == maxId).ToList();
            if (response[0].Statuses.Any())
            {
                maxId = response[0].Statuses.Min(s => s.StatusID) - 1;
                Statuses.AddRange(response[0].Statuses.Where(x => !x.FullText.StartsWith("RT")));

                if (response[0].Statuses.Any() && Statuses.Count() < MaxStatusCount)
                    Search(context, searchTerm, sinceId, maxId);
            }

        }
    }
}