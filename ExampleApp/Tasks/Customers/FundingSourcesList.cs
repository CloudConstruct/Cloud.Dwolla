using System;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.Customers
{
    [Task("lcfs", "List a Customer's Funding Sources")]
    internal class FundingSourcesList : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to list the funding sources: ");
            var input = ReadLineAsGuid();

            var res = await Service.GetCustomerFundingSourcesAsync(input);

            res.Embedded.FundingSources
                .ForEach(fs => WriteLine($" - ID:{fs.Id}  Name:{fs.Name} Type:{fs.Type}"));
        }
    }
}
