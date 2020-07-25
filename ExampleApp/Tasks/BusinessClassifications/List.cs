using System.Threading.Tasks;

namespace ExampleApp.Tasks.BusinessClassifications
{
    [Task("lbc", "List Business Classifications")]
    internal class List : BaseTask
    {
        public override async Task Run()
        {
            var res = await Service.GetBusinessClassificationsAsync();
            res.Embedded.BusinessClassifications
                .ForEach(bc => bc.Embedded.IndustryClassifications
                    .ForEach(ic => WriteLine($"{bc.Id}:{bc.Name} - {ic.Id}:{ic.Name}")));
        }
    }
}