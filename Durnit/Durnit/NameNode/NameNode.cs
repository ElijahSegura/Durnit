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
            //while (true) { }
        }

        private void handleRequest(IAsyncResult ar)
        {
            Console.WriteLine("NameNode : handling request");
            HttpListener listener = (HttpListener)ar.AsyncState;
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);
            Console.WriteLine("Queued up another one: " + this.URI);

            HttpListenerContext context = listener.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            NameValueCollection requestHeaders = context.Request.Headers;
            HttpListenerResponse response = context.Response;


            string durnitOp = requestHeaders.Get(ourDurnitOp).ToLower().Split(':')[0];

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
                DataNodeInfo correspondingInfo = log.FirstOrDefault(x => x.URIAddress == sentInfo.URIAddress);
                if (correspondingInfo != null)
                {
                    correspondingInfo.Files = sentInfo.Files;
                    correspondingInfo.HowManyFriends = sentInfo.HowManyFriends;
                }
                else
                    log.Add(sentInfo);
            }

            response.StatusCode = 200;
            response.Close();
            sendOverNewFriends(sentInfo);
        }

        private void sendOverNewFriends(DataNodeInfo currentDataNode)
        {
            if (needMoreFriends(currentDataNode))
            {
                DataNodeInfo[] newFriends = determineNewFriends(currentDataNode);
                List<string> urisToSend = new List<string>();
                foreach (DataNodeInfo friend in newFriends)
                {
                    urisToSend.Add(friend.URIAddress);
                }
                HttpWebRequest newRequest = (HttpWebRequest)WebRequest.Create(currentDataNode.URIAddress);
                newRequest.Headers.Add(ourDurnitOp, "NewFriends");
                newRequest.Method = "POST";
                JSONWriteToStream(newRequest.GetRequestStream(), urisToSend);
                newRequest.GetResponse();
            }
        }

        private void JSONWriteToStream(Stream stream, object whatToWrite)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(stream))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, whatToWrite);
            }
        }

        private bool needMoreFriends(DataNodeInfo info)
        {
            int properAmountOfFriends = log.Count / 2 + 1;
            return info.HowManyFriends < properAmountOfFriends;
        }

        private DataNodeInfo[] determineNewFriends(DataNodeInfo currentDataNode)
        {
            int howMany = 1;
            return log.Where(x => x.URIAddress != currentDataNode.URIAddress).OrderBy(x => x.HowManyFriends).Take(howMany).ToArray();
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

            List<string> UrisToSend = new List<string>();
            foreach (DataNodeInfo info in nodesToSend)
            {
                UrisToSend.Add(info.URIAddress);
            }

            JSONWriteToStream(response.OutputStream, UrisToSend);
            response.StatusCode = 200;
        }

        private DataNodeInfo[] getDataNodesFromCount(int howManyToReturn)
        {
            return log.OrderByDescending(x => x.Files.Count).Take(howManyToReturn).ToArray();
            //HashSet<int> indecies = new HashSet<int>();
            //Random generator = new Random();
            //while (indecies.Count != howManyToReturn)
            //{
            //    indecies.Add(generator.Next(log.Count));
            //}
            //List<DataNodeInfo> returningList = new List<DataNodeInfo>();
            //foreach (int index in indecies)
            //{
            //    returningList.Add(log[index]);
            //}
            //return returningList.ToArray();
            ////throw new NotImplementedException();
        }
    }
}
