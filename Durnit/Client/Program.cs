using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            socket.Connect(endPoint);

            byte[] message = Encoding.Default.GetBytes("ayy");
            socket.Send(message, 0, message.Length, 0);

            byte[] received = new byte[255];
            int rec = socket.Receive(received, 0, received.Length, 0);

            Array.Resize(ref received, rec);

            Console.WriteLine("Received: {0}", Encoding.Default.GetString(received));

            socket.Close();

            Console.Read();
        }
    }
}
