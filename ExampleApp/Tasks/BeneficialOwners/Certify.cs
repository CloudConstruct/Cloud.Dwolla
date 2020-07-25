using System;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.BeneficialOwners
{
    [Task("crtbo", "Certify Beneficial Ownership")]
    internal class Certify : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to certify beneficial ownership: ");
            var input = ReadLineAsGuid();

            var uri = await Service.CertifyBeneficialOwnershipAsync(input);

            if (uri == null) return;

            WriteLine("Certified");
        }
    }
}
