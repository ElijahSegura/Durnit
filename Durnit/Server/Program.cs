using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            socket.Bind(new IPEndPoint(0, 8080));
            socket.Listen(0);

            Socket accept = socket.Accept();

            byte[] sent = Encoding.Default.GetBytes("Message Received!");
            accept.Send(sent, 0, sent.Length, 0);

            byte[] received = new byte[255];

            int rec = accept.Receive(received, 0, received.Length, 0);

            Array.Resize(ref received, rec);

            Console.WriteLine("Received: {0}", Encoding.Default.GetString(received));

            socket.Close();
            accept.Close();
            
            Console.Read();
        }
    }
}
