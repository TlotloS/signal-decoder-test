using FluentAssertions;
using SignalDecoder.Application.Services;

namespace SignalDecoder.UnitTests;

public class SignalSimulatorServiceTests
{
    [Fact]
    public void Simulate_ReturnsSameLengthSignal()
    {
        // Arrange
        var simulator = new SignalSimulatorService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2, 3, 4 } },
            { "D02", new[] { 5, 6, 7, 8 } },
            { "D03", new[] { 0, 1, 0, 1 } }
        };

        // Act
        var response = simulator.Simulate(devices);

        // Assert
        response.ReceivedSignal.Length.Should().Be(4);
        response.SignalLength.Should().Be(4);
    }

    [Fact]
    public void Simulate_ActiveCountWithinRange()
    {
        // Arrange
        var simulator = new SignalSimulatorService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2 } },
            { "D02", new[] { 3, 4 } },
            { "D03", new[] { 5, 6 } }
        };

        // Act
        var response = simulator.Simulate(devices);

        // Assert
        response.ActiveDeviceCount.Should().BeGreaterOrEqualTo(1);
        response.ActiveDeviceCount.Should().BeLessOrEqualTo(3);
        response.TotalDevices.Should().Be(3);
    }

    [Fact]
    public void Simulate_SignalIsNonNegative()
    {
        // Arrange
        var simulator = new SignalSimulatorService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2, 3 } },
            { "D02", new[] { 4, 5, 6 } }
        };

        // Act
        var response = simulator.Simulate(devices);

        // Assert
        response.ReceivedSignal.Should().OnlyContain(value => value >= 0);
    }

    [Fact]
    public void Simulate_WithSeed_IsReproducible()
    {
        // Arrange
        var seed = 42;
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2, 3 } },
            { "D02", new[] { 4, 5, 6 } },
            { "D03", new[] { 7, 8, 9 } }
        };
        var simulator1 = new SignalSimulatorService(seed);
        var simulator2 = new SignalSimulatorService(seed);

        // Act
        var response1 = simulator1.Simulate(devices);
        var response2 = simulator2.Simulate(devices);

        // Assert
        response1.ReceivedSignal.Should().Equal(response2.ReceivedSignal);
        response1.ActiveDeviceCount.Should().Be(response2.ActiveDeviceCount);
    }

    [Fact]
    public void Simulate_EmptyDevices_ThrowsException()
    {
        // Arrange
        var simulator = new SignalSimulatorService();
        var devices = new Dictionary<string, int[]>();

        // Act & Assert
        var act = () => simulator.Simulate(devices);
        act.Should().Throw<ArgumentException>();
    }
}
