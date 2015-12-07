using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;


namespace Durnit
{
    public class DataNode
    {
        private const int HEARTBEAT_RATE = 5000;

        public List<DataNodeModel> replication = new List<DataNodeModel>();

        public List<string> DataStored = new List<string>();

        public DataNode()
        {
            new Thread(beginOperation).Start();
            new Thread(ConstantHeartBeat).Start();
        }


        static string URI = "http://localhost:8080/";
        private void beginOperation()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(URI);
            listener.Start();
            IAsyncResult context = listener.BeginGetContext(new AsyncCallback(handleRequest), listener);
        }

        private void handleRequest(IAsyncResult ar)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;
            listener.BeginGetContext(new AsyncCallback(handleRequest), listener);

            HttpListenerContext context = listener.EndGetContext(ar);

            if (context.Request.HttpMethod.Equals("POST"))
            {
                if (context.Request.Headers["X-DurnitOp"].Equals("Replication"))
                {
                    string fileName = context.Request.Headers["X-FileName"];
                    DataReplication(context.Request, fileName);
                }
                else if (context.Request.Headers["X-DurnitOp"].Equals("DataCreation"))
                {
                    string fileName = context.Request.Headers["X-FileName"];
                    DataCreation(context.Request, fileName);
                }
                else if (context.Request.Headers["X-DurnitOp"].Equals("NewFriend"))
                {
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

        private void ConstantHeartBeat()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(HEARTBEAT_RATE);
                HeartBeat();
            }
        }

        private void HeartBeat()
        {
            string requestData = "";
            foreach (string datum in DataStored)
            {
                requestData += datum + "-";
            }
            byte[] requestBytes = new byte[requestData.Length];
            char[] data = requestData.ToCharArray();
            for (int i = 0; i < requestData.Length; i++)
            {
                requestBytes[i] = (byte)data[i];
            }
            string URI = "http://NameNode:0000/";
            HttpWebRequest request = WebRequest.CreateHttp(URI);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = requestBytes.Length;
            request.Headers.Add("X-DurnitOp=Heartbeat");
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(requestBytes, 0, requestBytes.Length);
        }

        private void RequestReplication(string file)
        {
            byte[] requestBytes = File.ReadAllBytes(file);
            foreach(DataNodeModel DNM in replication ){
                string URI = DNM.URI;
                HttpWebRequest request = WebRequest.CreateHttp(URI);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestBytes.Length;
                request.Headers.Add("X-DurnitOp=Replication");
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(requestBytes, 0, requestBytes.Length);
            }
        }

        private void getFriends()
        {
            string URI = "http://NameNode:0000/";
            HttpWebRequest request = WebRequest.CreateHttp(URI);
            request.Method = "GET";
            request.Headers.Add("X-DurnitOp=DeadFriends");
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            char[] responseBytes = new char[dataStream.Length];
            for (int i = 0; i < responseBytes.Length; i++)
            {
                responseBytes[i] = (char)dataStream.ReadByte();
            }
            List<string> newFriends = new List<string>();
            string word ="";
            for (int j = 0; j < responseBytes.Length; j++)
            {
                if (responseBytes[j] != ';')
                {
                    word += responseBytes[j];
                }
                else
                {
                    newFriends.Add(word);
                    word = "";
                }
            }
            replication = new List<DataNodeModel>();
            foreach (string s in newFriends)
            {
                replication.Add(new DataNodeModel(s));
            }
        }

        private void DataCreation(HttpListenerRequest request, string file)
        {
            DataReplication(request, file);
            RequestReplication(file);
        }

        private void DataReplication(HttpListenerRequest request, string file)
        {
            byte[] theData = new byte[request.InputStream.Length];
            request.InputStream.Read(theData, 0, theData.Length);
            File.WriteAllBytes(file, theData);
        }

        private void NewFriend(HttpListenerRequest request)
        {
            byte[] theData = new byte[request.InputStream.Length];
            request.InputStream.Read(theData, 0, theData.Length);
            string word = "";
            for (int j = 0; j < theData.Length; j++)
            {
                if (theData[j] != ';')
                {
                    word += theData[j];
                }
                else
                {
                    replication.Add(new DataNodeModel(word));
                    word = "";
                }
            }
        }

        private void GetData(HttpListenerContext context, string file)
        {
            byte[] dataBytes = File.ReadAllBytes(file);
            HttpListenerResponse response = context.Response;
            response.OutputStream.Write(dataBytes, 0, dataBytes.Length);
        }
    }
}
