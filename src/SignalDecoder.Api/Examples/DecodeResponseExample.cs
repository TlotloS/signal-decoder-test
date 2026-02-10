using SignalDecoder.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace SignalDecoder.Api.Examples;

public class DecodeResponseExample : IExamplesProvider<DecodeResponse>
{
    public DecodeResponse GetExamples()
    {
        return new DecodeResponse
        {
            Solutions = new List<DecodeResult>
            {
                new DecodeResult
                {
                    TransmittingDevices = new List<string> { "D01", "D02", "D03" },
                    DecodedSignals = new Dictionary<string, int[]>
                    {
                        { "D01", new[] { 2, 5, 1, 3 } },
                        { "D02", new[] { 1, 0, 4, 2 } },
                        { "D03", new[] { 3, 2, 0, 1 } }
                    },
                    ComputedSum = new[] { 6, 7, 5, 6 },
                    MatchesReceived = true
                }
            },
            SolutionCount = 1,
            SolveTimeMs = 5
        };
    }
}
