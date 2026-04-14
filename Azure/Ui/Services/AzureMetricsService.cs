using Azure.Identity;
using Azure.Monitor.Query;

namespace Ui.Services
{
    public class AzureMetricsService
    {
        private readonly MetricsQueryClient _client;
        private readonly string? _resourceId;

        public AzureMetricsService(IConfiguration config)
        {
            //var credential = new ClientSecretCredential(
            //    config["Azure:TenantId"],
            //    config["Azure:ClientId"],
            //    config["Azure:ClientSecret"]);

            //_client = new MetricsQueryClient(credential);

            _resourceId = config["Azure:ResourceId"];
        }

        public async Task<double?> GetCpuMetric()
        {
            var options = new MetricsQueryOptions
            {
                TimeRange = new QueryTimeRange(TimeSpan.FromMinutes(30))
            };

            var response = await _client.QueryResourceAsync(
                _resourceId,
                new[] { "CpuPercentage" },
                options);

            var metric = response.Value.Metrics.FirstOrDefault();

            return metric?.TimeSeries
                .FirstOrDefault()?
                .Values
                .LastOrDefault()?
                .Average;
        }
    }
}
