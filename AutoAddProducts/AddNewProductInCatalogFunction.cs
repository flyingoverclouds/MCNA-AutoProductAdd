using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace AutoAddProducts
{
    /// <summary>
    /// This function create the new product entry in the cosmos catalog database
    /// 
    /// TRIGGER : queue message
    /// OUTPUT : cosmoddb document
    /// </summary>
    public static class AddNewProductInCatalogFunction
    {
        [FunctionName("AddNewProductInCatalogFunction")]
        public static void Run([QueueTrigger("newproductdetails", Connection = "WoodgroveCatalogStorage")]string productDetailsMsg,
            [Queue("addedproducts", Connection = "WoodgroveCatalogStorage")] out string addproductConfirmationMsg,
            ILogger log)
        {
            log.LogInformation($"Product details message : {productDetailsMsg}");

            // TOTO  : call Translation cognitive service

            // TODO : get the json content from the message and insert/update the document in CosmosDB

            // TODO : extract product ID & name to generate a confirmation message, or update metadata on blob, or ....
            addproductConfirmationMsg = "ADDED : " + productDetailsMsg;
        }
    }
}
