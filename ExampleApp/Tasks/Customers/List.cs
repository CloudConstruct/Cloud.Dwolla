using System.Threading.Tasks;

namespace ExampleApp.Tasks.Customers
{
    [Task("lc", "List Customers")]
    internal class List : BaseTask
    {
        public override async Task Run()
        {
            var res = await Service.GetCustomersAsync();
            res.Embedded.Customers
                .ForEach(c => WriteLine($" - ID:{c.Id}  {c.FirstName} {c.LastName}"));
        }
    }
}
