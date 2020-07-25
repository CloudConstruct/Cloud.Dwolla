using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.Customers
{
    [Task("gcb", "Get Customer Balance")]
    internal class Balance : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to get the balance: ");
            var input = ReadLineAsGuid();

            var sourcesRes = await Service.GetCustomerFundingSourcesAsync(input);
            var balanceRes = await Service.GetFundingSourceBalanceAsync(sourcesRes.Embedded.FundingSources
                .First(x => x.Type == "balance").Links["balance"].Id.Value);

            var balance = balanceRes.Balance;
            WriteLine(balance == null ? $"Status={balanceRes.Status}" : $"Balance={balance.Value} {balance.Currency}");
        }
    }
}
