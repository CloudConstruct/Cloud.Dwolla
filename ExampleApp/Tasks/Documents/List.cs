using System;
using System.Threading.Tasks;

namespace ExampleApp.Tasks.Documents
{
    [Task("ld", "List Documents")]
    internal class List : BaseTask
    {
        public override async Task Run()
        {
            Write("Customer ID for whom to upload a document: ");
            var input = ReadLineAsGuid();

            var res = await Service.GetCustomerDocumentsAsync(input);
            res.Embedded.Documents
                .ForEach(d => WriteLine($" - ID:{d.Id}  {d.Type} {d.Status}"));
        }
    }
}
