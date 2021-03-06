﻿using System;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.Customers
{
    [Task("gcit", "Get a Customer IAV Token")]
    internal class IavToken : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to get an IAV token: ");
            var input = ReadLineAsGuid();

            var res = await Service.GetCustomerIavTokenAsync(input);
            WriteLine($"Token created: {res.Token}");
        }
    }
}
