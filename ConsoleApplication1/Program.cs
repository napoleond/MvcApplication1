using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ConsoleApplication1
{
    //NOTE: it would probably be nicer to reuse the interfaces from MvcApplication1Context,
    //but this will have to do for now

    public interface IListStorage<T>
    {
        void Add(T item);
    }

    public class ListStorage : IListStorage<Visitor>
    {
        private CloudBlockBlob visitorsBlob;

        public ListStorage(CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
            container.CreateIfNotExists();
            this.visitorsBlob = container.GetBlockBlobReference("visitors");
            try
            {
                this.visitorsBlob.FetchAttributes();
            }
            catch (Exception)
            {
                this.visitorsBlob.UploadText("[]");
            }
        }

        public void Add(Visitor item)
        {
            MemoryStream stream1 = new MemoryStream();
            MemoryStream stream2 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor[]));
            this.visitorsBlob.DownloadToStream(stream1);
            stream1.Position = 0;
            Visitor[] visitorsA = (Visitor[])ser.ReadObject(stream1);
            List<Visitor> visitorsL = visitorsA.Cast<Visitor>().ToList();
            visitorsL.Add(item);
            ser.WriteObject(stream2, visitorsL.ToArray());
            stream2.Position = 0;
            this.visitorsBlob.UploadFromStream(stream2);
        }
    }

    public interface IQueue<T>
    {
        T GetMessage();
        void DeleteMessage(T message);
    }

    public class Queue : IQueue<CloudQueueMessage>
    {
        private CloudQueue queue;

        public Queue(CloudStorageAccount storageAccount)
        {
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            this.queue = queueClient.GetQueueReference("visitors");
            this.queue.CreateIfNotExists();
        }

        public CloudQueueMessage GetMessage()
        {
            return this.queue.GetMessage();
        }

        public void DeleteMessage(CloudQueueMessage message)
        {
            this.queue.DeleteMessage(message);
        }
    }

    class Store
    {
        public IQueue<CloudQueueMessage> queue;
        public IListStorage<Visitor> listStore;

        public Store()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dnoel;AccountKey=SmfIMeCfHIwiqNb4ZfTwxmEHNj1UKkMKt5cvG3zGdcbbU6MfIn4iM+k5AbPPb09pCnVRFCcdh09pw+BSuz3Wlg==");

            this.queue = new Queue(storageAccount);
            this.listStore = new ListStorage(storageAccount);
        }
    }

    class Program
    {
        public static void Main()
        {
            Store store = new Store();
            Console.WriteLine("Waiting for messages...");
            while (true)
            {
                CloudQueueMessage retrievedMessage = store.queue.GetMessage();
                if (retrievedMessage != null)
                {
                    Console.WriteLine("Message received");
                    MemoryStream stream1 = new MemoryStream(retrievedMessage.AsBytes);
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor));
                    stream1.Position = 0;
                    Visitor visitor = (Visitor)ser.ReadObject(stream1);
                    store.listStore.Add(visitor);
                    Console.WriteLine("Message added");
                    store.queue.DeleteMessage(retrievedMessage);
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }
    }

    [DataContract]
    public class Visitor
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Phrase { get; set; }

        [DataMember]
        public DateTime Date { get; set; }
    }
}