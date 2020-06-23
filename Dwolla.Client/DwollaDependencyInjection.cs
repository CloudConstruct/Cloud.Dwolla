using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Dwolla.Client;
using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DwollaDependencyInjection
    {
        public static IServiceCollection AddDwollaService(
            this IServiceCollection services,
            Func<IServiceProvider, Task<DwollaCredentials>> fetchCredentials,
            Func<IServiceProvider, string> dwollaApiUrl,
            Func<IServiceProvider, Task<DwollaToken>> initializeToken,
            Func<IServiceProvider, DwollaToken, Task> saveToken)
        {
            services
                .AddSingleton((sp) => fetchCredentials(sp).Result)
                .AddScoped<IDwollaService>(
                    (sp) => new DwollaService(
                        sp,
                        sp.GetRequiredService<IDwollaClient>(),
                        sp.GetRequiredService<DwollaCredentials>(),
                        initializeToken,
                        saveToken))
                .AddHttpClient<IDwollaClient, DwollaClient>((sp, client) =>
                {
                    client.BaseAddress = new Uri(dwollaApiUrl(sp));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.dwolla.v1.hal+json"));
                });

            return services;
        }
    }
}
