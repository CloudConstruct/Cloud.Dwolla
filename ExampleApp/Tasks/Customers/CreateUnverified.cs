using System.Threading.Tasks;
using Dwolla.Client.Models;

namespace ExampleApp.Tasks.Customers
{
    [Task("cuc", "Create an Unverified Customer")]
    internal class CreateUnverified : BaseTask
    {
        public override async Task Run()
        {
            var uri = await Service.CreateCustomerAsync(
                new Dwolla.Client.Models.Requests.CreateCustomerRequest
                {
                    FirstName = "night",
                    LastName = $"man-{RandomAlphaNumericString(5)}",
                    Email = $"{RandomAlphaNumericString(20)}@example.com"
                });

            if (uri == null) return;

            var customer = await Service.GetCustomerAsync(Link.ParseId(uri).Value);
            WriteLine($"Created {customer.FirstName} {customer.LastName} with email={customer.Email}");
        }
    }
}
