using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalDecoder.Domain.Models;

namespace SignalDecoder.IntegrationTests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GenerateEndpoint_Returns200_WithValidDevices()
    {
        // Act
        var response = await _client.GetAsync("/api/devices/generate?count=5&signalLength=4&maxStrength=9");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await response.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        devices.Should().HaveCount(5);
        devices!.Values.Should().OnlyContain(pattern => pattern.Length == 4);
    }

    [Fact]
    public async Task GenerateEndpoint_Returns400_OnInvalidInput()
    {
        // Act
        var response = await _client.GetAsync("/api/devices/generate?count=150"); // > 100

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SimulateEndpoint_Returns200_WithCombinedSignal()
    {
        // Arrange
        var request = new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                { "D01", new[] { 1, 2, 3 } },
                { "D02", new[] { 4, 5, 6 } },
                { "D03", new[] { 7, 8, 9 } }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/signal/simulate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.ReceivedSignal.Should().HaveCount(3);
        result.ActiveDeviceCount.Should().BeInRange(1, 3);
        result.TotalDevices.Should().Be(3);
        result.SignalLength.Should().Be(3);
    }

    [Fact]
    public async Task? SimulateEndpoint_Returns400_OnEmptyDevices()
    {
        // Arrange
        var request = new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/signal/simulate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DecodeEndpoint_Returns200_WithCorrectSolution()
    {
        // Arrange
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                { "D01", new[] { 1, 2 } },
                { "D02", new[] { 3, 4 } },
                { "D03", new[] { 0, 1 } }
            },
            ReceivedSignal = new[] { 4, 6 }, // D01 + D02
            Tolerance = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolutionCount.Should().BeGreaterThan(0);
        result.Solutions.Should().Contain(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02"));
    }

    [Fact]
    public async Task DecodeEndpoint_Returns400_OnInvalidInput()
    {
        // Arrange
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                { "D01", new[] { 1, 2, 3 } },
                { "D02", new[] { 4, 5 } } // Mismatched length!
            },
            ReceivedSignal = new[] { 5, 7, 3 },
            Tolerance = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FullWorkflow_Generate_Simulate_Decode_RoundTrips()
    {
        // Step 1: Generate devices
        var generateResponse = await _client.GetAsync("/api/devices/generate?count=5&signalLength=4&maxStrength=5");
        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await generateResponse.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();

        // Step 2: Simulate transmission
        var simulateRequest = new SimulateRequest { Devices = devices! };
        var simulateResponse = await _client.PostAsJsonAsync("/api/signal/simulate", simulateRequest);
        simulateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var simulation = await simulateResponse.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        simulation.Should().NotBeNull();

        // Step 3: Decode the received signal
        var decodeRequest = new DecodeRequest
        {
            Devices = devices!,
            ReceivedSignal = simulation!.ReceivedSignal,
            Tolerance = 0
        };
        var decodeResponse = await _client.PostAsJsonAsync("/api/signal/decode", decodeRequest);
        decodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var decoded = await decodeResponse.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        decoded.Should().NotBeNull();

        // Step 4: Verify one of the solutions matches the received signal
        decoded!.SolutionCount.Should().BeGreaterThan(0);
        var firstSolution = decoded.Solutions[0];

        // Verify the computed sum matches the received signal
        firstSolution.ComputedSum.Should().Equal(simulation.ReceivedSignal);

        // Manually verify the solution by summing the decoded devices
        var manualSum = new int[simulation.ReceivedSignal.Length];
        foreach (var deviceId in firstSolution.TransmittingDevices)
        {
            var pattern = devices[deviceId];
            for (int i = 0; i < pattern.Length; i++)
            {
                manualSum[i] += pattern[i];
            }
        }
        manualSum.Should().Equal(simulation.ReceivedSignal,
            "The decoded devices should sum to the received signal");
    }

    [Fact]
    public async Task DecodeEndpoint_IncludesSolveTime()
    {
        // Arrange
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                { "D01", new[] { 1, 2 } },
                { "D02", new[] { 3, 4 } }
            },
            ReceivedSignal = new[] { 4, 6 },
            Tolerance = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolveTimeMs.Should().BeGreaterOrEqualTo(0);
    }
}
