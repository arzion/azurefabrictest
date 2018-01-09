using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Wirex.Engine.Api;
using Wirex.Engine.Api.Data;
using Xunit;

namespace Wirex.Fabric.IntegrationTests.given_WirexFabricStatefulService
{
    // ReSharper disable once InconsistentNaming
    [Category("Integration")]
    public class When_CallingServiceApi
    {
        [Fact]
        public async Task Then_AllPartitionsReturn()
        {
            // Arrange
            var orderData = new Fixture().Create<PlaceOrderData>();
            var serviceName = new Uri("fabric:/Wirex/Wirex.Engine.Api");
            var proxyAddress = new Uri($"http://localhost:19081{serviceName.AbsolutePath}");
            long partitionKey = PartitionProvider.GetPartition(orderData.BaseCurrency, orderData.QuoteCurrency);

            var proxyUrl =
                $"{proxyAddress}/api/Orders?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            StringContent putContent = new StringContent(
                JsonConvert.ToString(orderData),
                Encoding.UTF8,
                "application/json");
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            var response = await new HttpClient().PutAsync(proxyUrl, putContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
