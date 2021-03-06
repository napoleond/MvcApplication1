using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MvcApplication1.Models
{
    public interface ICommandAgent
    {
        void SendCommand(object command);
    }

    public class CommandQueueAgent : ICommandAgent
    {
        private CloudQueue queue;

        public CommandQueueAgent()
        {
            //so obviously the connection string shouldn't be hard coded, but using web.config wasn't
            //working as expected and debugging proved difficult with my current setup...
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dnoel;AccountKey=SmfIMeCfHIwiqNb4ZfTwxmEHNj1UKkMKt5cvG3zGdcbbU6MfIn4iM+k5AbPPb09pCnVRFCcdh09pw+BSuz3Wlg==");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            this.queue = queueClient.GetQueueReference("visitors");
            this.queue.CreateIfNotExists();
        }

        public void SendCommand(object command)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor));
            ser.WriteObject(stream1, command);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            CloudQueueMessage message = new CloudQueueMessage(sr.ReadToEnd());
            queue.AddMessage(message);
        }
    }

    public interface IListFactory<T>
    {
        IEnumerable<T> GetList();
    }

    public class VisitorListFactory : IListFactory<Visitor>
    {
        private CloudBlockBlob visitorsBlob;
        public VisitorListFactory()
        {
            //see note above
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dnoel;AccountKey=SmfIMeCfHIwiqNb4ZfTwxmEHNj1UKkMKt5cvG3zGdcbbU6MfIn4iM+k5AbPPb09pCnVRFCcdh09pw+BSuz3Wlg==");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
            container.CreateIfNotExists();
            this.visitorsBlob = container.GetBlockBlobReference("visitors");
            try
            {
                this.visitorsBlob.FetchAttributes();
            }
            catch (Exception ex)
            {
                this.visitorsBlob.UploadText("[]");
            }
        }

        public IEnumerable<Visitor> GetList()
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor[]));
            this.visitorsBlob.DownloadToStream(stream1);
            stream1.Position = 0;
            return (IEnumerable<Visitor>)ser.ReadObject(stream1);
        }
    }

    public class MvcApplication1Context
    {
        //this had DB stuff in it; empty now
    }
}