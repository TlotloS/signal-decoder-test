namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class DecoderCorrectnessTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DecoderCorrectnessTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Decode_FiveDevices_FindsCorrectThree()
    {
        // Pre-computed: D01 + D03 + D05 = received
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [2, 4, 1, 3],
                ["D02"] = [7, 1, 5, 2],
                ["D03"] = [3, 6, 2, 8],
                ["D04"] = [1, 0, 9, 4],
                ["D05"] = [5, 2, 3, 1],
            },
            ReceivedSignal = [10, 12, 6, 12],  // D01 + D03 + D05
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Count.Should().BeGreaterOrEqualTo(1);
        var solution = result.Solutions[0];
        solution.TransmittingDevices.Count.Should().Be(3);
        solution.TransmittingDevices.Should().Contain("D01");
        solution.TransmittingDevices.Should().Contain("D03");
        solution.TransmittingDevices.Should().Contain("D05");
    }

    [Fact]
    public async Task Decode_MultipleSolutions_ReturnsAll()
    {
        // Short signal length with values designed so multiple subsets match
        // D01 + D02 = [5, 5] and D03 = [5, 5]
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [2, 3],
                ["D02"] = [3, 2],
                ["D03"] = [5, 5],
            },
            ReceivedSignal = [5, 5],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Solutions.Count.Should().BeGreaterOrEqualTo(2,
            $"Expected at least 2 solutions but got {result.Solutions.Count}");
    }

    [Fact]
    public async Task Decode_EmptySubset_ZeroSignal()
    {
        // Received signal is all zeros — the "no devices active" case
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2],
                ["D02"] = [3, 4],
            },
            ReceivedSignal = [0, 0],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        // Should find exactly one solution: the empty set
        result!.Solutions.Should().ContainSingle();
        result.Solutions[0].TransmittingDevices.Should().BeEmpty();
    }

    [Fact]
    public async Task Decode_VerifiesComputedSum()
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
        result.Solutions[0].ComputedSum.Should().Equal([4, 6, 6]);
        result.Solutions[0].MatchesReceived.Should().BeTrue();
    }

    [Fact]
    public async Task Decode_IncludesSolveTime()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2],
                ["D02"] = [3, 4],
            },
            ReceivedSignal = [4, 6],
            Tolerance = 0
        };

        var response = await _client.PostAsJsonAsync("/api/signal/decode", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DecodeResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SolveTimeMs.Should().BeGreaterOrEqualTo(0, "SolveTimeMs should be populated");
    }

    [Fact]
    public async Task Decode_ReportsCorrectSolutionCount()
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
        result!.SolutionCount.Should().Be(result.Solutions.Count);
    }
}
