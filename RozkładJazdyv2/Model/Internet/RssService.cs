using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Syndication;
using RozkładJazdyv2.Model.RSS;

namespace RozkładJazdyv2.Model.Internet
{
    public class RssService
    {
        public static readonly string KZKGOP_COMMUNICATES_FEED = "http://rozklady.kzkgop.pl/RSS/komunikaty/";

        private SyndicationClient _RssClient;
        private string _RssUrl;

        public RssService(string url)
        {
            this._RssClient = new SyndicationClient();
            this._RssUrl = url;
        }

        public async Task<ObservableCollection<RssItem>> GetCommunicatesAsync()
        {
            ObservableCollection<RssItem> communicates = new ObservableCollection<RssItem>();
            SyndicationFeed rssFeed = await _RssClient.RetrieveFeedAsync(new Uri(_RssUrl));

            if (rssFeed == null)
                return communicates;

            foreach(SyndicationItem communication in rssFeed.Items)
            {
                communicates.Add(new RssItem()
                {
                    Title = new string(communication.Title.Text.Select((p,i) => i == 0 ? p.ToString().ToUpper()[0] : p).ToArray<char>()),
                    Desc = communication.Summary.Text.Replace(".", $".{Environment.NewLine}"),
                    Url = communication.Id
                });
            }

            return communicates;
        }
    }
}
