namespace SignalDecoder.Domain.Models;

public class DecodeResult
{
    public List<string> TransmittingDevices { get; set; } = new();
    public Dictionary<string, int[]> DecodedSignals { get; set; } = new();
    public int[] ComputedSum { get; set; } = Array.Empty<int>();
    public bool MatchesReceived { get; set; }
}
