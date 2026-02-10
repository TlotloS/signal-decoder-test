namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderToleranceTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_WithTolerance_FindsFuzzyMatch()
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

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1, "Should find at least one fuzzy match");
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02"));
        Assert.True(hasD01D02, "D01 + D02 should be a valid fuzzy match");
    }

    [Fact]
    public void Decode_WithTolerance_RejectsOutOfRange()
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

        var result = _solver.Decode(request);

        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.False(hasD01D02, "D01 + D02 should NOT match — difference exceeds tolerance");
    }

    [Fact]
    public void Decode_ToleranceZero_ExactMatchOnly()
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

        var result = _solver.Decode(request);

        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.False(hasD01D02, "With tolerance 0, off-by-one should not match");
    }
}
