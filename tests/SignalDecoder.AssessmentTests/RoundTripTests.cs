namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class RoundTripTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public RoundTripTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task FullWorkflow_Generate_Simulate_Decode_RoundTrips()
    {
        // Generate devices
        var generateResponse = await _client.GetAsync("/api/devices/generate?count=5&signalLength=4&maxStrength=9");
        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await generateResponse.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        devices!.Count.Should().Be(5);

        // Simulate transmission
        var simulateRequest = new SimulateRequest { Devices = devices };
        var simulateResponse = await _client.PostAsJsonAsync("/api/signal/simulate", simulateRequest);
        simulateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var simResult = await simulateResponse.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        simResult.Should().NotBeNull();
        simResult!.ActiveDeviceCount.Should().BeGreaterOrEqualTo(1);

        // Decode the signal
        var decodeRequest = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = simResult.ReceivedSignal,
            Tolerance = 0
        };

        var decodeResponse = await _client.PostAsJsonAsync("/api/signal/decode", decodeRequest);
        decodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var decodeResult = await decodeResponse.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        decodeResult.Should().NotBeNull();

        // Verify at least one solution exists
        decodeResult!.Solutions.Count.Should().BeGreaterOrEqualTo(1,
            "Decoder should find the solution for a simulated signal");

        // Note: there may be multiple subsets that produce the same sum,
        // so we verify any solution actually sums correctly
        foreach (var solution in decodeResult.Solutions)
        {
            var length = simResult.ReceivedSignal.Length;
            var computed = new int[length];
            foreach (var deviceId in solution.TransmittingDevices)
                for (int i = 0; i < length; i++)
                    computed[i] += devices[deviceId][i];

            computed.Should().Equal(simResult.ReceivedSignal);
        }
    }
}
