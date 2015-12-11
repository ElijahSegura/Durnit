using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            
            WebRequest request = WebRequest.Create("http://localhost:8080/");

            request.Headers.Add("X-DurnitOp", "GetDatanodes:2");

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            StreamReader reader = new StreamReader();

            


            response.Close();
        }
    }
}
