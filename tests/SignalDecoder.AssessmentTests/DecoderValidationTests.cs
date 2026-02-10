namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderValidationTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_NoSolution_ReturnsEmptySolutions()
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

        var result = _solver.Decode(request);

        Assert.Empty(result.Solutions);
        Assert.Equal(0, result.SolutionCount);
    }

    [Fact]
    public void Decode_LargerSignalLength_StillWorks()
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

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1);
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.True(hasD01D02);
    }

    [Fact]
    public void Decode_SingleDeviceInList_CanMatch()
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

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Single(result.Solutions[0].TransmittingDevices);
        Assert.Contains("D01", result.Solutions[0].TransmittingDevices);
    }

    [Fact]
    public void Decode_DecodedSignals_ContainsCorrectPatterns()
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

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        var decoded = result.Solutions[0].DecodedSignals;
        Assert.Equal(2, decoded.Count);
        Assert.Equal([3, 1, 4], decoded["D01"]);
        Assert.Equal([1, 5, 2], decoded["D02"]);
    }
}
