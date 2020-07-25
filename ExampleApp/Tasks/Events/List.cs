using System.Threading.Tasks;

namespace ExampleApp.Tasks.Events
{
    [Task("le", "List Events")]
    internal class List : BaseTask
    {
        public override async Task Run()
        {
            var res = await Service.GetEventsAsync();
            res.Embedded.Events
                .ForEach(ev => WriteLine($" - {ev.Id}: {ev.Topic} {ev.ResourceId}"));
        }
    }
}
