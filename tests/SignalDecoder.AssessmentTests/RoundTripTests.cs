namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class RoundTripTests
{
    [Fact]
    public void FullWorkflow_Generate_Simulate_Decode_RoundTrips()
    {
        var generator = new DeviceGeneratorService();
        var simulator = new SignalSimulatorService();
        var decoder = new SignalDecoderService();

        // Generate devices
        var devices = generator.GenerateDevices(5, 4, 9);
        Assert.Equal(5, devices.Count);

        // Simulate transmission
        var simResult = simulator.Simulate(devices);
        Assert.True(simResult.ActiveDeviceCount >= 1);

        // Decode the signal
        var decodeRequest = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = simResult.ReceivedSignal,
            Tolerance = 0
        };

        var decodeResult = decoder.Decode(decodeRequest);

        // Verify at least one solution exists
        Assert.True(decodeResult.Solutions.Count >= 1,
            "Decoder should find the solution for a simulated signal");

        // Verify the solution has the correct number of active devices
        var matchingSolution = decodeResult.Solutions
            .FirstOrDefault(s => s.TransmittingDevices.Count == simResult.ActiveDeviceCount);

        // Note: there may be multiple subsets that produce the same sum,
        // so we verify any solution actually sums correctly
        foreach (var solution in decodeResult.Solutions)
        {
            var length = simResult.ReceivedSignal.Length;
            var computed = new int[length];
            foreach (var deviceId in solution.TransmittingDevices)
                for (int i = 0; i < length; i++)
                    computed[i] += devices[deviceId][i];

            Assert.Equal(simResult.ReceivedSignal, computed);
        }
    }
}
