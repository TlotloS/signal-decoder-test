namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;

public class GeneratorAndSimulatorTests
{
    private readonly DeviceGeneratorService _generator = new();
    private readonly SignalSimulatorService _simulator = new();

    [Fact]
    public void Generator_ReturnsRequestedCount()
    {
        var devices = _generator.GenerateDevices(10, 4, 9);
        Assert.Equal(10, devices.Count);
    }

    [Fact]
    public void Generator_AllPatternsCorrectLength()
    {
        var devices = _generator.GenerateDevices(5, 7, 9);
        Assert.All(devices.Values, p => Assert.Equal(7, p.Length));
    }

    [Fact]
    public void Generator_ValuesWithinRange()
    {
        var devices = _generator.GenerateDevices(5, 4, 5);
        Assert.All(devices.Values, pattern =>
            Assert.All(pattern, v =>
            {
                Assert.True(v >= 0, $"Value {v} is negative");
                Assert.True(v <= 5, $"Value {v} exceeds max strength 5");
            }));
    }

    [Fact]
    public void Generator_DeviceIdsAreFormatted()
    {
        var devices = _generator.GenerateDevices(5, 3, 9);
        Assert.Contains("D01", devices.Keys);
        Assert.Contains("D05", devices.Keys);
    }

    [Fact]
    public void Simulator_ReturnsCorrectSignalLength()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2, 3, 4],
            ["D02"] = [5, 6, 7, 8],
        };

        var result = _simulator.Simulate(devices);

        Assert.Equal(4, result.SignalLength);
        Assert.Equal(4, result.ReceivedSignal.Length);
    }

    [Fact]
    public void Simulator_ActiveCountWithinRange()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2],
            ["D02"] = [3, 4],
            ["D03"] = [5, 6],
            ["D04"] = [7, 8],
            ["D05"] = [9, 0],
        };

        var result = _simulator.Simulate(devices);

        Assert.True(result.ActiveDeviceCount >= 1);
        Assert.True(result.ActiveDeviceCount <= 5);
        Assert.Equal(5, result.TotalDevices);
    }

    [Fact]
    public void Simulator_SignalValuesAreNonNegative()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2, 3],
            ["D02"] = [4, 5, 6],
        };

        var result = _simulator.Simulate(devices);

        Assert.All(result.ReceivedSignal, v => Assert.True(v >= 0));
    }
}
