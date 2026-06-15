using System.Net;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("TeamLinksApi", (sp, client) =>
{
    var baseUrl = builder.Configuration["TeamLinks:BaseUrl"] ?? "http://localhost:8080";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.MapGet("/", () => "POST /api/shorten com {\"url\":\"...\"} — depois abra GET /r/{codigoBase62}");

app.MapPost("/api/shorten", async (HttpRequest req, IHttpClientFactory httpFactory, IConfiguration cfg) =>
{
    var body = await req.ReadFromJsonAsync<Encurtar>();
    if (body is null || string.IsNullOrWhiteSpace(body.Url))
        return Results.BadRequest("Envie um JSON com \"url\".");

    var projetoId = cfg.GetValue<long>("TeamLinks:ProjectId", 1);
    var publicBase = (cfg["TeamLinks:PublicBaseUrl"] ?? "http://localhost:5006").TrimEnd('/');

    var nome = string.IsNullOrWhiteSpace(body.Name)
        ? "Link"
        : body.Name.Trim();

    var http = httpFactory.CreateClient("TeamLinksApi");
    var resp = await http.PostAsJsonAsync(
        $"api/links/project/{projetoId}",
        new { url = body.Url.Trim(), name = nome, description = body.Description, tagNames = body.TagNames });

    if (!resp.IsSuccessStatusCode)
        return Results.Problem($"API Java respondeu {(int)resp.StatusCode}. Confira se ela está no ar.");

    var criado = await resp.Content.ReadFromJsonAsync<LinkDaApi>();
    if (criado is null || string.IsNullOrWhiteSpace(criado.ShortCode))
        return Results.Problem("Resposta vazia ou sem shortCode da API.");

    var curto = $"{publicBase}/r/{criado.ShortCode}";
    return Results.Json(new { shortUrl = curto, shortCode = criado.ShortCode, linkId = criado.Id });
});

app.MapGet("/r/{code}", async (string code, IHttpClientFactory httpFactory) =>
{
    if (string.IsNullOrWhiteSpace(code))
        return Results.BadRequest();

    var http = httpFactory.CreateClient("TeamLinksApi");
    var resp = await http.GetAsync($"api/links/ref/{Uri.EscapeDataString(code.Trim())}/redirect");

    if (resp.StatusCode == HttpStatusCode.NotFound)
        return Results.NotFound();

    resp.EnsureSuccessStatusCode();
    var link = await resp.Content.ReadFromJsonAsync<LinkDaApi>();
    return link is null ? Results.NotFound() : Results.Redirect(link.Url);
});

app.Run();

record Encurtar(string Url, string? Name = null, string? Description = null, string[]? TagNames = null);
record LinkDaApi(long Id, string ShortCode, string Url);
