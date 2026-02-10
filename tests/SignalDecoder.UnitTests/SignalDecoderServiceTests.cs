using FluentAssertions;
using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

namespace SignalDecoder.UnitTests;

public class SignalDecoderServiceTests
{
    [Fact]
    public void SimpleCase_TwoDevicesFromFive_FindsCorrectPair()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2, 3, 4 } },
            { "D02", new[] { 2, 1, 0, 1 } },
            { "D03", new[] { 0, 0, 1, 0 } },
            { "D04", new[] { 1, 1, 1, 1 } },
            { "D05", new[] { 0, 1, 0, 2 } }
        };
        var received = new[] { 3, 3, 3, 5 }; // D01 + D02
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolutionCount.Should().Be(1);
        response.Solutions[0].TransmittingDevices.Should().BeEquivalentTo(new[] { "D01", "D02" });
        response.Solutions[0].ComputedSum.Should().Equal(received);
        response.Solutions[0].MatchesReceived.Should().BeTrue();
    }

    [Fact]
    public void NoSolution_ImpossibleSignal_ReturnsEmpty()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 1 } },
            { "D02", new[] { 2, 2 } }
        };
        var received = new[] { 5, 5 }; // Impossible - max sum is 3, 3
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolutionCount.Should().Be(0);
        response.Solutions.Should().BeEmpty();
    }

    [Fact]
    public void MultipleSolutions_ShortSignal_ReturnsAll()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 1 } },
            { "D02", new[] { 1, 1 } },
            { "D03", new[] { 2, 2 } }
        };
        var received = new[] { 2, 2 }; // Can be D01+D02 or D03 alone
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolutionCount.Should().Be(2);
        var deviceSets = response.Solutions.Select(s => s.TransmittingDevices).ToList();
        deviceSets.Should().ContainEquivalentOf(new[] { "D01", "D02" });
        deviceSets.Should().ContainEquivalentOf(new[] { "D03" });
    }

    [Fact]
    public void SingleDevice_ExactMatch_Works()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 5, 3, 2 } }
        };
        var received = new[] { 5, 3, 2 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolutionCount.Should().Be(1);
        response.Solutions[0].TransmittingDevices.Should().ContainSingle().Which.Should().Be("D01");
    }

    [Fact]
    public void AllDevices_SumOfAll_Works()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 0 } },
            { "D02", new[] { 0, 1 } },
            { "D03", new[] { 2, 2 } }
        };
        var received = new[] { 3, 3 }; // Sum of all devices
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.Solutions.Should().ContainSingle(s =>
            s.TransmittingDevices.Count == 3 &&
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Contains("D03"));
    }

    [Fact]
    public void NoDevicesActive_ZeroSignal_ReturnsEmpty()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2 } },
            { "D02", new[] { 3, 4 } }
        };
        var received = new[] { 0, 0 }; // No devices transmitting
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        // Should find the empty set (no devices selected)
        response.Solutions.Should().ContainSingle(s => s.TransmittingDevices.Count == 0);
    }

    [Fact]
    public void WithTolerance_FuzzyMatch_FindsSolution()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 5, 3 } },
            { "D02", new[] { 2, 4 } }
        };
        var received = new[] { 7, 8 }; // Exact would be 7, 7 but with tolerance=1, 7,8 matches
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 1
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolutionCount.Should().BeGreaterThan(0);
        response.Solutions.Should().Contain(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02"));
    }

    [Fact]
    public void WithTolerance_StillPrunes_DoesntReturnEverything()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 1 } },
            { "D02", new[] { 1, 1 } },
            { "D03", new[] { 9, 9 } }
        };
        var received = new[] { 2, 2 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 1
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        // D03 alone is way off (9,9 vs 2,2), should not be in solutions
        response.Solutions.Should().NotContain(s =>
            s.TransmittingDevices.Count == 1 &&
            s.TransmittingDevices.Contains("D03"));
    }

    [Fact]
    public void LargerInput_15Devices_CompletesUnderOneSecond()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var generator = new DeviceGeneratorService(seed: 42);
        var devices = generator.GenerateDevices(15, 5, 9);
        var received = new[] { 10, 15, 12, 18, 14 }; // Some target signal
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 2
        };

        // Act
        var response = decoder.Decode(request);

        // Assert
        response.SolveTimeMs.Should().BeLessThan(1000);
    }

    [Fact]
    public void Validation_MismatchedLengths_Throws()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, 2, 3 } },
            { "D02", new[] { 4, 5 } } // Different length!
        };
        var received = new[] { 5, 7, 3 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act & Assert
        var act = () => decoder.Decode(request);
        act.Should().Throw<ArgumentException>().WithMessage("*same length*");
    }

    [Fact]
    public void Validation_NegativeValues_Throws()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var devices = new Dictionary<string, int[]>
        {
            { "D01", new[] { 1, -2, 3 } } // Negative value!
        };
        var received = new[] { 1, 2, 3 };
        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        // Act & Assert
        var act = () => decoder.Decode(request);
        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Fact]
    public void Validation_EmptyDevices_Throws()
    {
        // Arrange
        var decoder = new SignalDecoderService();
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>(), // Empty!
            ReceivedSignal = new[] { 1, 2 },
            Tolerance = 0
        };

        // Act & Assert
        var act = () => decoder.Decode(request);
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }
}
