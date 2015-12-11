using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Durnit.Models;
using System.Text.RegularExpressions;


namespace Durnit
{
    public class DataNode : INode
    {
        private const int HEARTBEAT_RATE = 3000;

        /// <summary>
        /// A list of DataNodeInfo objects
        /// </summary>

        private bool inOperation;

        private DataNodeInfo selfInfo;


        public DataNode(InitInstructionModel info)
        {
            selfInfo = new DataNodeInfo();
            Console.WriteLine("data node initialized");
            selfInfo.URIAddress = "http://" + info.Address + ":" + info.Port + "/";
            selfInfo.connections = new List<string>();
            nameNodeURI = "http://" + info.NameNodeAddress + ":" + info.NameNodePort + "/";
            new Thread(beginOperation).Start();
            new Thread(ConstantHeartBeat).Start();
        }

        private string nameNodeURI { get; set; }

        /// <summary>
        /// Begins the Data Node's operation
        /// </summary>
        private void beginOperation()
        {
            inOperation = true;
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(selfInfo.URIAddress);
            listener.Start();
            while (inOperation)
            {
            IAsyncResult context = listener.BeginGetContext(new AsyncCallback(handleRequest), listener);
        }
        }

        /// <summary>
        /// Handles a request by distributing it to the proper method
        /// </summary>
        /// <param name="ar"></param>
        private void handleRequest(IAsyncResult ar)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);

            HttpListenerContext context = listener.EndGetContext(ar);

            if (context.Request.HttpMethod.Equals("POST"))
            {
                Console.WriteLine(context.Request.Headers["X-DurnitOp"]);
                if (context.Request.Headers["X-DurnitOp"].Equals("Replication"))
                {
                    string fileName = context.Request.Headers["X-FileName"];
                    DataReplication(context.Request, fileName);
                }
                else if (context.Request.Headers["X-DurnitOp"].Equals("DataCreation"))
                {
                    DataCreation(context.Request);
                }
                else if (context.Request.Headers["X-DurnitOp"].Equals("NewFriends"))
                {
                    Console.WriteLine("new friending");
                    NewFriend(context.Request);
                }
            }
            else
            {
                if (context.Request.Headers["X-DurnitOp"].Equals("Data"))
                {
                    string fileName = context.Request.Headers["X-FileName"];
                    GetData(context, fileName);
                }
            }
        }

        /// <summary>
        /// Call once, in a seperate thread, to maintain a constant heartbeat rate.
        /// </summary>
        private void ConstantHeartBeat()
        {
            while (true)
            {
                Console.WriteLine(selfInfo.URIAddress + "queued up heartbeat");
                System.Threading.Thread.Sleep(HEARTBEAT_RATE);
                HeartBeat();
            }
        }


        /// <summary>
        /// Send information to the name node containing the files this node stores and
        /// </summary>
        private void HeartBeat()
        {
            Console.WriteLine(selfInfo.URIAddress + "heartbeat");

            HttpWebRequest request = WebRequest.CreateHttp(nameNodeURI);
            request.Method = "POST";
            request.Headers.Add("X-DurnitOp", "Heartbeat");

            using (Stream dataStream = request.GetRequestStream())
            using (StreamWriter sw = new StreamWriter(dataStream))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                JsonWriter JW = new JsonTextWriter(sw);
                serializer.Serialize(JW, selfInfo);
            }
                request.GetResponse().Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
<<<<<<< HEAD
          
            request.GetResponse().Close();
        }

        /// <summary>
        /// Request that the DataNodes that this node is connected to replecate a file
        /// </summary>
        /// <param name="file">The path to the file to be replicated</param>
        private void RequestReplication(string file)
        {
            byte[] requestBytes = File.ReadAllBytes(file);
            foreach (string URI in selfInfo.connections)
            {
                HttpWebRequest request = WebRequest.CreateHttp(URI);
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
                request.ContentLength = requestBytes.Length;
                request.Headers.Add("X-DurnitOp", "Replication");
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(requestBytes, 0, requestBytes.Length);
                request.GetResponse().Close() ;
            }
        }

        /// <summary>
        /// Creates data on the node, requests the nodes that this node is in contact with replicate the same data
        /// </summary>
        /// <param name="request">The request which started the creation</param>
        /// <param name="file">The file path</param>
        private void DataCreation(HttpListenerRequest request){
            string contentDisposition = request.Headers["content-disposition"];
            Regex filePattern = new Regex("(\\w+.\\w*)");
            string file = filePattern.Match(contentDisposition).ToString();
            DataReplication(request, file);
            RequestReplication(file);
        }

        /// <summary>
        /// Adds data to the node. Does not call for further replication
        /// </summary>
        /// <param name="request">The request for replication</param>
        /// <param name="file">the file path</param>
        private void DataReplication(HttpListenerRequest request, string file)
        {
            byte[] theData = new byte[request.InputStream.Length];
            request.InputStream.Read(theData, 0, theData.Length);
            File.WriteAllBytes(file, theData);
            selfInfo.Files.Add(file);
        }

        /// <summary>
        /// Add 1 new node to this nodes replication shadow
        /// </summary>
        /// <param name="request">the request which asked for new friend creation</param>
        private void NewFriend(HttpListenerRequest request)
        {
            JsonSerializer Jace = new JsonSerializer();
            
            byte[] theData = new byte[request.InputStream.Length];
            request.InputStream.Read(theData, 0, theData.Length);
            string json = "";
            for (int j = 0; j < theData.Length; j++)
            {
                    json += theData[j];
            }
            List<string> addresses = JsonConvert.DeserializeObject<List<string>>(json);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="file"></param>
        private void GetData(HttpListenerContext context, string file)
        {
            byte[] dataBytes = File.ReadAllBytes(file);
            HttpListenerResponse response = context.Response;
            response.OutputStream.Write(dataBytes, 0, dataBytes.Length);
        }
    }
}
