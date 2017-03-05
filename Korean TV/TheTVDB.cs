using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Korean_TV
{
    class TheTVDB
    {
        private static readonly Uri address = new Uri("https://api.thetvdb.com");

        public static string login(String apiKey, String username, String userkey)
        {
            JObject authObject = new JObject();
            authObject.Add(new JProperty("apikey", apiKey));
            authObject.Add(new JProperty("username", username));
            authObject.Add(new JProperty("userkey", userkey));
            
            String json = POST(new Uri(address, "/login").ToString(), authObject.ToString());
            JToken response = JObject.Parse(json);
            return (String)response.SelectToken("token");

        }

        public static int search(string title)
        {
            title = WebUtility.HtmlEncode(title);
            Uri uri = new Uri(address, "/search/series?name=" + title);
            String json = GET(uri.ToString());
            JToken data = JObject.Parse(json).SelectToken("data");
            if (data == null) return -1;
            return (int)data.First.SelectToken("id");
        }

        public static List<Tuple<int, int, DateTime>> episodes(int id)
        {
            List<Tuple<int, int, DateTime>> episodes = new List<Tuple<int, int, DateTime>>();

            int page = 1;
            while(true)
            {
                Uri uri = new Uri(address, "/series/" + id + "/episodes?page=" + page);

                String json = GET(uri.ToString());
                JArray data = (JArray)JObject.Parse(json).SelectToken("data");

                if (data == null) break;
                foreach (JToken episode in data)
                {
                    int season = (int)episode.SelectToken("airedSeason");
                    int episodeNumber = (int)episode.SelectToken("airedEpisodeNumber");
                    string date = (string)episode.SelectToken("firstAired");
                    if (String.IsNullOrEmpty(date)) continue;
                    DateTime firstAired = DateTime.Parse(date);
                    episodes.Add(new Tuple<int, int, DateTime>(season, episodeNumber, firstAired));
                }
                page++;
            }

            return episodes;
        }

        private static string POST(string url, string jsonContent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 25000;

            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                String data = reader.ReadToEnd();
                return WebUtility.HtmlDecode(data);
            }
        }

        private static string GET(string url)
        {
            String json = null;
            const int attempts = 10;
            for(int i=0; i< attempts; i++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = @"application/json";
                request.Headers["Authorization"] = "Bearer " + Program.token;
                request.Headers["Accept-Language"] = "ko";
                request.Timeout = 25000;

                try
                {
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        String data = reader.ReadToEnd();
                        json = WebUtility.HtmlDecode(data);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.Timeout)
                        continue;
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        String errorText = reader.ReadToEnd();
                        json = WebUtility.HtmlDecode(errorText);
                    }
                }

                try
                {
                    JObject.Parse(json);
                }
                catch (JsonReaderException)
                {
                    continue;
                }

                break;
            }

            return json;
        }

    }
}
