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
    enum PostKind
    {
        Link, Self, Image, Video, Videogif
    }

    class RedditSubmitter
    {
        private const string ACCESS_TOKEN_URL = "https://www.reddit.com/api/v1/access_token";
        private const string SUBMIT_URL = "https://oauth.reddit.com/api/submit";

        private readonly string username;
        private readonly string password;
        private readonly string clientId;
        private readonly string secret;

        private string accessToken;
        private DateTime? tokenExpires;

        private AuthenticationHeaderValue basicAuth;
        private HttpClient client;

        public RedditSubmitter(string username, string password, string clientId, string secret)
        {
            this.username = username;
            this.password = password;
            this.clientId = clientId;
            this.secret = secret;

            client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
            basicAuth = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Authorization = basicAuth;
            client.DefaultRequestHeaders.Add("User-Agent", "reddit-submitter by foxneZz");
        }

        public void Post(PostKind pk, bool resubmit, bool sendReplies, string subreddit, string title, string text = null, string url = null)
        {
            GetAccessToken();

            var values = new Dictionary<string, string>
            {
                { "api_type", "json" },
                { "kind", pk.ToString().ToLower() },
                { "resubmit", resubmit.ToString().ToLower() },
                { "sendreplies", sendReplies.ToString().ToLower() },
                { "sr", subreddit.ToLower() },
                { "title", title }
            };

            switch (pk)
            {
                case PostKind.Self:
                    if (text == null)
                        throw new Exception();

                    values.Add("text", text);
                    break;

                default:
                    if (url == null)
                        throw new Exception();

                    values.Add("url", url);
                    break;
            }

            var response = client.PostAsync(SUBMIT_URL, new FormUrlEncodedContent(values)).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(responseString);
        }

        private void GetAccessToken()
        {
            var response = client.PostAsync(ACCESS_TOKEN_URL, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            })).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                (accessToken, tokenExpires) = ExtractToken(responseString);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }
            else
                throw new Exception("Not successful");
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
