using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RedditSubmitter
{
    class RedditSubmitter
    {
        private const string ACCESS_TOKEN_URL = "https://www.reddit.com/api/v1/access_token";

        private readonly string username;
        private readonly string password;
        private readonly string clientId;
        private readonly string secret;

        private HttpClient client;

        public RedditSubmitter(string username, string password, string clientId, string secret)
        {
            this.username = username;
            this.password = password;
            this.clientId = clientId;
            this.secret = secret;

            client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task Authorize()
        {
            var response = await client.PostAsync(ACCESS_TOKEN_URL, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            }));

            var responseString = await response.Content.ReadAsStringAsync();

            /*if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(response);
            }*/

            Console.WriteLine(response);
        }
    }
}
