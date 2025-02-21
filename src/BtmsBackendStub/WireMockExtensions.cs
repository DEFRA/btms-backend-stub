using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Defra.BtmsBackendStub;
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
    private const string WireMockWildcard = "*";
    
    public static void StubSingleImportNotifications(
        this WireMockServer wireMock
    )
    {
        var request = Request.Create()
            .WithPath(Endpoints.ImportNotifications.Get(WireMockWildcard))
            .UsingGet();

        wireMock.Given(request)
            .RespondWith(Response.Create()
            .WithCallback(req =>
            {
                var chedReferenceNumber = req.PathSegments.Last();
                try
                {
                    var body = GetBody($"btms-import-notification-single-{chedReferenceNumber}.json");
                    return CreateJson200Ok(body);
                }
                catch (InvalidOperationException _)
                {
                    return CreateJson404NotFound();
                }
            }));
    }
    
    public static void StubSingleMovements(
        this WireMockServer wireMock
    )
    {
        var request = Request.Create()
            .WithPath(Endpoints.Movements.Get(WireMockWildcard))
            .UsingGet();

        wireMock.Given(request)
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var mrn = req.PathSegments.Last();
                    try
                    {
                        var body = GetBody($"btms-movement-single-{mrn}.json");
                        return CreateJson200Ok(body);
                    }
                    catch (InvalidOperationException _)
                    {
                        return CreateJson404NotFound();
                    }
                }));
    }

    private static ResponseMessage CreateJson200Ok(string body) => new()
    {
        Headers = new Dictionary<string, WireMockList<string>>
        {
            { "Content-Type", "application/json" },
        },
        BodyData = new BodyData { DetectedBodyType = BodyType.String, BodyAsString = body },
        StatusCode = StatusCodes.Status200OK
    };
    
    private static ResponseMessage CreateJson404NotFound() => new()
    {
        StatusCode = StatusCodes.Status404NotFound
    };
    
    private static ResponseMessage CreateJson500ServerError() => new()
    {
        StatusCode = StatusCodes.Status404NotFound
    };

    public static void StubImportNotificationUpdates( this WireMockServer wireMock,
        bool shouldFail = false,
        Func<JsonNode, JsonNode>? transformBody = null,
        string? path = null,
        Func<IRequestBuilder, IRequestBuilder>? transformRequest = null,
        int? statusCode = null)
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

    private static string GetBody(string fileName)
    {
        var type = typeof(WireMockExtensions);
        var assembly = type.Assembly;

        using var stream = assembly.GetManifestResourceStream($"{type.Namespace}.Scenarios.{fileName}");

        if (stream is null)
            throw new InvalidOperationException($"Unable to find embedded resource {fileName}");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
