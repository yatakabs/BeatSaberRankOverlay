using Microsoft.AspNetCore.Mvc;

namespace RankOverlay.Web.BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private const string DefaultImageMediaType = "image/jpeg";

    private IHttpClientFactory HttpClientFactory { get; }

    public ImagesController(IHttpClientFactory httpClientFactory)
    {
        this.HttpClientFactory = httpClientFactory;
    }

    [HttpGet("{url}")]
    public async Task<IActionResult> Get(string url)
    {
        using var httpClient = this.HttpClientFactory.CreateClient();

        using var res = await httpClient.GetAsync(Uri.UnescapeDataString(url));
        _ = res.EnsureSuccessStatusCode();

        if (!res.IsSuccessStatusCode)
        {
            return this.StatusCode((int)res.StatusCode, await res.Content.ReadAsStringAsync());
        }

        using var ms = new MemoryStream();
        await res.Content.CopyToAsync(ms);

        return this.File(
            ms.ToArray(),
            res.Content.Headers.ContentType?.MediaType ?? DefaultImageMediaType);
    }
}
