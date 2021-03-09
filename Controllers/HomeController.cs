using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json.Linq;
using Sentry;
using Microsoft.AspNetCore.Http;
using AspNetCoreMVC.Controllers;
using Sentry.AspNetCore;

namespace AspNetCoreMVC.Controllers
{

    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        private void Checkout(List<Item> cart)
        {
            _logger.LogInformation("*********** Inventory before: {inventory}", Store.inventory);
            var tempInventory = Store.inventory;
            foreach (var item in cart)
            {
                if (Store.inventory[item.id.ToString()] <= 0)
                {
                    throw new Exception("Not enough inventory for " + item.id);
                }
                else
                {
                    tempInventory[item.id.ToString()] = tempInventory[item.id.ToString()] - 1;
                }
            }
            Store.inventory = tempInventory;
            _logger.LogInformation("*********** Inventory after: {inventory}", Store.inventory);
        }

        [HttpPost("checkout")]
        public string Checkout([FromBody] Order order)
        {
            var email = order.email.ToString();
            var transactionId = Request.Headers["X-transaction-ID"];
            var sessionId = Request.Headers["X-session-ID"];
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new Sentry.Protocol.User
                {
                    Email = email
                };
                scope.SetTag("transaction_id", transactionId);
                scope.SetTag("session_id", sessionId);
                scope.SetExtra("inventory", Store.inventory);
            });

            Checkout(order.cart);
            return "SUCCESS: order has been placed";
        }

        [HttpGet("handled")]
        public string Handled()
        {

            SentrySdk.ConfigureScope(scope => {
                scope.SetTag("CustomerType","Enterprise");
                scope.User = new User
                {
                    Email = "john.doe@example.com"
                };
            });
            _logger.LogInformation("This log entry is added as a breadcrumb");
            
            try
            {
                string[] weekDays = new string[7] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                Console.WriteLine( weekDays[7]); 
            }
            catch (Exception exception){
                //System.IndexOutOfRangeException
                _logger.LogError(exception, "This log entry is added as error.");
            }
            return "SUCCESS: back-end error handled gracefully";
        }


        [HttpGet("unhandled")]
        public string Unhandled()
        {
            SentrySdk.ConfigureScope(scope => {
                scope.SetTag("CustomerType","Enterprise");
                scope.User = new User
                {
                    Email = "john.doe@example.com"
                };
            });
            _logger.LogInformation("This log entry is added as a breadcrumb");

            int n1 = 1;
            int n2 = 0;

            //System.DivideByZeroException
            int ans = n1 / n2;

            return "FAILURE: Server-side Error";
        }
    }

    public class Order
    {
        public string email { get; set; }
        public List<Item> cart { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string name { get; set; }
        public int price { get; set; }
        public string image { get; set; }
    }

    public static class Store
    {
        public static Dictionary<string, int> inventory
            = new Dictionary<string, int>
        {
        { "wrench", 1 },
        { "nails", 1 },
        { "hammer", 1 }
        };
    }

}
