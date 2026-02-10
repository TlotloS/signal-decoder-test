using SignalDecoder.Domain.Models;

namespace SignalDecoder.Domain.Interfaces;

public interface ISignalSimulatorService
{
    SimulateResponse Simulate(Dictionary<string, int[]> devices);
}
