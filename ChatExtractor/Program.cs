using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChatExtractor
{
    class Program
    {
        struct Log
        {
            public string name, server;
            public List<Chat> chats;
        }

        struct Chat
        {
            public string name, content, time;
        }

        static void Main(string[] args)
        {
            var clientRegex = new Regex(@"Searching (.+)[\\/](.+)[\\/].minecraft[\\/]mods for mods");
            var serverRegex = new Regex("Connecting to (.+)");
            var regex = new Regex(@"\[(\d\d:\d\d:\d\d)\] .+ \[CHAT\] (\[.+\])?(<(.+)> )?(.+)");
            /*if (!File.Exists("logs") || !File.GetAttributes("logs").HasFlag(FileAttribute.Directory))
            {
                Console.Error.WriteLine("请在 Everything 使用 !debug .log.gz|latest.log 过滤所有的文件并粘贴到 logs 文件夹下。");
                return;
            }*/

            Directory.CreateDirectory("logs_json");

            Task.WaitAll(Directory.EnumerateFiles("logs")
                .Select(logFile => Task.Run(() =>
                {
                    var chats = new List<Chat>();
                    string server = null;
                    string client = null;

                    
                    Stream stream = new FileStream(logFile, FileMode.Open);
                    if (logFile.EndsWith(".gz"))
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    }

                    var reader = new StreamReader(stream);

                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine()!;
                        if (client == null)
                        {
                            var collection = clientRegex.Matches(line);
                            if (collection.Count >= 1)
                            {
                                client = collection[0].Groups[2].Value;
                            }
                        }

                        if (server == null)
                        {
                            var collection = serverRegex.Matches(line);
                            if (collection.Count >= 1)
                            {
                                server = collection[0].Groups[1].Value;
                            }
                        }

                        var c = regex.Matches(line);
                        if (c.Count >= 1)
                        {
                            chats.Add(new Chat {name = c[0].Groups[4].Value, content = c[0].Groups[5].Value, time = c[0].Groups[1].Value});
                        }
                    }

                    var log = new Log {name = client, server = server, chats = chats};

                    File.WriteAllText(
                        $"logs_json{Path.DirectorySeparatorChar}{client}-{Path.GetFileNameWithoutExtension(logFile)}.json", JsonConvert.SerializeObject(log, Formatting.Indented));

                    reader.Close();
                    stream.Close();
                })).ToArray());
            Console.WriteLine("Hello World!");
        }
    }
}
