using SignalDecoder.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace SignalDecoder.Api.Examples;

public class SimulateRequestExample : IExamplesProvider<SimulateRequest>
{
    public SimulateRequest GetExamples()
    {
        return new SimulateRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                { "D01", new[] { 2, 5, 1, 3 } },
                { "D02", new[] { 1, 0, 4, 2 } },
                { "D03", new[] { 3, 2, 0, 1 } },
                { "D04", new[] { 0, 1, 2, 4 } },
                { "D05", new[] { 4, 3, 2, 0 } }
            }
        };
    }
}
