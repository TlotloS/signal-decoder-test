namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderCorrectnessTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_FiveDevices_FindsCorrectThree()
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

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1);
        var solution = result.Solutions[0];
        Assert.Equal(3, solution.TransmittingDevices.Count);
        Assert.Contains("D01", solution.TransmittingDevices);
        Assert.Contains("D03", solution.TransmittingDevices);
        Assert.Contains("D05", solution.TransmittingDevices);
    }

    [Fact]
    public void Decode_MultipleSolutions_ReturnsAll()
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

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 2,
            $"Expected at least 2 solutions but got {result.Solutions.Count}");
    }

    [Fact]
    public void Decode_EmptySubset_ZeroSignal()
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

        var result = _solver.Decode(request);

        // Should find exactly one solution: the empty set
        Assert.Single(result.Solutions);
        Assert.Empty(result.Solutions[0].TransmittingDevices);
    }

    [Fact]
    public void Decode_VerifiesComputedSum()
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
        Assert.Equal([4, 6, 6], result.Solutions[0].ComputedSum);
        Assert.True(result.Solutions[0].MatchesReceived);
    }

    [Fact]
    public void Decode_IncludesSolveTime()
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

        var result = _solver.Decode(request);

        Assert.True(result.SolveTimeMs >= 0, "SolveTimeMs should be populated");
    }

    [Fact]
    public void Decode_ReportsCorrectSolutionCount()
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

        Assert.Equal(result.Solutions.Count, result.SolutionCount);
    }
}
