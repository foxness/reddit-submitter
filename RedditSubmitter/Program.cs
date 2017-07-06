using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSubmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Console.ReadLine();
        }

        static async void Run()
        {
            var lines = File.ReadAllLines("secret.txt");
            var clientId = lines[0];
            var secret = lines[1];
            var username = lines[2];
            var password = lines[3];

            var rs = new RedditSubmitter(username, password, clientId, secret);
            await rs.Authorize();
        }
    }
}
