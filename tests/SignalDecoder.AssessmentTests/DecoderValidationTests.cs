namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class DecoderValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DecoderValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Decode_NoSolution_ReturnsEmptySolutions()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 1],
                ["D02"] = [2, 2],
            },
            ReceivedSignal = [9, 9],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Should().BeEmpty();
        result.SolutionCount.Should().Be(0);
    }

    [Fact]
    public async Task Decode_LargerSignalLength_StillWorks()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2, 3, 4, 5, 6, 7, 8],
                ["D02"] = [8, 7, 6, 5, 4, 3, 2, 1],
                ["D03"] = [1, 1, 1, 1, 1, 1, 1, 1],
            },
            ReceivedSignal = [9, 9, 9, 9, 9, 9, 9, 9],  // D01 + D02
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Count.Should().BeGreaterOrEqualTo(1);
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        hasD01D02.Should().BeTrue();
    }

    [Fact]
    public async Task Decode_SingleDeviceInList_CanMatch()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [5, 5, 5],
            },
            ReceivedSignal = [5, 5, 5],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Should().ContainSingle();
        result.Solutions[0].TransmittingDevices.Should().ContainSingle();
        result.Solutions[0].TransmittingDevices.Should().Contain("D01");
    }

    [Fact]
    public async Task Decode_DecodedSignals_ContainsCorrectPatterns()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 6, 6],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Should().ContainSingle();
        var decoded = result.Solutions[0].DecodedSignals;
        decoded.Should().HaveCount(2);
        decoded["D01"].Should().Equal([3, 1, 4]);
        decoded["D02"].Should().Equal([1, 5, 2]);
    }
}
