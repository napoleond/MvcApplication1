using System;
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
        }

        public void Add(Visitor item)
        {
            MemoryStream stream1 = new MemoryStream();
            MemoryStream stream2 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor[]));
            this.visitorsBlob.DownloadToStream(stream1);
            stream1.Position = 0;
            List<Visitor> visitors = (List<Visitor>)ser.ReadObject(stream1);
            visitors.Add(item);
            ser.WriteObject(stream2, visitors.ToArray());
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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            this.queue = new Queue(storageAccount);
            this.listStore = new ListStorage(storageAccount);
        }
    }

    class Program
    {
        public static void Main()
        {
            Store store = new Store();
            while (true)
            {
                CloudQueueMessage retrievedMessage = store.queue.GetMessage();
                if (retrievedMessage != null)
                {
                    MemoryStream stream1 = new MemoryStream(retrievedMessage.AsBytes);
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Visitor));
                    stream1.Position = 0;
                    Visitor visitor = (Visitor)ser.ReadObject(stream1);
                    store.listStore.Add(visitor);
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