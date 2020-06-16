using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AutoAddProducts
{
    /// <summary>
    /// This function call the CognitiveService Computer Vision to analyze the pictures
    /// TRIGGER : queue message new product to anazyle
    /// OUTPUT : if forbidden product (nude, racis) -> msg in specific queue, otherwide newproductdetailsmessage
    /// </summary>
    public static class AnalyzeNewProductBlobFunction
    {
        [FunctionName("AnalyzeNewProductBlobFunction")]
        public static void Run([BlobTrigger("newproduct-to-add/{name}", Connection = "WoodgroveCatalogStorage")] Stream newProductBlob, string name,
            [Queue("newproductdetails", Connection = "WoodgroveCatalogStorage")]out string newProductDetailsMsg,
            [Queue("forbiddenproducts", Connection = "WoodgroveCatalogStorage")]out string forbiddenProductMsg,
            ILogger log)
        {
            log.LogInformation($"Analyse new product image : {name}   (size={newProductBlob.Length})");

            // TODO : call cog svc 


            // TODO : if image contains adult stuff -> generate a forbiden message
            forbiddenProductMsg = null;
            
            // TODO : generate message for DB update
            newProductDetailsMsg = "{ \"productId\":\""+ Guid.NewGuid().ToString() + "\", \"productName\":\"" + name +"\", \"descriptionUS\":\"this is new product\", \"descriptionFR\":\"Contiendra la traduction FR\", \"descriptionDE\":\"Spater das deutsche Version\" }";
        }
    }
}
