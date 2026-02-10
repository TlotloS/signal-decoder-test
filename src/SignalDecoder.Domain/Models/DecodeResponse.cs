namespace SignalDecoder.Domain.Models;

public class DecodeResponse
{
    public List<DecodeResult> Solutions { get; set; } = new();
    public int SolutionCount { get; set; }
    public long SolveTimeMs { get; set; }
}
