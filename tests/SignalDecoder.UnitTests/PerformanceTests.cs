using FluentAssertions;
using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;
using Xunit.Abstractions;

namespace SignalDecoder.UnitTests;

public class PerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Performance_20Devices_CompletesReasonably()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var generator = new DeviceGeneratorService(seed: 42);
        var devices = generator.GenerateDevices(20, 5, 9);
        var received = new[] { 15, 20, 18, 22, 16 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 2
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        _output.WriteLine($"20 devices: {response.SolveTimeMs}ms, {response.SolutionCount} solutions");
        response.SolveTimeMs.Should().BeLessThan(3000); // Should complete in < 3 seconds
    }

    [Fact]
    public void Performance_25Devices_CompletesUnderFiveSeconds()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var generator = new DeviceGeneratorService(seed: 123);
        var devices = generator.GenerateDevices(25, 5, 9);
        var received = new[] { 18, 25, 20, 28, 22 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 2
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        _output.WriteLine($"25 devices: {response.SolveTimeMs}ms, {response.SolutionCount} solutions");
        response.SolveTimeMs.Should().BeLessThan(5000);
    }

    [Fact]
    public void Performance_30Devices_StressTest()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var generator = new DeviceGeneratorService(seed: 456);
        var devices = generator.GenerateDevices(30, 5, 9);
        var received = new[] { 20, 30, 25, 35, 28 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 3
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        _output.WriteLine($"30 devices: {response.SolveTimeMs}ms, {response.SolutionCount} solutions");
        // This is a stress test - we just want to see how long it takes
        // May take longer but should still complete
        response.SolveTimeMs.Should().BeLessThan(30000); // Allow up to 30 seconds for 30 devices
    }

    [Fact]
    public void Performance_LongSignalPattern_CompletesWell()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var generator = new DeviceGeneratorService(seed: 789);
        var devices = generator.GenerateDevices(15, 20, 9); // 15 devices, LONG signal (20 positions)
        var received = new int[20];
        for (int i = 0; i < 20; i++)
        {
            received[i] = 10 + (i % 5);
        }
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 2
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        _output.WriteLine($"15 devices with 20-position signals: {response.SolveTimeMs}ms, {response.SolutionCount} solutions");
        response.SolveTimeMs.Should().BeLessThan(2000);
    }

    [Fact]
    public void Performance_PrefilteringEffectiveness()
    {
        // Arrange - create scenario where pre-filtering helps significantly
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 1, 1 } },
            { "D02", new[] { 2, 2, 2 } },
            { "D03", new[] { 3, 3, 3 } },
            // These should be filtered out - they're too large
            { "D04", new[] { 100, 100, 100 } },
            { "D05", new[] { 200, 200, 200 } },
            { "D06", new[] { 150, 150, 150 } },
            { "D07", new[] { 99, 99, 99 } },
            { "D08", new[] { 88, 88, 88 } }
        };
        var received = new[] { 5, 5, 5 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 1
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        _output.WriteLine($"Prefiltering test: {response.SolveTimeMs}ms, {response.SolutionCount} solutions");
        response.SolveTimeMs.Should().BeLessThan(100); // Should be very fast due to pre-filtering
        response.SolutionCount.Should().BeGreaterThan(0); // Should find some solutions
    }
}
