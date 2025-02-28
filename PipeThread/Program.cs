// See https://aka.ms/new-console-template for more information
using System.IO;
using System.IO.Pipes;

using (var server = new NamedPipeServerStream("ThreadPipe"))
            {
                Console.WriteLine("Waiting for connection...");
                server.WaitForConnection();

                using (var reader = new StreamReader(server))
                {
                        string line = reader.ReadToEnd();
                        Console.WriteLine($"Received from client: {line}");
                        //Thread.Sleep(30000);
                }
            }