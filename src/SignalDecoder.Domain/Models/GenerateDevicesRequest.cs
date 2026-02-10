namespace SignalDecoder.Domain.Models;

public class GenerateDevicesRequest
{
    public int Count { get; set; } = 5;
    public int SignalLength { get; set; } = 4;
    public int MaxSignalStrength { get; set; } = 9;
}
