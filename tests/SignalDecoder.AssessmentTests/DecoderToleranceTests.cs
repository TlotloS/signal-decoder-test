namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class DecoderToleranceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DecoderToleranceTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Decode_WithTolerance_FindsFuzzyMatch()
    {
        // Exact sum of D01 + D02 = [4, 6, 6]
        // Received is [4, 7, 6] — off by 1 at position 1
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 7, 6],
            Tolerance = 1
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Count.Should().BeGreaterOrEqualTo(1, "Should find at least one fuzzy match");
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02"));
        hasD01D02.Should().BeTrue("D01 + D02 should be a valid fuzzy match");
    }

    [Fact]
    public async Task Decode_WithTolerance_RejectsOutOfRange()
    {
        // D01 + D02 = [4, 6, 6], received = [4, 9, 6] — off by 3 at position 1
        // Tolerance of 1 should NOT match
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
            },
            ReceivedSignal = [4, 9, 6],
            Tolerance = 1
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        var hasD01D02 = result!.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        hasD01D02.Should().BeFalse("D01 + D02 should NOT match — difference exceeds tolerance");
    }

    [Fact]
    public async Task Decode_ToleranceZero_ExactMatchOnly()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
            },
            ReceivedSignal = [4, 7, 6],  // off by 1
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        var hasD01D02 = result!.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        hasD01D02.Should().BeFalse("With tolerance 0, off-by-one should not match");
    }
}
