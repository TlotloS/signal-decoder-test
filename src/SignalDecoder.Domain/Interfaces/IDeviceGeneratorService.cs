namespace SignalDecoder.Domain.Interfaces;

public interface IDeviceGeneratorService
{
    Dictionary<string, int[]> GenerateDevices(int count, int signalLength, int maxStrength);
}
