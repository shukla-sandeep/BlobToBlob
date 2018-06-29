using System;
using System.Diagnostics;
using Sandboxable.Microsoft.WindowsAzure.Storage;
using Sandboxable.Microsoft.WindowsAzure.Storage.Auth;
using Sandboxable.Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace ConsoleApp3
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                SameStorageAccount();
                //DifferentStorageAccounts();
            }
            catch(Exception ex)
            {
                log.Error($"Exception => {ex.Message}");
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

        private static void DifferentStorageAccounts()
        {
            CloudStorageAccount storageAccount1 = new CloudStorageAccount(new StorageCredentials(ConfigurationSettings.AppSettings["AccountName1"], ConfigurationSettings.AppSettings["AccountKey1"]), true);
            CloudBlobClient cloudBlobClientSource = storageAccount1.CreateCloudBlobClient();
            CloudBlobContainer sourceContainer = cloudBlobClientSource.GetContainerReference("container2");

            CloudStorageAccount storageAccount2 = new CloudStorageAccount(new StorageCredentials(ConfigurationSettings.AppSettings["AccountName2"], ConfigurationSettings.AppSettings["AccountKey2"]), true); 
            CloudBlobClient cloudBlobClientDestination = storageAccount2.CreateCloudBlobClient();
            CloudBlobContainer targetContainer = cloudBlobClientDestination.GetContainerReference("container1");

            CopyAndDelete(sourceContainer, targetContainer);
        }

        private static void SameStorageAccount()
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(ConfigurationSettings.AppSettings["AccountName1"], ConfigurationSettings.AppSettings["AccountKey1"]), true);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer sourceContainer = cloudBlobClient.GetContainerReference("container1");
            CloudBlobContainer targetContainer = cloudBlobClient.GetContainerReference("container2");
            CopyAndDelete(sourceContainer, targetContainer);
        }
        private static void CopyAndDelete(CloudBlobContainer sourceContainer, CloudBlobContainer targetContainer)
        {
            Console.WriteLine("In progress...");
            var blobList = sourceContainer.ListBlobs();
            foreach (CloudBlockBlob blobItem in blobList)
            {
                var watch = Stopwatch.StartNew();
                string blobName = blobItem.Name;
                CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(blobName);
                CloudBlockBlob targetBlob = targetContainer.GetBlockBlobReference(blobName);
                targetBlob.StartCopy(sourceBlob);
                Console.WriteLine($"Copying file => {blobName}");
                watch.Stop();
                log.Info($"File Name => {blobName}; Time Elapsed (milliseconds) => {watch.ElapsedMilliseconds}");
                sourceBlob.DeleteAsync();
            }
            Console.WriteLine("Copy Complete.");
        }
    }
}
