using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dwolla.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DwollaServiceCollectionExtensions
    {
        public static IServiceCollection AddDwollaService(
            this IServiceCollection services,
            DwollaCredentials dwollaCredentials,
            string dwollaApiUrl)
        {
            services
                .AddScoped<IDwollaService>(
                    (sp) => new DwollaService(
                        sp.GetRequiredService<DwollaClient>(),
                        dwollaCredentials))
                .AddHttpClient<DwollaClient>((_, client) =>
                {
                    client.BaseAddress = new Uri(dwollaApiUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentType));
                });

            return services;
        }

        public static IServiceCollection AddDwollaService(this IServiceCollection services, Uri dwollaApiUrl,
            Func<IServiceProvider, DwollaCredentials> fetchCredentials, Func<IServiceProvider, Task<DwollaToken>> fetchToken,
            Func<IServiceProvider, DwollaToken, Task> saveToken)
        {
            services
                .AddSingleton((sp) => fetchCredentials(sp))
                .AddScoped<IDwollaService>(
                    (sp) => new DwollaService(
                        sp.GetRequiredService<DwollaClient>(),
                        sp.GetRequiredService<DwollaCredentials>(),
                        sp,
                        fetchToken,
                        saveToken))
                .AddHttpClient<DwollaClient>((_, client) =>
                {
                    client.BaseAddress = dwollaApiUrl;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentType));
                });

            return services;
        }
    }
}
