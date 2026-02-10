namespace SignalDecoder.Domain.Models;

public class SimulateRequest
{
    public Dictionary<string, int[]> Devices { get; set; } = new();
}
