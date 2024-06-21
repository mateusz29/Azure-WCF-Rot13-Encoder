using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("kodowanie");
            queue.CreateIfNotExists();

            while (true)
            {
                TimeSpan visibilityTimeout = TimeSpan.FromSeconds(5);
                var message = queue.GetMessage(visibilityTimeout: visibilityTimeout);
                if (message != null)
                {
                    try
                    {
                        string nazwa = message.AsString;
                        CloudBlobClient blobClient = account.CreateCloudBlobClient();
                        CloudBlobContainer input = blobClient.GetContainerReference("input");
                        input.CreateIfNotExists();
                        CloudBlockBlob inputBlob = input.GetBlockBlobReference(nazwa);

                        string tresc = inputBlob.DownloadText();

                        Random rand = new Random();
                        if (rand.Next(3) == 0)
                            throw new Exception("wyjatek");

                        string nowaTresc = ROT13(tresc);
                        CloudBlobContainer output = blobClient.GetContainerReference("output");
                        output.CreateIfNotExists();
                        CloudBlockBlob outputBlob = output.GetBlockBlobReference(nazwa);

                        var bytes = new ASCIIEncoding().GetBytes(nowaTresc);
                        using (var s = new MemoryStream(bytes))
                        {
                            outputBlob.UploadFromStream(s);
                        }

                        queue.DeleteMessage(message);

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        private string ROT13(string tresc)
        {
            char[] znaki = tresc.ToCharArray();
            for (int i = 0; i < znaki.Length; i++)
            {
                char znak = znaki[i];
                if (znak >= 'a' && znak <= 'm') znak = (char)(znak + 13);
                else if (znak >= 'n' && znak <= 'z') znak = (char)(znak - 13);
                else if (znak >= 'A' && znak <= 'M') znak = (char)(znak + 13);
                else if (znak >= 'N' && znak <= 'Z') znak = (char)(znak - 13);
                znaki[i] = znak;
            }
            return new string(znaki);
        }
    }
}
