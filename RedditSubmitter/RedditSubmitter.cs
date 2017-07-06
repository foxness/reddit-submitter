using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
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

        private string accessToken;
        private DateTime? tokenExpires;

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

        public async Task GetAccessToken()
        {
            var response = await client.PostAsync(ACCESS_TOKEN_URL, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            }));

            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                (accessToken, tokenExpires) = ExtractToken(responseString);
            else
                throw new Exception(response.ToString());
        }

        private (string token, DateTime? expire) ExtractToken(string response)
        {
            /* {"access_token": "Booty", "token_type": "bearer", "expires_in": 3600, "scope": "*"} */

            string token = null;
            DateTime? expire = null;

            var r = new Regex(@"""(?<k>\w+)"": ((?<v>\d+)|""(?<v>[^""]*)"")");
            foreach (Match m in r.Matches(response))
            {
                switch (m.Groups["k"].Value)
                {
                    case "access_token": token = m.Groups["v"].Value; break;
                    case "expires_in": expire = DateTime.Now.AddSeconds(Int32.Parse(m.Groups["v"].Value)); break;
                }
            }

            if (token == null || expire == null)
                throw new Exception("no token bois");

            return (token, expire);
        }
    }
}
