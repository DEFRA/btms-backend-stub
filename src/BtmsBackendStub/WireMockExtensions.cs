using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace Defra.BtmsBackendStub;

[ExcludeFromCodeCoverage]
public static class WireMockExtensions
{
    private static Type Anchor => typeof(WireMockExtensions);

    public static void StubSingleImportNotification(
        this WireMockServer wireMock,
        bool shouldFail = false,
        string chedReferenceNumber = "",
        Func<IRequestBuilder, IRequestBuilder>? transformRequest = null,
        Func<JsonNode, JsonNode>? transformResponse = null
    )
    {
        var code = shouldFail ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;
        var response = Response.Create().WithStatusCode(code);

        if (!shouldFail)
        {
            var responseBody = GetBody($"btms-import-notification-single-{chedReferenceNumber}.json");
            if (transformResponse is not null)
                responseBody = transformResponse(JsonNode.Parse(responseBody)!).ToJsonString();

            response = response.WithBody(responseBody);
        }

        var request = Request.Create().WithPath(Endpoints.ImportNotifications.Get(chedReferenceNumber)).UsingGet();

        if (transformRequest is not null)
            request = transformRequest(request);

        wireMock.Given(request).RespondWith(response);
    }

    public static void StubImportNotificationUpdates(
        this WireMockServer wireMock,
        bool shouldFail = false,
        Func<JsonNode, JsonNode>? transformBody = null,
        string? path = null,
        Func<IRequestBuilder, IRequestBuilder>? transformRequest = null,
        int? statusCode = null
    )
    {
        var code = statusCode ?? (shouldFail ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK);
        var response = Response.Create().WithStatusCode(code);

        if (!shouldFail)
        {
            var body = GetBody("btms-import-notification-updates.json");

            if (transformBody != null)
            {
                var jsonNode = JsonNode.Parse(body);
                if (jsonNode is null)
                    throw new InvalidOperationException("JSON node was null");

                body = transformBody(jsonNode).ToString();
            }

            response = response.WithBody(body);
        }

        var request = Request.Create().WithPath(path ?? Endpoints.ImportNotifications.Get()).UsingGet();

        if (transformRequest is not null)
            request = transformRequest(request);

        wireMock.Given(request).RespondWith(response);
    }

    public static void StubSingleMovement(
        this WireMockServer wireMock,
        bool shouldFail = false,
        string mrn = "",
        Func<IRequestBuilder, IRequestBuilder>? transformRequest = null
    )
    {
        var code = shouldFail ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;
        var response = Response.Create().WithStatusCode(code);

        if (!shouldFail)
            response = response.WithBody(GetBody($"btms-movement-single-{mrn}.json"));

        var request = Request.Create().WithPath(Endpoints.Movements.Get(mrn)).UsingGet();

        if (transformRequest is not null)
            request = transformRequest(request);

        wireMock.Given(request).RespondWith(response);
    }

    public static void StubSingleGmr(
        this WireMockServer wireMock,
        bool shouldFail = false,
        string gmrId = "",
        Func<IRequestBuilder, IRequestBuilder>? transformRequest = null
    )
    {
        var code = shouldFail ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;
        var response = Response.Create().WithStatusCode(code);

        if (!shouldFail)
            response = response.WithBody(GetBody($"btms-goods-movement-single-{gmrId}.json"));

        var request = Request.Create().WithPath(Endpoints.Gmrs.Get(gmrId)).UsingGet();

        if (transformRequest is not null)
            request = transformRequest(request);

        wireMock.Given(request).RespondWith(response);
    }

    public static void StubAllGmrs(this WireMockServer wireMock)
    {
        foreach (var gmrId in GetAllStubGmrs())
            wireMock.StubSingleGmr(gmrId: gmrId);
    }

    public static void StubAllMovements(this WireMockServer wireMock)
    {
        foreach (var movementId in GetAllStubMovementIds())
            wireMock.StubSingleMovement(mrn: movementId);
    }

    public static void StubAllImportNotifications(this WireMockServer wireMock)
    {
        foreach (var chedReferenceNumber in GetAllStubChedReferenceNumbers())
            wireMock.StubSingleImportNotification(chedReferenceNumber: chedReferenceNumber);
    }

    public static void StubUtilityEndpoints(this WireMockServer wireMock)
    {
        var data = GetAllStubChedReferenceNumbers().Select(x => new
        {
            type = "import-notifications",
            id = x,
            attributes = new { updatedEntity = DateTime.UtcNow.ToString("O"), referenceNumber = x },
            links = new { self = $"/api/import-notifications/{x}" }
        }).ToList();

        wireMock.Given(Request.Create()
                .WithPath("/utility/import-notification-updates")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(StatusCodes.Status200OK)
                .WithBodyAsJson(
                    new
                    {
                        links = new { self = "/api/import-notifications", first = "/api/import-notifications" },
                        data,
                        meta = new { total = data.Count }
                    }, indented: true));
    }

    private static IEnumerable<string> GetAllStubChedReferenceNumbers() =>
        Anchor
            .Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith($"{GetScenarioPrefix()}btms-import-notification-single-"))
            .Select(x => x.Replace($"{GetScenarioPrefix()}btms-import-notification-single-", "").Replace(".json", ""));

    private static IEnumerable<string> GetAllStubMovementIds() =>
        Anchor
            .Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith($"{GetScenarioPrefix()}btms-movement-single-"))
            .Select(x => x.Replace($"{GetScenarioPrefix()}btms-movement-single-", "").Replace(".json", ""));

    private static IEnumerable<string> GetAllStubGmrs() =>
        Anchor
            .Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith($"{GetScenarioPrefix()}btms-goods-movement-single-"))
            .Select(x => x.Replace($"{GetScenarioPrefix()}btms-goods-movement-single-", "").Replace(".json", ""));

    private static string GetBody(string fileName)
    {
        using var stream = Anchor.Assembly.GetManifestResourceStream($"{GetScenarioPrefix()}{fileName}");

        if (stream is null)
            throw new InvalidOperationException($"Unable to find embedded resource {fileName}");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static string GetScenarioPrefix() => $"{Anchor.Namespace}.Scenarios.";
}
