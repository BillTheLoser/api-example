using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using Pnnl.Api.Approvals.Data.Interfaces;
//using System.Text.Json;

namespace Pnnl.Api.Approvals.Data.Http
{
    public class HttpPersonStore : IPersonStore
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The <see cref="ILogger"/> used to write diagnostic information.</value>
        protected ILogger<HttpPersonStore> _logger { get; }

        /// <summary>
        /// Gets the options used to configure this service.
        /// </summary>
        /// <value>The <see cref="IOptions{TOptions}"/> used to configure this service.</value>
        protected IOptions<HttpPersonStoreOptions> _options { get; }

        /// <summary>
        /// Gets the http client used to make service requests.
        /// </summary>
        private HttpClient _client { get; set; }

        public HttpPersonStore(ILogger<HttpPersonStore> logger, IHttpClientFactory clients, IOptions<HttpPersonStoreOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _client = clients.CreateClient(_options.Value.Client);
        }

        public Task<IPagedResult<Person>> GetAsync(int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        public async Task<Person> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null)
        {
            var uri = new Uri(_client.BaseAddress, $"people/{id}");

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await _client.SendAsync(request, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();

                    //return JsonSerializer.Deserialize<Person>(json);
                    return JsonConvert.DeserializeObject<Person>(json);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to retrieve person {@id}. Reason: {@Exception}", id, ex);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="domain">The network domain of the user to retrieve.</param>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        public async Task<Person> GetByNetworkIdAsync(string domain, string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null)
        {
            //TODO: convert v2 opertations call

            //https://apifoundation.pnl.gov/operations/jsonapi/people?filter[query]=person-by-network&filter[domain]=PNL&filter[username]=GLOR911
            //GET /operations/jsonapi/people?filter[query]=person-by-network&amp; filter[domain]=PNL&amp; filter[username]=GLOR911 HTTP/1.1
            //Host: apifoundation.pnl.gov
            //Accept: application/vnd.api+json
            //User-Agent: PostmanRuntime/7.17.1
            //Cache-Control: no-cache
            //Postman-Token: bdad11f7-4c1a-45d8-8081-3f476b336889,9ee1008a-9e1d-4214-b5fa-225416604c93
            //Host: apifoundation.pnl.gov
            //Accept-Encoding: gzip, deflate
            //Connection: keep-alive
            //cache-control: no-cache
            // 
            //That's the fast route for lookup by network creds
            //Just have to be sure to set Accept header

            // Old https://api.pnl.gov/operations/v2/


            var uri = new Uri(_client.BaseAddress, $"people?domain={domain}&username={id}");
            //var uri = new Uri(_client.BaseAddress, $"people?filter[query]=person-by-network&filter[domain]={domain}&filter[username]={id}");

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (var response = await _client.SendAsync(request, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();

                    var matches = JsonConvert.DeserializeObject<List<Person>>(json);

                    return matches.Any() ? matches[0] : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to retrieve person {@id}. Reason: {@Exception}", id, ex);
                return null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// If <see langword="true"/>, the method has been directly or indirectly by a user's code; 
        /// if <see langword="false"/> the method has been called by the runtime from inside a finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }
    }
}
