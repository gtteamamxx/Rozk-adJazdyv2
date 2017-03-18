using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class HtmlService
    {
        private HtmlService() {}

        public static async Task<string> GetHtmlFromSite(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        byte[] data = await response.Content.ReadAsByteArrayAsync();
                        return @Encoding.UTF8.GetString(data);
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
