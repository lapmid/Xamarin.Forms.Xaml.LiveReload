using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace VSIXHelloWorld
{
   public class LiveXaml 
    {
       
        public  string FilePath;
        public  void Main(string DirPath)
        {
            FilePath = DirPath;
            var port = 52222;

            Task.Run(() =>
            {

                while (true)
                {
                    var udp = new UdpClient { EnableBroadcast = true };
                    udp.Send(new byte[0], 0, "255.255.255.255", port);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
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

            var fw = new FileSystemWatcher(FilePath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite
            };
            fw.Changed += (sender, eventArgs) =>
            {
                string fullPath = eventArgs.FullPath;
                fullPath = GetWatchDirectory(DirPath);
                fullPath = fullPath.Replace(".#", "");
                var extension = Path.GetExtension(fullPath);
                if (extension == ".xaml~*" || extension == ".xaml")
                {
                    MessageBox.Show(fullPath);
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
                }
               

            };
            //Console.WriteLine($"Watching for file changes in {GetWatchDirectory(DirPath)}");

            Console.ReadLine();
        }

        private static string GetWatchDirectory(string path)
        {

            string ms= Helper.main();
            
            if (String.IsNullOrEmpty(ms))
                return path;

            return ms;
        }

        

        
       
    }
}
