using SignalDecoder.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace SignalDecoder.Api.Examples;

public class SimulateResponseExample : IExamplesProvider<SimulateResponse>
{
    public SimulateResponse GetExamples()
    {
        return new SimulateResponse
        {
            ReceivedSignal = new[] { 6, 7, 3, 5 },
            ActiveDeviceCount = 3,
            SignalLength = 4,
            TotalDevices = 5
        };
    }
}
