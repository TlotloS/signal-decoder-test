using SignalDecoder.Domain.Interfaces;

namespace SignalDecoder.Application.Services;

public class DeviceGeneratorService : IDeviceGeneratorService
{
    private readonly Random _random;

    public DeviceGeneratorService(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }


    public Dictionary<string, int[]> GenerateDevices(int count, int signalLength, int maxStrength)
    {
        var devices = new Dictionary<string, int[]>();

        for (int i = 1; i <= count; i++)
        {
            // Generate device ID with zero-padding: D01, D02, etc.
            var deviceId = $"D{i.ToString().PadLeft(2, '0')}";

            // Generate random signal pattern
            var signalPattern = new int[signalLength];
            for (int j = 0; j < signalLength; j++)
            {
                signalPattern[j] = _random.Next(0, maxStrength + 1); // 0 to maxStrength inclusive
            }

            devices[deviceId] = signalPattern;
        }

        return devices;
    }
}
