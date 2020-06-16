using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
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
        public static void Run([BlobTrigger("newproduct-to-add/{name}", Connection = "WoodgroveCatalogStorage")] Stream newProductBlobStream, string name,
            [Queue("newproductdetails", Connection = "WoodgroveCatalogStorage")]out string newProductDetailsMsg,
            [Queue("forbiddenproducts", Connection = "WoodgroveCatalogStorage")]out string forbiddenProductMsg,
            ILogger log)
        {
            log.LogInformation($"Analyse new product image : {name}   (size={newProductBlobStream.Length})");

            // TODO : call cog svc 
            var visionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(System.Environment.GetEnvironmentVariable("csVisionKey")))
            {
                Endpoint = System.Environment.GetEnvironmentVariable("csVisionEndpoint")
            };

            var features = new VisualFeatureTypes[] {
                    VisualFeatureTypes.Adult,
                    VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Color,
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces,
                    VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Tags
                };

            var imgAnalysis = visionClient.AnalyzeImageInStreamAsync(newProductBlobStream, features).GetAwaiter().GetResult();

            PrintAnalysisResult(imgAnalysis,log); // debug print
            
            // if image contains adult/gory/racy stuff -> generate a forbiden message and do not add product in db
            if (imgAnalysis.Adult.IsAdultContent || imgAnalysis.Adult.IsGoryContent || imgAnalysis.Adult.IsRacyContent)
            {
                newProductDetailsMsg = null; // forbidden product -> no new product message 
                forbiddenProductMsg = $"{name} : adult({imgAnalysis.Adult.AdultScore})   gory({imgAnalysis.Adult.GoreScore})   racy({imgAnalysis.Adult.RacyScore})";
                return;
            }
            //var imgDescription = visionClient.DescribeImageInStreamAsync(newProductBlobStream).GetAwaiter().GetResult();

            forbiddenProductMsg = null; // valid image -> no forbidden message
            
            // TODO : generate message for DB update
            newProductDetailsMsg = "{ \"productId\":\"" + Guid.NewGuid().ToString() + "\", \"productName\":\"" + name + "\", \"descriptionUS\":\"" + imgAnalysis.Description.Captions.FirstOrDefault()?.Text + "\", \"descriptionFR\":\"Contiendra la traduction FR\", \"descriptionDE\":\"Spater das deutsche Version\" }";
        }

        static void PrintAnalysisResult(ImageAnalysis imgAn, ILogger log)
        {
            log.LogInformation("Vision Analysis result : ");
            log.LogInformation($"-- Description : ");
            log.LogInformation($"     |- Captions : {FormatListOf(imgAn.Description.Captions, (ImageCaption c) => c.Text)}");
            log.LogInformation($"     |- Tags : {FormatListOfString(imgAn.Description.Tags)}");
            log.LogInformation($"-- Categories : {FormatListOf(imgAn.Categories, (Category c) => $"{c.Name} [{c.Score}]")}");
            log.LogInformation($"-- Tags : {FormatListOf(imgAn.Tags, (ImageTag t) => $"{t.Name} [{t.Confidence}]")}");
            log.LogInformation($"-- Adult content : {imgAn.Adult.IsAdultContent}");
            log.LogInformation($"     |- Adult score : {imgAn.Adult.AdultScore}");
            log.LogInformation($"-- Gore content : {imgAn.Adult.IsGoryContent}");
            log.LogInformation($"     |- Gore score : {imgAn.Adult.GoreScore}");
            log.LogInformation($"-- Racist content : {imgAn.Adult.IsRacyContent}");
            log.LogInformation($"     |- Gore score : {imgAn.Adult.RacyScore}");
        }

        static string FormatListOf<T>(IEnumerable<T> listOf, Func<T, string> extractor)
        {
            bool notFirst = false;
            StringBuilder sb = new StringBuilder();
            foreach (var obj in listOf)
            {
                if (notFirst)
                    sb.Append(" , ");
                sb.Append(extractor(obj));
                notFirst = true;
            }
            return sb.ToString();
        }



        static string FormatListOfString(IEnumerable<string> listOfString)
        {
            bool notFirst = false;
            StringBuilder sb = new StringBuilder();
            foreach (var s in listOfString)
            {
                if (notFirst)
                    sb.Append(",");
                sb.Append(s);
                notFirst = true;
            }
            return sb.ToString();
        }
    }
}
