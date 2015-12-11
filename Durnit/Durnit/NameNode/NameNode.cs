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
    public class NameNode : INode
    {
        private string ourDurnitOp = "X-DurnitOp";
        private List<DataNodeInfo> log;
        private string URI;

        public NameNode(string Address, string Port)
        {
            URI = "http://" + Address + ":" + Port + "/";
            log = new List<DataNodeInfo>();
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(URI);
            listener.Start();
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);
        }

        private void handleRequest(IAsyncResult ar)
        {
            Console.WriteLine("NameNode : handling request");
            HttpListener listener = (HttpListener)ar.AsyncState;
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);

            HttpListenerContext context = listener.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            NameValueCollection requestHeaders = context.Request.Headers;
            HttpListenerResponse response = context.Response;


            string durnitOp = requestHeaders.Get(ourDurnitOp).ToLower().Split(':')[0];
            JsonSerializer serializer = new JsonSerializer();


            switch (durnitOp)
            {
                case "getdatanodes":
                    handleGetDataNodes(request, response);
                    break;
                case "heartbeat":
                    handleHeartBeat(request, response);
                    break;
                default:
                    response.StatusCode = 404;
                    break;
            }
            response.Close();
        }

        private void handleHeartBeat(HttpListenerRequest request, HttpListenerResponse response)
        {
            JsonSerializer serializer = new JsonSerializer();
            DataNodeInfo sentInfo;
            using (StreamReader reader = new StreamReader(request.InputStream))
            using (JsonReader JsonRead = new JsonTextReader(reader))
            {
                sentInfo = (DataNodeInfo)serializer.Deserialize(JsonRead, typeof(DataNodeInfo));
            }
            lock(log)
            {
                DataNodeInfo correspondingInfo = log.FirstOrDefault(x => x.URIAdress == sentInfo.URIAdress);
                if (correspondingInfo != null)
                {
                    correspondingInfo.Files = sentInfo.Files;
                }
                else
                {
                    log.Add(sentInfo);
                }
                Console.WriteLine("HOW MANY I HAVE " + log.Count);
                foreach (var item in log)
                {
                    Console.WriteLine(item.URIAdress + " " + item.Files + " " + item.HowManyFriends + " " + item.connections);
                }
            }
            response.StatusCode = 200;
            response.Close();
        }

        private DataNodeInfo[] determineNewFriends(DataNodeInfo currentDataNode)
        {
            int howMany = 1;
            return log.Where(x => x.URIAdress != currentDataNode.URIAdress).OrderBy(x => x.HowManyFriends).Take(howMany).ToArray();
            //for (int i = 0; i < howMany; i++)
            //{
            //    if(!sorted[i].URIAdress.Equals(currentDataNode.URIAdress))
            //        friends[]
            //}

            //HashSet<int> indecies = new HashSet<int>();
            //Random generator = new Random();
            //while (indecies.Count != 4)
            //{
            //    indecies.Add(generator.Next(log.Count));
            //}
            //List<DataNodeInfo> returningList = new List<DataNodeInfo>();
            //foreach (int index in indecies)
            //{
            //    returningList.Add(log[index]);
            //}
            //return returningList.ToArray();
        }

        //expecting GetDatanodes:(number)
        private void handleGetDataNodes(HttpListenerRequest request, HttpListenerResponse response)
        {
            int howMany = int.Parse(request.Headers.Get(ourDurnitOp).Split(':')[1]);

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            DataNodeInfo[] nodesToSend = getDataNodesFromCount(howMany);

            using (StreamWriter sw = new StreamWriter(response.OutputStream))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                List<string> UrisToSend = new List<string>();
                foreach (DataNodeInfo info in nodesToSend)
                {
                    UrisToSend.Add(info.URIAdress);
                }
                serializer.Serialize(writer, UrisToSend);
                // {"ExpiryDate":new Date(1230375600000),"Price":0}
            }
            response.StatusCode = 200;
        }

        private DataNodeInfo[] getDataNodesFromCount(int howManyToReturn)
        {
            HashSet<int> indecies = new HashSet<int>();
            Random generator = new Random();
            while (indecies.Count != howManyToReturn)
            {
                indecies.Add(generator.Next(log.Count));
            }
            List<DataNodeInfo> returningList = new List<DataNodeInfo>();
            foreach (int index in indecies)
            {
                returningList.Add(log[index]);
            }
            return returningList.ToArray();
            //throw new NotImplementedException();
        }
    }
}
