using System.Threading.Tasks;
using Dwolla.Client.Models;

namespace ExampleApp.Tasks.WebhookSubscriptions
{
    [Task("cws", "Create a Webhook Subscription")]
    internal class Create : BaseTask
    {
        public override async Task Run()
        {
            Write("Enter the url the webhook should output to: ");
            var url = ReadLine();
            Write("Enter a secret for the webhook: ");
            var secret = ReadLine();

            var createdSubscriptionUri = await Service.CreateWebhookSubscriptionAsync(url, secret);

            var subscription = await Service.GetWebhookSubscriptionAsync(Link.ParseId(createdSubscriptionUri).Value);
            WriteLine($"Created Subscription {subscription.Id} with url={subscription.Url}");
        }
    }
}
