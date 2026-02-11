namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Domain.Models;

public class GeneratorAndSimulatorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public GeneratorAndSimulatorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Generator_ReturnsRequestedCount()
    {
        var response = await _client.GetAsync("/api/devices/generate?count=10&signalLength=4&maxStrength=9");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        devices!.Count.Should().Be(10);
    }

    [Fact]
    public async Task Generator_AllPatternsCorrectLength()
    {
        var response = await _client.GetAsync("/api/devices/generate?count=5&signalLength=7&maxStrength=9");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        devices!.Values.Should().OnlyContain(p => p.Length == 7);
    }

    [Fact]
    public async Task Generator_ValuesWithinRange()
    {
        var response = await _client.GetAsync("/api/devices/generate?count=5&signalLength=4&maxStrength=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        foreach (var pattern in devices!.Values)
        {
            foreach (var v in pattern)
            {
                v.Should().BeGreaterOrEqualTo(0, $"Value {v} is negative");
                v.Should().BeLessOrEqualTo(5, $"Value {v} exceeds max strength 5");
            }
        }
    }

    [Fact]
    public async Task Generator_DeviceIdsAreFormatted()
    {
        var response = await _client.GetAsync("/api/devices/generate?count=5&signalLength=3&maxStrength=9");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var devices = await response.Content.ReadFromJsonAsync<Dictionary<string, int[]>>(_jsonOptions);
        devices.Should().NotBeNull();
        devices!.Keys.Should().Contain("D01");
        devices.Keys.Should().Contain("D05");
    }

    [Fact]
    public async Task Simulator_ReturnsCorrectSignalLength()
    {
        var request = new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2, 3, 4],
                ["D02"] = [5, 6, 7, 8],
            }
        };

        var response = await _client.PostAsJsonAsync("/api/signal/simulate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.SignalLength.Should().Be(4);
        result.ReceivedSignal.Length.Should().Be(4);
    }

    [Fact]
    public async Task Simulator_ActiveCountWithinRange()
    {
        var request = new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2],
                ["D02"] = [3, 4],
                ["D03"] = [5, 6],
                ["D04"] = [7, 8],
                ["D05"] = [9, 0],
            }
        };

        var response = await _client.PostAsJsonAsync("/api/signal/simulate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.ActiveDeviceCount.Should().BeGreaterOrEqualTo(1);
        result.ActiveDeviceCount.Should().BeLessOrEqualTo(5);
        result.TotalDevices.Should().Be(5);
    }

    [Fact]
    public async Task Simulator_SignalValuesAreNonNegative()
    {
        var request = new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2, 3],
                ["D02"] = [4, 5, 6],
            }
        };

        var response = await _client.PostAsJsonAsync("/api/signal/simulate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SimulateResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.ReceivedSignal.Should().OnlyContain(v => v >= 0);
    }
}
