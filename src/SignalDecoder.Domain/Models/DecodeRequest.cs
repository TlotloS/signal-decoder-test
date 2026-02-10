namespace SignalDecoder.Domain.Models;

public class DecodeRequest
{
    public Dictionary<string, int[]> Devices { get; set; } = new();
    public int[] ReceivedSignal { get; set; } = Array.Empty<int>();
    public int Tolerance { get; set; } = 0;
}
