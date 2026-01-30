using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    internal static class Factory
    {
        internal static BlobServiceClient CreateBlobClient()
        {
            var connectionString =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");
            }

            return new BlobServiceClient(connectionString);
        }
    }
}
