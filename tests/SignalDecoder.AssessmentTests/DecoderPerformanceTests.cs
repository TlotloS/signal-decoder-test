namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class DecoderPerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DecoderPerformanceTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Decode_10Devices_CompletesUnder1Second()
    {
        var devices = GenerateTestDevices(10, 5, seed: 42);
        // Pick 4 devices to be active
        var activeKeys = devices.Keys.Take(4).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolveTimeMs.Should().BeLessThan(1000,
            $"Took {result.SolveTimeMs}ms — should be under 1000ms for 10 devices");
        result.Solutions.Count.Should().BeGreaterOrEqualTo(1, "Should find at least one solution");
    }

    [Fact]
    public async Task Decode_15Devices_CompletesUnder3Seconds()
    {
        var devices = GenerateTestDevices(15, 5, seed: 123);
        var activeKeys = devices.Keys.Take(5).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolveTimeMs.Should().BeLessThan(3000,
            $"Took {result.SolveTimeMs}ms — should be under 3000ms for 15 devices");
        result.Solutions.Count.Should().BeGreaterOrEqualTo(1, "Should find at least one solution");
    }

    [Fact]
    public async Task Decode_20Devices_CompletesUnder5Seconds()
    {
        var devices = GenerateTestDevices(20, 6, seed: 456);
        var activeKeys = devices.Keys.Take(7).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolveTimeMs.Should().BeLessThan(5000,
            $"Took {result.SolveTimeMs}ms — should be under 5000ms for 20 devices");
        result.Solutions.Count.Should().BeGreaterOrEqualTo(1, "Should find at least one solution");
    }

    // Helper methods
    private static Dictionary<string, int[]> GenerateTestDevices(int count, int length, int seed)
    {
        var rng = new Random(seed);
        var devices = new Dictionary<string, int[]>();
        for (int i = 0; i < count; i++)
        {
            var pattern = new int[length];
            for (int j = 0; j < length; j++)
                pattern[j] = rng.Next(0, 10);
            devices[$"D{(i + 1):D2}"] = pattern;
        }
        return devices;
    }

    private static int[] ComputeSum(Dictionary<string, int[]> devices, List<string> activeKeys)
    {
        var length = devices.Values.First().Length;
        var sum = new int[length];
        foreach (var key in activeKeys)
            for (int i = 0; i < length; i++)
                sum[i] += devices[key][i];
        return sum;
    }
}
