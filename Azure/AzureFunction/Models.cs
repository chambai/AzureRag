using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction
{
    public class VectorResponse
    {
        // response data from the GPU Embedding service
        public List<List<double>> Vectors { get; set; } = new();
    }

}
