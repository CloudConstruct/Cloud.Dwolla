using System;
using System.Threading.Tasks;
using Dwolla.Client.Models.Requests;

namespace ExampleApp.Tasks.Customers
{
    [Task("cu", "Update Customer")]
    internal class Update : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to update: ");
            var input = ReadLineAsGuid();

            var res = await Service.UpdateCustomerAsync(input,
                new UpdateCustomerRequest { Status = UpdateCustomerStatus.Deactivated });

            WriteLine($"Customer updated: Status={res.Status}");
        }
    }
}
