namespace SignalDecoder.Domain.Models;

public class SimulateResponse
{
    public int[] ReceivedSignal { get; set; } = Array.Empty<int>();
    public int ActiveDeviceCount { get; set; }
    public int SignalLength { get; set; }
    public int TotalDevices { get; set; }
}
