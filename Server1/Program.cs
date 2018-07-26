using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server1
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 52222;

            Task.Run(() =>
            {

                while (true)
                {
                    var udp = new UdpClient { EnableBroadcast = true };
                    udp.Send(new byte[0], 0, "255.255.255.255", port);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            });

            var clients = new List<TcpClient>();

            var tcpListener = new TcpListener(port);
            tcpListener.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    var client = tcpListener.AcceptTcpClient();
                    Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
                    clients.Add(client);
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            var message = client.ReceiveMessage();
                            switch (message.MessageType)
                            {
                                case MessageType.GetHostname:
                                    client.SendMessage(new Message
                                    {
                                        MessageType = MessageType.GetHostnameResponse,
                                        Payload = Encoding.UTF8.GetBytes(Dns.GetHostName())
                                    });
                                    break;
                            }
                        }
                    });
                }
            });

            var fw = new FileSystemWatcher(GetWatchDirectory())
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite
            };
            fw.Changed += (sender, eventArgs) =>
            {
                string fullPath = eventArgs.FullPath;
                fullPath = fullPath.Replace(".#", "");
                var extension = Path.GetExtension(fullPath);
                if (extension != ".xaml~*" && extension != ".xaml") return;
                var tildeIndex = fullPath.IndexOf('~');

                var path = tildeIndex > 0
                    ? fullPath.Substring(0, fullPath.IndexOf('~'))
                    : fullPath;

                Console.WriteLine(path);
                var xaml = "";
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    xaml = textReader.ReadToEnd();
                }

                clients.RemoveAll(x => !x.Connected);
                clients.SendMessage(new Message
                {
                    MessageType = MessageType.XamlUpdated,
                    Payload = Encoding.UTF8.GetBytes(xaml)
                });

            };
            Console.WriteLine($"Watching for file changes in {GetWatchDirectory()}");

            Console.ReadLine();
        }

        private static string GetWatchDirectory()
        {
            return "/Users/pranshu/Xenolt/Xamarin.Forms.Xaml.LiveReload";
        }
    }
}
