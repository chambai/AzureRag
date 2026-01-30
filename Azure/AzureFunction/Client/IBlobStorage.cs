using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public interface IBlobStorage
    {
        Task UploadAsync(string containerName, string blobName, Stream content);
    }
}
