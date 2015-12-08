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
            HttpListenerRequest request = context.Request;
            NameValueCollection requestHeaders = context.Request.Headers;
            HttpListenerResponse response = context.Response;
            
            string durnitOp = requestHeaders.Get("X-DurnitOp");
            switch(durnitOp)
            {
                case "GetDatanodes":

                    response.StatusCode = 200;
                    break;
                case "Heartbeat":
                    //List<string> data = (List<string>)request.InputStream;
                    response.StatusCode = 200;
                    break;
                default:
                    response.StatusCode = 404;
                    break;
            }
            //response.AddHeader("X-DurnitOp", "woah");

            response.Close();

            Console.WriteLine("got it!");
        }
    }
}
