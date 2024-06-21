using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public void Koduj(string nazwa, string tresc)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("input");
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference(nazwa);

            var bytes = new ASCIIEncoding().GetBytes(tresc);
            using (var s = new MemoryStream(bytes))
            {
                blob.UploadFromStream(s);
            }

            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("kodowanie");
            queue.CreateIfNotExists();
            for(int i = 0; i < 1000; i++)
            {
                queue.AddMessage(new CloudQueueMessage(nazwa+i.ToString()));
            }
        }

        public string Pobierz(string nazwa)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("output");
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference(nazwa);

            using (var s2 = new MemoryStream())
            {
                blob.DownloadToStream(s2);
                string content = Encoding.UTF8.GetString(s2.ToArray());
                return content;
            }
        }
    }
}
