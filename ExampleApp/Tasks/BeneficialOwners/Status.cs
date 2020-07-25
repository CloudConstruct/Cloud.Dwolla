using System;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.BeneficialOwners
{
    [Task("gbos", "Get Beneficial Ownership Status")]
    internal class Status : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to get the status: ");
            var input = ReadLineAsGuid();

            var statusRes = await Service.GetBeneficialOwnershipAsync(input);

            WriteLine($"Status={statusRes.Status}");
        }
    }
}
