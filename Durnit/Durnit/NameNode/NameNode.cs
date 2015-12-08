using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Durnit
{
    class NameNode
    {
        private List<DataNodeInfo> log;
        static string URI = "http://localhost:8080/";
        public NameNode()
        {
            log = new List<DataNodeInfo>();
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(URI);
            listener.Start();
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);
        }

        private void handleRequest(IAsyncResult ar)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);

            HttpListenerContext context = listener.EndGetContext(ar);
            NameValueCollection requestHeaders = context.Request.Headers;
            HttpListenerResponse response = context.Response;
            for (int i = 0; i < requestHeaders.Count; i++)
            {
                Console.WriteLine(requestHeaders.Get(i));
            }

            response.AddHeader("X-Durnit", "woah");

            response.Close();

            Console.WriteLine("got it!");
        }
    }
}
