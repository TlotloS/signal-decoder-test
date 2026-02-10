using FluentAssertions;
using SignalDecoder.Application.Services;

namespace SignalDecoder.UnitTests;

public class DeviceGeneratorServiceTests
{
    [Fact]
    public void GenerateDevices_ReturnsCorrectCount()
    {
        // Arrange
        var generator = new DeviceGeneratorService();
        var count = 10;

        // Act
        var devices = generator.GenerateDevices(count, 4, 9);

        // Assert
        devices.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateDevices_PatternsHaveCorrectLength()
    {
        // Arrange
        var generator = new DeviceGeneratorService();
        var signalLength = 6;

        // Act
        var devices = generator.GenerateDevices(5, signalLength, 9);

        // Assert
        devices.Values.Should().OnlyContain(pattern => pattern.Length == signalLength);
    }

    [Fact]
    public void GenerateDevices_ValuesWithinRange()
    {
        // Arrange
        var generator = new DeviceGeneratorService();
        var maxStrength = 7;

        // Act
        var devices = generator.GenerateDevices(5, 4, maxStrength);

        // Assert
        foreach (var pattern in devices.Values)
        {
            pattern.Should().OnlyContain(value => value >= 0 && value <= maxStrength);
        }
    }

    [Fact]
    public void GenerateDevices_WithSeed_IsReproducible()
    {
        // Arrange
        var seed = 12345;
        var generator1 = new DeviceGeneratorService(seed);
        var generator2 = new DeviceGeneratorService(seed);

        // Act
        var devices1 = generator1.GenerateDevices(5, 4, 9);
        var devices2 = generator2.GenerateDevices(5, 4, 9);

        // Assert
        devices1.Should().BeEquivalentTo(devices2);
    }

    [Fact]
    public void GenerateDevices_DeviceIdsAreFormatted()
    {
        // Arrange
        var generator = new DeviceGeneratorService();

        // Act
        var devices = generator.GenerateDevices(15, 4, 9);

        // Assert
        devices.Keys.Should().Contain("D01");
        devices.Keys.Should().Contain("D09");
        devices.Keys.Should().Contain("D10");
        devices.Keys.Should().Contain("D15");
    }
}
