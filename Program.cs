using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Yammer.Downloader
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            var token = "Bearer "; //use the developer tools to get a token.

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

            //Task.WaitAll(Get("users"));
            Task.WaitAll(Get("groups"));
            //Task.WaitAll(GetAllMessages());
        }

        public static async Task GetAllMessages()
        {            
            var query = string.Empty;
            int page = 0;
            bool cont = true;
            do
            {
                
                var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.yammer.com/api/v1/messages.json?{query}");
                Console.WriteLine($"Downloading messages {page}");
                var resp = client.SendAsync(req).Result;

                if ((int)resp.StatusCode == 429)
                {
                    await Task.Delay(10 * 10000);
                    Console.WriteLine($"Pause at {page}");
                    continue;
                }

                var data = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(data);
                JArray messages = json.GetValue("messages") as JArray;
                if (messages.Count < 20)
                {
                    cont = false;
                }

                var olderthan = messages.Last["id"].Value<string>();
                query = "older_than=" + olderthan;

                File.WriteAllText($"messages{olderthan}.json", JsonConvert.SerializeObject(messages));
                page++;
            } while (cont);
        }

        public static async Task Get(string type)
        {
            int page = 1;
            bool cont = true;
            do
            {

                var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.yammer.com/api/v1/{type}.json?page={page}");
                Console.WriteLine($"Downloading {type} {page}");
                var resp = client.SendAsync(req).Result;

                if ((int)resp.StatusCode == 429)
                {
                    await Task.Delay(10 * 10000);
                    Console.WriteLine($"Pause at {page}");
                    continue;
                }

                var data = await resp.Content.ReadAsStringAsync();


                if (JArray.Parse(data).Count < 50)
                {
                    cont = false;
                }



                File.WriteAllText($"{type}{page}.json", data);
                page++;
            } while (cont);
        }
    }
}

