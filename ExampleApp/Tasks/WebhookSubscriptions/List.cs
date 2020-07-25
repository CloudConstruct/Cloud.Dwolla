using System.Threading.Tasks;

namespace ExampleApp.Tasks.WebhookSubscriptions
{
    [Task("lws", "List Webhook Subscriptions")]
    internal class List : BaseTask
    {
        public override async Task Run()
        {
            var res = await Service.GetWebhookSubscriptionsAsync();
            res.Embedded.WebhookSubscriptions
                .ForEach(ws => WriteLine($" - {ws.Id}: {ws.Url}{(ws.Paused ? " PAUSED" : null)}"));
        }
    }
}
