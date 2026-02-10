# Signal Decoder API — Complete Build Script (Claude Code)

## Overview

Build a complete ASP.NET Core 8 Web API solution for a "Signal Decoder" technical assessment. This includes:

1. **Template repo** — what the candidate gets (interfaces, models, sample tests, GitHub Action)
2. **Hidden tests** — compiled DLL that runs against the candidate's implementation
3. **Reference implementation** — your answer key (kept in a separate folder, never given to candidates)

## Part 1: Solution Structure

Create everything under a root folder called `signal-decoder-assessment`:

```
signal-decoder-assessment/
├── template/                          # THIS IS WHAT THE CANDIDATE GETS
│   ├── src/
│   │   ├── SignalDecoder.Api/
│   │   ├── SignalDecoder.Domain/
│   │   └── SignalDecoder.Application/
│   ├── tests/
│   │   └── SignalDecoder.SampleTests/
│   ├── .github/
│   │   └── workflows/
│   │       └── assessment.yml
│   ├── SignalDecoder.sln
│   ├── .gitignore
│   └── README.md
│
├── reference/                         # YOUR ANSWER KEY — never shared
│   └── SignalDecoder.ReferenceSolver/
│
├── hidden-tests/                      # COMPILED INTO DLL, injected into template
│   └── SignalDecoder.AssessmentTests/
│
└── scripts/
    ├── build-hidden-tests.sh
    ├── inject-tests.sh
    └── evaluate-candidate.sh
```

---

## Part 2: Domain Layer (template/src/SignalDecoder.Domain)

Create a class library project.

### Models

```csharp
// Models/DecodeRequest.cs
namespace SignalDecoder.Domain.Models;

public class DecodeRequest
{
    public Dictionary<string, int[]> Devices { get; set; } = new();
    public int[] ReceivedSignal { get; set; } = [];
    public int Tolerance { get; set; } = 0;
}
```

```csharp
// Models/DecodeResult.cs
namespace SignalDecoder.Domain.Models;

public class DecodeResult
{
    public List<string> TransmittingDevices { get; set; } = [];
    public Dictionary<string, int[]> DecodedSignals { get; set; } = new();
    public int[] ComputedSum { get; set; } = [];
    public bool MatchesReceived { get; set; }
}
```

```csharp
// Models/DecodeResponse.cs
namespace SignalDecoder.Domain.Models;

public class DecodeResponse
{
    public List<DecodeResult> Solutions { get; set; } = [];
    public int SolutionCount { get; set; }
    public long SolveTimeMs { get; set; }
}
```

```csharp
// Models/SimulateRequest.cs
namespace SignalDecoder.Domain.Models;

public class SimulateRequest
{
    public Dictionary<string, int[]> Devices { get; set; } = new();
}
```

```csharp
// Models/SimulateResponse.cs
namespace SignalDecoder.Domain.Models;

public class SimulateResponse
{
    public int[] ReceivedSignal { get; set; } = [];
    public int ActiveDeviceCount { get; set; }
    public int SignalLength { get; set; }
    public int TotalDevices { get; set; }
}
```

### Interfaces

```csharp
// Interfaces/ISignalDecoderService.cs
namespace SignalDecoder.Domain.Interfaces;

using SignalDecoder.Domain.Models;

public interface ISignalDecoderService
{
    DecodeResponse Decode(DecodeRequest request);
}
```

```csharp
// Interfaces/IDeviceGeneratorService.cs
namespace SignalDecoder.Domain.Interfaces;

public interface IDeviceGeneratorService
{
    Dictionary<string, int[]> GenerateDevices(int count, int signalLength, int maxStrength);
}
```

```csharp
// Interfaces/ISignalSimulatorService.cs
namespace SignalDecoder.Domain.Interfaces;

using SignalDecoder.Domain.Models;

public interface ISignalSimulatorService
{
    SimulateResponse Simulate(Dictionary<string, int[]> devices);
}
```

### Service Registration Extension

```csharp
// DependencyInjection.cs
namespace SignalDecoder.Domain;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Candidates must call this to register their service implementations.
    /// The hidden tests use this method to resolve services.
    /// </summary>
    public static IServiceCollection AddSignalDecoderServices(this IServiceCollection services)
    {
        // Candidate registers their implementations here:
        // services.AddSingleton<ISignalDecoderService, YourDecoderService>();
        // services.AddSingleton<IDeviceGeneratorService, YourGeneratorService>();
        // services.AddSingleton<ISignalSimulatorService, YourSimulatorService>();
        
        // NOTE TO CANDIDATE: Uncomment and replace with your implementations
        return services;
    }
}
```

IMPORTANT: The Domain project must reference `Microsoft.Extensions.DependencyInjection.Abstractions` NuGet package for the `IServiceCollection` extension method.

---

## Part 3: Application Layer (template/src/SignalDecoder.Application)

Create a class library project. This is where the candidate writes their implementation.

Include placeholder files to guide them:

```csharp
// Services/DeviceGeneratorService.cs
namespace SignalDecoder.Application.Services;

using SignalDecoder.Domain.Interfaces;

public class DeviceGeneratorService : IDeviceGeneratorService
{
    public Dictionary<string, int[]> GenerateDevices(int count, int signalLength, int maxStrength)
    {
        // TODO: Implement device generation
        // - Generate 'count' devices with IDs "D01", "D02", etc.
        // - Each device has a signal pattern of 'signalLength' random integers
        // - Values between 0 and 'maxStrength'
        throw new NotImplementedException();
    }
}
```

```csharp
// Services/SignalSimulatorService.cs
namespace SignalDecoder.Application.Services;

using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

public class SignalSimulatorService : ISignalSimulatorService
{
    public SimulateResponse Simulate(Dictionary<string, int[]> devices)
    {
        // TODO: Implement signal simulation
        // - Randomly select a subset of devices
        // - Sum their patterns position by position
        // - Return the combined signal and active count
        // - Do NOT return which devices were selected
        throw new NotImplementedException();
    }
}
```

```csharp
// Services/SignalDecoderService.cs
namespace SignalDecoder.Application.Services;

using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

public class SignalDecoderService : ISignalDecoderService
{
    public DecodeResponse Decode(DecodeRequest request)
    {
        // TODO: This is the core problem — implement the decoder
        // Given a set of devices and a received signal,
        // find which combination of devices produced the signal.
        //
        // Requirements:
        // - tolerance = 0: exact match at every position
        // - tolerance > 0: |computed - received| <= tolerance at each position
        // - Return ALL valid combinations
        // - Measure and return solve time in milliseconds
        throw new NotImplementedException();
    }
}
```

---

## Part 4: API Layer (template/src/SignalDecoder.Api)

Create a webapi project.

### Controllers

```csharp
// Controllers/DevicesController.cs
namespace SignalDecoder.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalDecoder.Domain.Interfaces;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceGeneratorService _generator;

    public DevicesController(IDeviceGeneratorService generator)
    {
        _generator = generator;
    }

    /// <summary>
    /// Generates a set of random devices with signal patterns.
    /// </summary>
    [HttpGet("generate")]
    public IActionResult Generate(
        [FromQuery] int count = 5,
        [FromQuery] int signalLength = 4,
        [FromQuery] int maxStrength = 9)
    {
        if (count < 1 || count > 100)
            return BadRequest("Count must be between 1 and 100");
        if (signalLength < 1 || signalLength > 20)
            return BadRequest("Signal length must be between 1 and 20");
        if (maxStrength < 1 || maxStrength > 100)
            return BadRequest("Max strength must be between 1 and 100");

        var devices = _generator.GenerateDevices(count, signalLength, maxStrength);
        return Ok(new { devices });
    }
}
```

```csharp
// Controllers/SignalController.cs
namespace SignalDecoder.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

[ApiController]
[Route("api/signal")]
public class SignalController : ControllerBase
{
    private readonly ISignalSimulatorService _simulator;
    private readonly ISignalDecoderService _decoder;

    public SignalController(ISignalSimulatorService simulator, ISignalDecoderService decoder)
    {
        _simulator = simulator;
        _decoder = decoder;
    }

    /// <summary>
    /// Simulates a transmission. Returns combined signal but not which devices transmitted.
    /// </summary>
    [HttpPost("simulate")]
    public IActionResult Simulate([FromBody] SimulateRequest request)
    {
        if (request.Devices == null || request.Devices.Count == 0)
            return BadRequest("At least one device is required");

        var lengths = request.Devices.Values.Select(v => v.Length).Distinct().ToList();
        if (lengths.Count > 1)
            return BadRequest("All device signal patterns must have the same length");

        var result = _simulator.Simulate(request.Devices);
        return Ok(result);
    }

    /// <summary>
    /// Decodes a received signal to determine which devices were transmitting.
    /// </summary>
    [HttpPost("decode")]
    public IActionResult Decode([FromBody] DecodeRequest request)
    {
        if (request.Devices == null || request.Devices.Count == 0)
            return BadRequest("At least one device is required");

        if (request.ReceivedSignal == null || request.ReceivedSignal.Length == 0)
            return BadRequest("Received signal is required");

        var lengths = request.Devices.Values.Select(v => v.Length).Distinct().ToList();
        if (lengths.Count > 1)
            return BadRequest("All device signal patterns must have the same length");

        if (lengths[0] != request.ReceivedSignal.Length)
            return BadRequest("Received signal length must match device signal pattern length");

        if (request.Tolerance < 0)
            return BadRequest("Tolerance must be non-negative");

        if (request.ReceivedSignal.Any(v => v < 0))
            return BadRequest("Signal values must be non-negative");

        var result = _decoder.Decode(request);
        return Ok(result);
    }
}
```

### Program.cs

```csharp
using SignalDecoder.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Signal Decoder API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Register candidate's service implementations
builder.Services.AddSignalDecoderServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
```

### .gitignore

Use the standard .NET gitignore. Include:
```
bin/
obj/
*.user
*.vs/
.idea/
TestResults/
```

---

## Part 5: Sample Tests (template/tests/SignalDecoder.SampleTests)

Create an xUnit test project. These tests are visible to the candidate to help them get started.

```csharp
// BasicDecoderTests.cs
namespace SignalDecoder.SampleTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class BasicDecoderTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_TwoDevicesFromThree_FindsCorrectPair()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 6, 6],  // D01 + D02
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Contains("D01", result.Solutions[0].TransmittingDevices);
        Assert.Contains("D02", result.Solutions[0].TransmittingDevices);
        Assert.Equal(2, result.Solutions[0].TransmittingDevices.Count);
    }

    [Fact]
    public void Decode_NoMatch_ReturnsEmptySolutions()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 1, 1],
                ["D02"] = [2, 2, 2],
            },
            ReceivedSignal = [9, 9, 9],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Empty(result.Solutions);
    }

    [Fact]
    public void Decode_SingleDevice_FindsIt()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [5, 3, 7],
                ["D02"] = [2, 8, 1],
            },
            ReceivedSignal = [5, 3, 7],  // just D01
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Single(result.Solutions[0].TransmittingDevices);
        Assert.Contains("D01", result.Solutions[0].TransmittingDevices);
    }

    [Fact]
    public void Decode_AllDevices_FindsAll()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2, 3],
                ["D02"] = [4, 5, 6],
            },
            ReceivedSignal = [5, 7, 9],  // D01 + D02
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Equal(2, result.Solutions[0].TransmittingDevices.Count);
    }
}
```

```csharp
// BasicGeneratorTests.cs
namespace SignalDecoder.SampleTests;

using SignalDecoder.Application.Services;

public class BasicGeneratorTests
{
    private readonly DeviceGeneratorService _generator = new();

    [Fact]
    public void Generate_ReturnsCorrectCount()
    {
        var devices = _generator.GenerateDevices(5, 4, 9);
        Assert.Equal(5, devices.Count);
    }

    [Fact]
    public void Generate_PatternsHaveCorrectLength()
    {
        var devices = _generator.GenerateDevices(3, 6, 9);
        Assert.All(devices.Values, pattern => Assert.Equal(6, pattern.Length));
    }
}
```

---

## Part 6: Hidden Assessment Tests (hidden-tests/SignalDecoder.AssessmentTests)

Create an xUnit test project. This will be compiled into a DLL and injected into the candidate repo.

IMPORTANT: This project must reference SignalDecoder.Domain and SignalDecoder.Application from the template. Use relative paths that will work when the project is copied into the template's tests/ folder.

The .csproj should reference:
```xml
<ProjectReference Include="..\..\src\SignalDecoder.Domain\SignalDecoder.Domain.csproj" />
<ProjectReference Include="..\..\src\SignalDecoder.Application\SignalDecoder.Application.csproj" />
```

### Test Classes

```csharp
// DecoderCorrectnessTests.cs
namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderCorrectnessTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_FiveDevices_FindsCorrectThree()
    {
        // Pre-computed: D01 + D03 + D05 = received
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [2, 4, 1, 3],
                ["D02"] = [7, 1, 5, 2],
                ["D03"] = [3, 6, 2, 8],
                ["D04"] = [1, 0, 9, 4],
                ["D05"] = [5, 2, 3, 1],
            },
            ReceivedSignal = [10, 12, 6, 12],  // D01 + D03 + D05
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1);
        var solution = result.Solutions[0];
        Assert.Equal(3, solution.TransmittingDevices.Count);
        Assert.Contains("D01", solution.TransmittingDevices);
        Assert.Contains("D03", solution.TransmittingDevices);
        Assert.Contains("D05", solution.TransmittingDevices);
    }

    [Fact]
    public void Decode_MultipleSolutions_ReturnsAll()
    {
        // Short signal length with values designed so multiple subsets match
        // D01 + D02 = [5, 5] and D03 = [5, 5]
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [2, 3],
                ["D02"] = [3, 2],
                ["D03"] = [5, 5],
            },
            ReceivedSignal = [5, 5],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 2,
            $"Expected at least 2 solutions but got {result.Solutions.Count}");
    }

    [Fact]
    public void Decode_EmptySubset_ZeroSignal()
    {
        // Received signal is all zeros — the "no devices active" case
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2],
                ["D02"] = [3, 4],
            },
            ReceivedSignal = [0, 0],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        // Should find exactly one solution: the empty set
        Assert.Single(result.Solutions);
        Assert.Empty(result.Solutions[0].TransmittingDevices);
    }

    [Fact]
    public void Decode_VerifiesComputedSum()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 6, 6],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Equal([4, 6, 6], result.Solutions[0].ComputedSum);
        Assert.True(result.Solutions[0].MatchesReceived);
    }

    [Fact]
    public void Decode_IncludesSolveTime()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2],
                ["D02"] = [3, 4],
            },
            ReceivedSignal = [4, 6],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.SolveTimeMs >= 0, "SolveTimeMs should be populated");
    }

    [Fact]
    public void Decode_ReportsCorrectSolutionCount()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 6, 6],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Equal(result.Solutions.Count, result.SolutionCount);
    }
}
```

```csharp
// DecoderToleranceTests.cs
namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderToleranceTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_WithTolerance_FindsFuzzyMatch()
    {
        // Exact sum of D01 + D02 = [4, 6, 6]
        // Received is [4, 7, 6] — off by 1 at position 1
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 7, 6],
            Tolerance = 1
        };

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1, "Should find at least one fuzzy match");
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02"));
        Assert.True(hasD01D02, "D01 + D02 should be a valid fuzzy match");
    }

    [Fact]
    public void Decode_WithTolerance_RejectsOutOfRange()
    {
        // D01 + D02 = [4, 6, 6], received = [4, 9, 6] — off by 3 at position 1
        // Tolerance of 1 should NOT match
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
            },
            ReceivedSignal = [4, 9, 6],
            Tolerance = 1
        };

        var result = _solver.Decode(request);

        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.False(hasD01D02, "D01 + D02 should NOT match — difference exceeds tolerance");
    }

    [Fact]
    public void Decode_ToleranceZero_ExactMatchOnly()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
            },
            ReceivedSignal = [4, 7, 6],  // off by 1
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.False(hasD01D02, "With tolerance 0, off-by-one should not match");
    }
}
```

```csharp
// DecoderPerformanceTests.cs
namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderPerformanceTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_10Devices_CompletesUnder1Second()
    {
        var devices = GenerateTestDevices(10, 5, seed: 42);
        // Pick 4 devices to be active
        var activeKeys = devices.Keys.Take(4).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.SolveTimeMs < 1000,
            $"Took {result.SolveTimeMs}ms — should be under 1000ms for 10 devices");
        Assert.True(result.Solutions.Count >= 1, "Should find at least one solution");
    }

    [Fact]
    public void Decode_15Devices_CompletesUnder3Seconds()
    {
        var devices = GenerateTestDevices(15, 5, seed: 123);
        var activeKeys = devices.Keys.Take(5).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.SolveTimeMs < 3000,
            $"Took {result.SolveTimeMs}ms — should be under 3000ms for 15 devices");
        Assert.True(result.Solutions.Count >= 1, "Should find at least one solution");
    }

    [Fact]
    public void Decode_20Devices_CompletesUnder5Seconds()
    {
        var devices = GenerateTestDevices(20, 6, seed: 456);
        var activeKeys = devices.Keys.Take(7).ToList();
        var received = ComputeSum(devices, activeKeys);

        var request = new DecodeRequest
        {
            Devices = devices,
            ReceivedSignal = received,
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.SolveTimeMs < 5000,
            $"Took {result.SolveTimeMs}ms — should be under 5000ms for 20 devices");
        Assert.True(result.Solutions.Count >= 1, "Should find at least one solution");
    }

    // Helper methods
    private static Dictionary<string, int[]> GenerateTestDevices(int count, int length, int seed)
    {
        var rng = new Random(seed);
        var devices = new Dictionary<string, int[]>();
        for (int i = 0; i < count; i++)
        {
            var pattern = new int[length];
            for (int j = 0; j < length; j++)
                pattern[j] = rng.Next(0, 10);
            devices[$"D{(i + 1):D2}"] = pattern;
        }
        return devices;
    }

    private static int[] ComputeSum(Dictionary<string, int[]> devices, List<string> activeKeys)
    {
        var length = devices.Values.First().Length;
        var sum = new int[length];
        foreach (var key in activeKeys)
            for (int i = 0; i < length; i++)
                sum[i] += devices[key][i];
        return sum;
    }
}
```

```csharp
// DecoderValidationTests.cs
namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;
using SignalDecoder.Domain.Models;

public class DecoderValidationTests
{
    private readonly SignalDecoderService _solver = new();

    [Fact]
    public void Decode_NoSolution_ReturnsEmptySolutions()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 1],
                ["D02"] = [2, 2],
            },
            ReceivedSignal = [9, 9],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Empty(result.Solutions);
        Assert.Equal(0, result.SolutionCount);
    }

    [Fact]
    public void Decode_LargerSignalLength_StillWorks()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [1, 2, 3, 4, 5, 6, 7, 8],
                ["D02"] = [8, 7, 6, 5, 4, 3, 2, 1],
                ["D03"] = [1, 1, 1, 1, 1, 1, 1, 1],
            },
            ReceivedSignal = [9, 9, 9, 9, 9, 9, 9, 9],  // D01 + D02
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.True(result.Solutions.Count >= 1);
        var hasD01D02 = result.Solutions.Any(s =>
            s.TransmittingDevices.Contains("D01") &&
            s.TransmittingDevices.Contains("D02") &&
            s.TransmittingDevices.Count == 2);
        Assert.True(hasD01D02);
    }

    [Fact]
    public void Decode_SingleDeviceInList_CanMatch()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [5, 5, 5],
            },
            ReceivedSignal = [5, 5, 5],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        Assert.Single(result.Solutions[0].TransmittingDevices);
        Assert.Contains("D01", result.Solutions[0].TransmittingDevices);
    }

    [Fact]
    public void Decode_DecodedSignals_ContainsCorrectPatterns()
    {
        var request = new DecodeRequest
        {
            Devices = new Dictionary<string, int[]>
            {
                ["D01"] = [3, 1, 4],
                ["D02"] = [1, 5, 2],
                ["D03"] = [2, 0, 3],
            },
            ReceivedSignal = [4, 6, 6],
            Tolerance = 0
        };

        var result = _solver.Decode(request);

        Assert.Single(result.Solutions);
        var decoded = result.Solutions[0].DecodedSignals;
        Assert.Equal(2, decoded.Count);
        Assert.Equal([3, 1, 4], decoded["D01"]);
        Assert.Equal([1, 5, 2], decoded["D02"]);
    }
}
```

```csharp
// GeneratorAndSimulatorTests.cs
namespace SignalDecoder.AssessmentTests;

using SignalDecoder.Application.Services;

public class GeneratorAndSimulatorTests
{
    private readonly DeviceGeneratorService _generator = new();
    private readonly SignalSimulatorService _simulator = new();

    [Fact]
    public void Generator_ReturnsRequestedCount()
    {
        var devices = _generator.GenerateDevices(10, 4, 9);
        Assert.Equal(10, devices.Count);
    }

    [Fact]
    public void Generator_AllPatternsCorrectLength()
    {
        var devices = _generator.GenerateDevices(5, 7, 9);
        Assert.All(devices.Values, p => Assert.Equal(7, p.Length));
    }

    [Fact]
    public void Generator_ValuesWithinRange()
    {
        var devices = _generator.GenerateDevices(5, 4, 5);
        Assert.All(devices.Values, pattern =>
            Assert.All(pattern, v =>
            {
                Assert.True(v >= 0, $"Value {v} is negative");
                Assert.True(v <= 5, $"Value {v} exceeds max strength 5");
            }));
    }

    [Fact]
    public void Generator_DeviceIdsAreFormatted()
    {
        var devices = _generator.GenerateDevices(5, 3, 9);
        Assert.Contains("D01", devices.Keys);
        Assert.Contains("D05", devices.Keys);
    }

    [Fact]
    public void Simulator_ReturnsCorrectSignalLength()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2, 3, 4],
            ["D02"] = [5, 6, 7, 8],
        };

        var result = _simulator.Simulate(devices);

        Assert.Equal(4, result.SignalLength);
        Assert.Equal(4, result.ReceivedSignal.Length);
    }

    [Fact]
    public void Simulator_ActiveCountWithinRange()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2],
            ["D02"] = [3, 4],
            ["D03"] = [5, 6],
            ["D04"] = [7, 8],
            ["D05"] = [9, 0],
        };

        var result = _simulator.Simulate(devices);

        Assert.True(result.ActiveDeviceCount >= 1);
        Assert.True(result.ActiveDeviceCount <= 5);
        Assert.Equal(5, result.TotalDevices);
    }

    [Fact]
    public void Simulator_SignalValuesAreNonNegative()
    {
        var devices = new Dictionary<string, int[]>
        {
            ["D01"] = [1, 2, 3],
            ["D02"] = [4, 5, 6],
        };

        var result = _simulator.Simulate(devices);

        Assert.All(result.ReceivedSignal, v => Assert.True(v >= 0));
    }
}
```

```csharp
// RoundTripTests.cs
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
```

---

## Part 7: Reference Solver (reference/SignalDecoder.ReferenceSolver)

This is YOUR answer key. Never given to candidates.

```csharp
// ReferenceSolver.cs
namespace SignalDecoder.ReferenceSolver;

using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;
using System.Diagnostics;

public class ReferenceSignalDecoderService : ISignalDecoderService
{
    public DecodeResponse Decode(DecodeRequest request)
    {
        var sw = Stopwatch.StartNew();

        var deviceList = request.Devices.ToList();
        var signalLength = request.ReceivedSignal.Length;
        var tolerance = request.Tolerance;
        var received = request.ReceivedSignal;
        var solutions = new List<DecodeResult>();

        // Pre-filter: remove devices that can't possibly be in any solution
        var feasibleDevices = deviceList
            .Where(d => !d.Value.Select((v, i) => v).Where((v, i) => v > received[i] + tolerance).Any())
            .ToList();

        // Sort by max value descending — triggers pruning earlier
        feasibleDevices.Sort((a, b) =>
            b.Value.Max().CompareTo(a.Value.Max()));

        var currentSum = new int[signalLength];
        var selected = new List<KeyValuePair<string, int[]>>();

        Backtrack(feasibleDevices, received, tolerance, 0, currentSum, selected, solutions, signalLength);

        sw.Stop();

        return new DecodeResponse
        {
            Solutions = solutions,
            SolutionCount = solutions.Count,
            SolveTimeMs = sw.ElapsedMilliseconds
        };
    }

    private void Backtrack(
        List<KeyValuePair<string, int[]>> devices,
        int[] received,
        int tolerance,
        int index,
        int[] currentSum,
        List<KeyValuePair<string, int[]>> selected,
        List<DecodeResult> solutions,
        int signalLength)
    {
        // Prune: if current sum exceeds received + tolerance at any position, stop
        for (int p = 0; p < signalLength; p++)
        {
            if (currentSum[p] > received[p] + tolerance)
                return;
        }

        // Base case: checked all devices
        if (index == devices.Count)
        {
            // Check if current sum matches within tolerance at every position
            bool matches = true;
            for (int p = 0; p < signalLength; p++)
            {
                if (Math.Abs(currentSum[p] - received[p]) > tolerance)
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                var computedSum = (int[])currentSum.Clone();
                var result = new DecodeResult
                {
                    TransmittingDevices = selected.Select(s => s.Key).ToList(),
                    DecodedSignals = selected.ToDictionary(s => s.Key, s => s.Value),
                    ComputedSum = computedSum,
                    MatchesReceived = tolerance == 0
                        ? computedSum.SequenceEqual(received)
                        : computedSum.Select((v, i) => Math.Abs(v - received[i]) <= tolerance).All(x => x)
                };
                solutions.Add(result);
            }
            return;
        }

        // Remaining sum check: can we still reach the target?
        // Calculate max possible sum if we include all remaining devices
        bool canReachMin = true;
        for (int p = 0; p < signalLength; p++)
        {
            int maxPossible = currentSum[p];
            for (int d = index; d < devices.Count; d++)
                maxPossible += devices[d].Value[p];

            if (maxPossible < received[p] - tolerance)
            {
                canReachMin = false;
                break;
            }
        }
        if (!canReachMin) return;

        // Branch 1: Include this device
        var device = devices[index];
        for (int p = 0; p < signalLength; p++)
            currentSum[p] += device.Value[p];
        selected.Add(device);

        Backtrack(devices, received, tolerance, index + 1, currentSum, selected, solutions, signalLength);

        selected.RemoveAt(selected.Count - 1);
        for (int p = 0; p < signalLength; p++)
            currentSum[p] -= device.Value[p];

        // Branch 2: Exclude this device
        Backtrack(devices, received, tolerance, index + 1, currentSum, selected, solutions, signalLength);
    }
}
```

---

## Part 8: GitHub Action (template/.github/workflows/assessment.yml)

```yaml
name: Assessment Evaluation

on: [push]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    timeout-minutes: 10

    steps:
      - name: Checkout solution
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run sample tests
        run: dotnet test tests/SignalDecoder.SampleTests --no-build --logger "console;verbosity=normal"

      - name: Run assessment tests
        if: always()
        run: |
          if [ -d "tests/SignalDecoder.AssessmentTests" ]; then
            dotnet test tests/SignalDecoder.AssessmentTests --no-build \
              --logger "console;verbosity=normal" \
              --logger "trx;LogFileName=results.trx" \
              --results-directory TestResults
          else
            echo "Assessment tests not found — only sample tests were run"
          fi

      - name: Generate results summary
        if: always()
        run: |
          echo "## 📊 Assessment Results" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY

          if [ ! -f "TestResults/results.trx" ]; then
            echo "⚠️ No assessment test results found" >> $GITHUB_STEP_SUMMARY
            exit 0
          fi

          python3 << 'PYEOF' >> $GITHUB_STEP_SUMMARY
          import xml.etree.ElementTree as ET
          import os

          trx_path = "TestResults/results.trx"
          if not os.path.exists(trx_path):
              print("No test results file found")
              exit(0)

          tree = ET.parse(trx_path)
          root = tree.getroot()
          ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

          counters = root.find('.//t:Counters', ns)
          if counters is None:
              print("Could not parse test results")
              exit(0)

          total = int(counters.get('total', 0))
          passed = int(counters.get('passed', 0))
          failed = int(counters.get('failed', 0))

          pct = round(passed / total * 100) if total > 0 else 0

          print(f"### Score: {passed}/{total} ({pct}%)")
          print("")

          if failed == 0:
              print("✅ **All tests passed!**")
          else:
              print(f"❌ **{failed} test(s) failed**")

          print("")
          print("| Test | Result | Duration |")
          print("|------|--------|----------|")

          results = root.find('.//t:Results', ns)
          if results is not None:
              for r in results.findall('t:UnitTestResult', ns):
                  name = r.get('testName', 'Unknown')
                  # Show only the method name
                  short_name = name.split('.')[-1] if '.' in name else name
                  outcome = r.get('outcome', 'Unknown')
                  duration = r.get('duration', '0')

                  icon = "✅" if outcome == "Passed" else "❌" if outcome == "Failed" else "⏭️"
                  print(f"| {short_name} | {icon} {outcome} | {duration} |")

          # Category breakdown
          print("")
          print("### Category Breakdown")
          print("")

          categories = {}
          if results is not None:
              for r in results.findall('t:UnitTestResult', ns):
                  name = r.get('testName', '')
                  outcome = r.get('outcome', '')
                  # Extract class name as category
                  parts = name.split('.')
                  cat = parts[-2] if len(parts) >= 2 else "Other"
                  if cat not in categories:
                      categories[cat] = {"passed": 0, "failed": 0, "total": 0}
                  categories[cat]["total"] += 1
                  if outcome == "Passed":
                      categories[cat]["passed"] += 1
                  else:
                      categories[cat]["failed"] += 1

          for cat, counts in categories.items():
              icon = "✅" if counts["failed"] == 0 else "❌"
              print(f"- {icon} **{cat}**: {counts['passed']}/{counts['total']}")

          PYEOF

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: TestResults/
```

---

## Part 9: Scripts

### build-hidden-tests.sh
```bash
#!/bin/bash
# Compiles the hidden tests and copies the DLL into the template
set -e

echo "Building hidden tests..."
cd hidden-tests/SignalDecoder.AssessmentTests
dotnet build -c Release

echo "Copying to template..."
mkdir -p ../../template/tests/SignalDecoder.AssessmentTests
cp -r . ../../template/tests/SignalDecoder.AssessmentTests/

echo "Adding to solution..."
cd ../../template
dotnet sln add tests/SignalDecoder.AssessmentTests/SignalDecoder.AssessmentTests.csproj 2>/dev/null || true

echo "Done. Hidden tests injected into template."
```

### inject-tests.sh
```bash
#!/bin/bash
# Injects hidden tests into a candidate's cloned repo
# Usage: ./inject-tests.sh /path/to/candidate/repo
set -e

CANDIDATE_REPO=$1

if [ -z "$CANDIDATE_REPO" ]; then
    echo "Usage: ./inject-tests.sh /path/to/candidate/repo"
    exit 1
fi

echo "Injecting hidden tests into $CANDIDATE_REPO..."
cp -r hidden-tests/SignalDecoder.AssessmentTests "$CANDIDATE_REPO/tests/"

cd "$CANDIDATE_REPO"
dotnet sln add tests/SignalDecoder.AssessmentTests/SignalDecoder.AssessmentTests.csproj 2>/dev/null || true

echo "Done. Run 'dotnet test' in the candidate repo to evaluate."
```

### evaluate-candidate.sh
```bash
#!/bin/bash
# Full evaluation pipeline for a candidate submission
# Usage: ./evaluate-candidate.sh <repo-url> [candidate-name]
set -e

REPO_URL=$1
CANDIDATE=${2:-"candidate"}
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
WORK_DIR="/tmp/eval-${CANDIDATE}-${TIMESTAMP}"
SCRIPT_DIR="$(cd "$(dirname "$0")/.." && pwd)"

echo "========================================="
echo "Evaluating: $CANDIDATE"
echo "Repo: $REPO_URL"
echo "========================================="

# Clone
echo "Cloning repository..."
git clone --depth 1 "$REPO_URL" "$WORK_DIR"

# Inject hidden tests
echo "Injecting assessment tests..."
cp -r "$SCRIPT_DIR/hidden-tests/SignalDecoder.AssessmentTests" "$WORK_DIR/tests/"
cd "$WORK_DIR"
dotnet sln add tests/SignalDecoder.AssessmentTests/SignalDecoder.AssessmentTests.csproj 2>/dev/null || true

# Build
echo "Building..."
if ! dotnet build 2>&1; then
    echo "❌ BUILD FAILED"
    exit 1
fi

# Run tests
echo "Running assessment tests..."
dotnet test tests/SignalDecoder.AssessmentTests \
    --logger "console;verbosity=normal" \
    --logger "trx;LogFileName=results.trx" \
    --results-directory TestResults \
    2>&1

echo ""
echo "========================================="
echo "Results for: $CANDIDATE"
echo "========================================="

# Parse results
python3 << PYEOF
import xml.etree.ElementTree as ET
import os

trx_path = "TestResults/results.trx"
if not os.path.exists(trx_path):
    print("No results file found")
    exit(1)

tree = ET.parse(trx_path)
root = tree.getroot()
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

counters = root.find('.//t:Counters', ns)
total = int(counters.get('total', 0))
passed = int(counters.get('passed', 0))
failed = int(counters.get('failed', 0))

print(f"Score: {passed}/{total} ({round(passed/total*100) if total else 0}%)")
print()

results = root.find('.//t:Results', ns)
for r in results.findall('t:UnitTestResult', ns):
    name = r.get('testName', '').split('.')[-1]
    outcome = r.get('outcome')
    icon = "✅" if outcome == "Passed" else "❌"
    duration = r.get('duration', '0')
    print(f"  {icon} {name} ({duration})")

    if outcome == "Failed":
        msg = r.find('.//t:Message', ns)
        if msg is not None and msg.text:
            print(f"     Error: {msg.text[:200]}")
PYEOF

# Cleanup
rm -rf "$WORK_DIR"
```

---

## Part 10: Template README.md

```markdown
# Signal Decoder — Technical Assessment

## Background

A communications tower receives signals from nearby devices. Each device has a unique signal pattern — a fixed sequence of numbers. When multiple devices transmit simultaneously, the tower receives the combined signal (sum of each pattern, position by position).

Your task: build an API that determines which devices were transmitting based on the combined signal.

## Getting Started

### Prerequisites

- .NET 8 SDK

### Build & Run

\```bash
dotnet build
dotnet run --project src/SignalDecoder.Api
\```

### Run Tests

\```bash
dotnet test
\```

### Swagger

Once running, visit: `https://localhost:5001/swagger`

## API Endpoints

### Provided (already implemented)

- `GET /api/devices/generate?count=5&signalLength=4&maxStrength=9` — Generate random devices
- `POST /api/signal/simulate` — Simulate a transmission (returns combined signal)

### Your Task

- `POST /api/signal/decode` — Decode a received signal to find which devices transmitted

See the assessment document for full details on the decode endpoint requirements.

## What to Implement

1. Open `src/SignalDecoder.Application/Services/` — implement the three service classes
2. Register your implementations in `src/SignalDecoder.Domain/DependencyInjection.cs`
3. Run `dotnet test` to verify your solution
4. Push to trigger the GitHub Action for full evaluation

## Project Structure

\```
src/
├── SignalDecoder.Api/           # Web API host (controllers provided)
├── SignalDecoder.Domain/        # Models & interfaces (do not modify)
└── SignalDecoder.Application/   # YOUR CODE GOES HERE
tests/
└── SignalDecoder.SampleTests/   # Basic tests to get you started
\```
```

---

## Build Order

Execute in this order:
1. Create the solution and all projects using `dotnet new`
2. Add project references (Api → Application → Domain, Tests → all)
3. Add NuGet packages (xunit, Microsoft.NET.Test.Sdk, xunit.runner.visualstudio, Microsoft.AspNetCore.Mvc.Testing for integration tests)
4. Create all source files
5. Run `dotnet build` — must succeed with zero errors
6. Run `dotnet test` on sample tests — they will fail (NotImplementedException) and that's expected
7. Create the hidden test project separately, verify it compiles
8. Create the reference solver, verify it compiles
9. Create all scripts, make them executable with `chmod +x`

## Important Notes

- The Domain project's `AddSignalDecoderServices()` method is the bridge between the candidate's code and the hidden tests. The candidate MUST register their implementations there.
- The hidden tests instantiate services directly via `new SignalDecoderService()` AND via DI — test both paths.
- All projects must target `net8.0`.
- The `.github/workflows/assessment.yml` generates a nice summary in the GitHub Actions tab that the candidate can see.
- The reference solver uses backtracking with pruning, pre-filtering, and remaining-sum checks. It should handle 20 devices in under 5 seconds.
- VERIFY that `dotnet build` succeeds and `dotnet test` runs (even if tests fail due to NotImplementedException) before considering the task complete.
