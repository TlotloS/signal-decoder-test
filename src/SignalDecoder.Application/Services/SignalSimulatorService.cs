using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

namespace SignalDecoder.Application.Services;

public class SignalSimulatorService : ISignalSimulatorService
{
    private readonly Random _random;

    public SignalSimulatorService(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public SimulateResponse Simulate(Dictionary<string, int[]> devices)
    {
        if (devices == null || devices.Count == 0)
        {
            throw new ArgumentException("Devices dictionary cannot be null or empty.", nameof(devices));
        }

        // Get signal length from first device (all should be same length)
        var signalLength = devices.First().Value.Length;

        // Randomly select 1 to N devices to be active
        var deviceCount = devices.Count;
        var activeCount = _random.Next(1, deviceCount + 1); // 1 to deviceCount inclusive

        // Randomly select which devices are active
        var deviceIds = devices.Keys.ToList();
        var activeDeviceIds = deviceIds.OrderBy(_ => _random.Next()).Take(activeCount).ToList();

        // Compute the combined signal (sum of active device patterns)
        var combinedSignal = new int[signalLength];
        foreach (var deviceId in activeDeviceIds)
        {
            var pattern = devices[deviceId];
            for (int i = 0; i < signalLength; i++)
            {
                combinedSignal[i] += pattern[i];
            }
        }

        return new SimulateResponse
        {
            ReceivedSignal = combinedSignal,
            ActiveDeviceCount = activeCount,
            SignalLength = signalLength,
            TotalDevices = deviceCount
        };
    }
}
