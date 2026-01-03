using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PetCarePlatform.Infrastructure.Location
{
    /// <summary>
    /// DelegatingHandler to add User-Agent header for Nominatim API requests.
    /// User-Agent is a restricted header in HttpClient, so we need to use a handler to set it.
    /// </summary>
    public class NominatimUserAgentHandler : DelegatingHandler
    {
        private readonly string _userAgent;

        public NominatimUserAgentHandler(string userAgent)
        {
            _userAgent = userAgent ?? throw new System.ArgumentNullException(nameof(userAgent));
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Set User-Agent header on each request
            // This works because we're modifying the request before it's sent
            request.Headers.Remove("User-Agent");
            request.Headers.Add("User-Agent", _userAgent);
            
            return base.SendAsync(request, cancellationToken);
        }
    }
}

