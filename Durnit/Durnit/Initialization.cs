using Durnit.Exceptions;
using Durnit.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Durnit
{
    public class Initialization
    {
        public NameNodeInfo Start(string configXmlPath = null, long timeout = -1)
        {
            bool pathExists = false;
            NameNodeInfo nni = null;
            if (configXmlPath != null)
            {
                if (pathExists = File.Exists(configXmlPath))
                    nni = ParseInstructions(configXmlPath);
            }
            if (!pathExists)
            {
                nni = WaitForInstructions(timeout);
            }
            return nni;
        }

        //TODO:
        public NameNodeInfo WaitForInstructions(long timeout = -1)
        {
            Console.WriteLine("Waiting...");
            return null;
        }

        //TODO:
        public INode InitializeSelf(InitInstructionModel init)
        {
            INode returningNode = null;
            switch (init.Instruction)
            {
                case InitInstructions.DATANODE:
                    returningNode = new DataNode(init);
                    //TODO: Start dataNode
                    break;
                case InitInstructions.NAMENODE:
                    returningNode = new NameNode(init.Address, init.Port);
                    //TODO: Start nameNode
                    break;
                default:
                    break;
            }
            return returningNode;
        }


        List<InitInstructionModel> instructions = new List<InitInstructionModel>();
        InitInstructionModel myInstruction = null;

        public NameNodeInfo ParseInstructions(string configXmlPath)
        {
            Stack<string> elements = new Stack<string>();

            XmlTextReader reader = new XmlTextReader(configXmlPath);

            //int currentIndent = 0;
            string lastValue = null;
            string encapsulatingElement = null;
            InitInstructionModel ins = null;
            bool identityIns = false;
            bool nameNodeKnown = false;
            NameNodeInfo nni = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        elements.Push(reader.Name);
                        //for (int i = 0; i < currentIndent; i++)
                        //    Console.Write("-");
                        //currentIndent++;
                        string elem = reader.Name.ToUpper();
                        if (elem == "IDENTITY")
                        {
                            identityIns = true;
                        }
                        else if (elem == "DATANODE")
                        {
                            if (ins == null) ins = new InitInstructionModel();
                            ins.Instruction = InitInstructions.DATANODE;
                        }
                        else if (elem == "NAMENODE")
                        {
                            if (ins == null) ins = new InitInstructionModel();
                            if (nameNodeKnown) throw new SyntaxErrorException("Cannot have more than one name node in this setup");
                            nameNodeKnown = true;
                            ins.Instruction = InitInstructions.NAMENODE;
                        }
                        //Console.WriteLine(reader.Name);
                        break;
                    case XmlNodeType.Text:
                        //for (int i = 0; i < currentIndent; i++)
                        //    Console.Write("-");
                        string lastElem = elements.Peek().ToUpper();
                        if (lastElem == "ADDRESS")
                        {
                            ins.Address = reader.Value;
                        }
                        else if (lastElem == "PORT")
                        {
                            ins.Port = reader.Value;
                        }
                        //Console.WriteLine("=> " + reader.Value);
                        lastValue = reader.Value;
                        encapsulatingElement = elements.Peek();
                        break;
                    case XmlNodeType.EndElement:
                        if (elements.Pop() == reader.Name)
                        {
                            if (reader.Name.ToUpper() == "NAMENODE")
                            {
                                nameNodeKnown = true;
                                nni = new NameNodeInfo();
                                nni.Address = ins.Address;
                                nni.Port = ins.Port;
                            }
                            if (!identityIns && reader.Name.ToUpper().EndsWith("NODE"))
                            {
                                instructions.Add(ins);
                                ins = null;
                            }


                            if (reader.Name.ToUpper() == "IDENTITY" && identityIns)
                            {
                                identityIns = false;
                                if (myInstruction == null)
                                    myInstruction = ins;
                                else
                                    throw new SyntaxErrorException("Cannot set more than one instruction for self");
                                ins = null;
                            }
                            //currentIndent--;
                            //else if (reader.Name.ToUpper().StartsWith("CONFIG"))
                            //{
                            //    instructions.Add(ins);
                            //    ins = null;
                            //}
                        }
                        else
                            throw new SyntaxErrorException("Trying to end element that was not initialized: " + reader.Name);
                        break;
                }
            }

            InitializeSelf(myInstruction);
            SendInstructions();

            return nni;

            //Console.WriteLine("SELF: " + myInstruction.Instruction);

            //foreach (InitInstructionModel mod in instructions)
            //{
            //    Console.WriteLine(mod.Instruction);
            //}
        }

        //TODO:
        public void SendInstructions()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            foreach (var iim in instructions)
            {
                WebRequest request = WebRequest.Create("http://" + iim.Address + ":" + iim.Port);
                request.Method = "POST";
                request.ContentType = "application/json";


                using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, iim);

                    if (iim.Instruction == InitInstructions.NAMENODE)
                    {
                        foreach (var iim2 in instructions)
                        {
                            if (iim2.Instruction == InitInstructions.DATANODE)
                                serializer.Serialize(writer, iim2);
                        }
                    }

                    // {"ExpiryDate":new Date(1230375600000),"Price":0}
                }
                request.GetResponse();
            }
        }
    }

    public enum InitInstructions
    {
        DATANODE,
        NAMENODE,
        NONE
    }
}