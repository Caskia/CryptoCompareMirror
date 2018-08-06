using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoCompareMirror.ConnectionHandlers
{
    public class ContentManager
    {
        private static ContentManager _instance = null;

        private List<string> _contents = new List<string>();
        private string _filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/contents.txt";

        public ContentManager()
        {
            ReadContents();
        }

        public static ContentManager Instance
        {
            get { return _instance ?? (_instance = new ContentManager()); }
        }

        public List<string> GetContents()
        {
            return _contents;
        }

        private void ReadContents()
        {
            var lines = File.ReadAllLines(_filePath);
            _contents = lines.ToList();
        }
    }

    public class MarketConnectionHandler : ConnectionHandler
    {
        public MarketConnectionHandler()
        {
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            WriteMessageTask(connection);

            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    connection.Transport.Input.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }

        private void WriteMessageTask(ConnectionContext connection)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        foreach (var line in ContentManager.Instance.GetContents())
                        {
                            connection.Transport.Output.WriteAsync(Encoding.UTF8.GetBytes(line));
                            Thread.Sleep(10);
                        }
                    }
                }
                catch { }
            });
        }
    }
}