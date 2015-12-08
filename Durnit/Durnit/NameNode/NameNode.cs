using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
            JsonSerializer serializer = new JsonSerializer();

            switch(durnitOp)
            {
                case "GetDatanodes":
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;

                    DataNodeInfo[] nodesToSend = GetDataNodes();

                    using (StreamWriter sw = new StreamWriter(response.OutputStream))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        foreach (DataNodeInfo info in nodesToSend)
                        {
                            serializer.Serialize(writer, info.URIAdress);
                        }
                        // {"ExpiryDate":new Date(1230375600000),"Price":0}
                    }
                    response.StatusCode = 200;
                    break;
                case "Heartbeat":
                    DataNodeInfo sentInfo;
                    using (StreamReader reader = new StreamReader(request.InputStream))
                    using (JsonReader JsonRead = new JsonTextReader(reader))
                    {
                        sentInfo = (DataNodeInfo)serializer.Deserialize(JsonRead, typeof(DataNodeInfo));
                    }

                    DataNodeInfo correspondingInfo = log.FirstOrDefault(x => x.ID == sentInfo.ID);
                    correspondingInfo.Files = sentInfo.Files;

                    response.StatusCode = 200;
                    break;
                default:
                    response.StatusCode = 404;
                    break;
            }
            //response.AddHeader("X-DurnitOp", "woah");

            response.Close();

            //Console.WriteLine("got it!");
        }

        private DataNodeInfo[] GetDataNodes()
        {
            throw new NotImplementedException();
        }
    }
}
