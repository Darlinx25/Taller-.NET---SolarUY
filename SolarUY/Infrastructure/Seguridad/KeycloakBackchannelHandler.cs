using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SolarUY.Infrastructure.Seguridad;

/// <summary>
/// Reescribe las llamadas servidor-a-servidor del IdP local para que usen la URL interna de Docker
/// mientras el navegador sigue usando la URL pública configurada en <c>Authority</c>.
/// </summary>
public sealed class KeycloakBackchannelHandler : DelegatingHandler
{
    private readonly Uri _urlPublica;
    private readonly Uri _urlInterna;

    public KeycloakBackchannelHandler(Uri urlPublica, Uri urlInterna)
    {
        _urlPublica = urlPublica ?? throw new ArgumentNullException(nameof(urlPublica));
        _urlInterna = urlInterna ?? throw new ArgumentNullException(nameof(urlInterna));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is { } uri &&
            string.Equals(uri.Authority, _urlPublica.Authority, StringComparison.OrdinalIgnoreCase))
        {
            var constructor = new UriBuilder(uri)
            {
                Scheme = _urlInterna.Scheme,
                Host = _urlInterna.Host,
                Port = _urlInterna.IsDefaultPort ? -1 : _urlInterna.Port
            };
            request.RequestUri = constructor.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
