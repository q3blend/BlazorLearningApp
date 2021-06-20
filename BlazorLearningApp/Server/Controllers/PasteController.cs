using Azure.Storage.Blobs;
using BlazorLearningApp.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorLearningApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PasteController : ControllerBase
    {
        private readonly ILogger<PasteController> logger;
        private readonly CloudBlobContainer container;

        public PasteController(ILogger<PasteController> logger, IConfiguration config, CloudBlobContainer container)
        {
            this.logger = logger;
            this.container = container;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<Paste> Get(string id)
        {
            var paste = new Paste();
            var blockBlob = this.container.GetBlobReference(id);
            
            // fetch metadata
            await blockBlob.FetchAttributesAsync();

            if((DateTime.Parse(blockBlob.Metadata["Expire"]).Subtract(DateTime.UtcNow).TotalSeconds < 0))
            {
                throw new Exception("File already expired");
            }

            paste.Name = blockBlob.Metadata["Name"];

            using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream);
                paste.Value = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return paste;
        }

            [HttpPost]
        public async Task<string> Post([FromBody] Paste paste)
        {
            // This is a free service, size has to be limited :D
            if(paste.Value.Length > 1048576)
            {
                throw new Exception($"File size too big: {paste.Value.Length}");
            }
            var id = Guid.NewGuid();
            var blockBlob = container.GetBlockBlobReference(id.ToString());
            
            if (paste.Expiration > 0)
            {
                blockBlob.Metadata.Add("Expire", DateTime.UtcNow.AddDays(paste.Expiration).ToString());
            }

            blockBlob.Metadata.Add("Name", paste.Name);
            try
            {
                await blockBlob.UploadTextAsync(paste.Value);
                await blockBlob.SetMetadataAsync();
            } catch (Exception e)
            {
                throw e;
            }

            return id.ToString();
        }
    }
}
