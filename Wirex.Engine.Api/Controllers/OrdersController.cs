using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Wirex.Engine.Api.Data;
using Wirex.Engine.Engine;
using Wirex.Engine.Services;

namespace Wirex.Engine.Api.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly IReliableStateManager _stateManager;

        public OrdersController(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        // PUT api/Orders
        [HttpPut]
        public async Task<IActionResult> Put(PlaceOrderData data)
        {
            ITradingEngine tradingEngine = new ReliableTradingEngine(
                _stateManager,
                new OrdersMatchingRule());

            await tradingEngine.PlaceAsync(new Order(
                new CurrencyPair(data.BaseCurrency, data.QuoteCurrency),
                data.Side,
                data.Price,
                data.Amount));

            return new OkResult();
        }
    }
}