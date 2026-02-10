# Signal Decoder API — Claude Code Build Script

## Overview

Build a complete ASP.NET Core 8 Web API solution for a "Signal Decoder" technical assessment. This API has two parts:

1. **Provided endpoints** (given to candidates to test against)
2. **Reference decoder implementation** (the answer key — kept separate so it can be excluded from the candidate repo)

## Project Structure

Create a clean architecture solution called `SignalDecoder` with these projects:

```
SignalDecoder/
├── src/
│   ├── SignalDecoder.Api/              # ASP.NET Core Web API host
│   ├── SignalDecoder.Domain/           # Domain models, interfaces
│   ├── SignalDecoder.Application/      # Business logic, services
│   └── SignalDecoder.Infrastructure/   # External concerns (if needed later)
├── tests/
│   ├── SignalDecoder.UnitTests/        # xUnit tests for solver logic
│   └── SignalDecoder.IntegrationTests/ # API endpoint tests
├── SignalDecoder.sln
└── README.md
```

## Domain Models (SignalDecoder.Domain)

```csharp
// Models/Device.cs
public record Device(string Id, int[] SignalPattern);

// Models/DecodeRequest.cs
public class DecodeRequest
{
    public Dictionary<string, int[]> Devices { get; set; }
    public int[] ReceivedSignal { get; set; }
    public int Tolerance { get; set; } = 0;
}

// Models/DecodeResult.cs
public class DecodeResult
{
    public List<string> TransmittingDevices { get; set; }
    public Dictionary<string, int[]> DecodedSignals { get; set; }
    public int[] ComputedSum { get; set; }
    public bool MatchesReceived { get; set; }
}

// Models/DecodeResponse.cs
public class DecodeResponse
{
    public List<DecodeResult> Solutions { get; set; }
    public int SolutionCount { get; set; }
    public long SolveTimeMs { get; set; }
}

// Models/GenerateRequest.cs
public class GenerateDevicesRequest
{
    public int Count { get; set; } = 5;
    public int SignalLength { get; set; } = 4;
    public int MaxSignalStrength { get; set; } = 9;
}

// Models/SimulateRequest.cs
public class SimulateRequest
{
    public Dictionary<string, int[]> Devices { get; set; }
}

// Models/SimulateResponse.cs
public class SimulateResponse
{
    public int[] ReceivedSignal { get; set; }
    public int ActiveDeviceCount { get; set; }
    public int SignalLength { get; set; }
    public int TotalDevices { get; set; }
}

// Interfaces/ISignalDecoderService.cs
public interface ISignalDecoderService
{
    DecodeResponse Decode(DecodeRequest request);
}

// Interfaces/IDeviceGeneratorService.cs
public interface IDeviceGeneratorService
{
    Dictionary<string, int[]> GenerateDevices(int count, int signalLength, int maxStrength);
}

// Interfaces/ISignalSimulatorService.cs
public interface ISignalSimulatorService
{
    SimulateResponse Simulate(Dictionary<string, int[]> devices);
}
```

## Application Layer (SignalDecoder.Application)

### DeviceGeneratorService

- Generates N devices with random signal patterns
- Device IDs are "D01", "D02", etc.
- Each pattern is `signalLength` integers between 0 and `maxSignalStrength`
- Use a seeded Random for reproducibility (optional seed parameter)

### SignalSimulatorService

- Takes a set of devices
- Randomly picks a subset (between 1 and N devices) to be "active"
- Computes the sum of active device patterns position by position
- Returns the combined signal, active count, but NOT which devices are active

### SignalDecoderService (THE SOLVER — this is the answer key)

Implement the solver using **backtracking with pruning**. This is the reference implementation.

**Algorithm:**

```
function decode(devices[], receivedSignal[], tolerance):
    results = []
    backtrack(devices, receivedSignal, tolerance, index=0, currentSum=zeros, selected=[], results)
    return results

function backtrack(devices, received, tolerance, index, currentSum, selected, results):
    // Check if current sum already exceeds received + tolerance at any position
    for each position p:
        if currentSum[p] > received[p] + tolerance:
            return  // PRUNE — no point continuing

    if index == devices.length:
        // Check if currentSum matches received within tolerance at every position
        if all positions p: |currentSum[p] - received[p]| <= tolerance:
            results.add(copy of selected)
        return

    // Branch 1: INCLUDE device at index
    newSum = currentSum + devices[index].pattern  // position-wise addition
    selected.add(devices[index])
    backtrack(devices, received, tolerance, index + 1, newSum, selected, results)
    selected.removeLast()

    // Branch 2: EXCLUDE device at index
    backtrack(devices, received, tolerance, index + 1, currentSum, selected, results)
```

**Key optimisations to implement:**

1. **Early termination on overshoot**: If `currentSum[p] > received[p] + tolerance` at ANY position, prune immediately. Since all values are non-negative, adding more devices can only increase the sum.

2. **Pre-sort devices**: Sort devices by their maximum signal value descending. This makes the pruning trigger earlier because large values overshoot faster.

3. **Pre-filter impossible devices**: Before searching, eliminate any device that has a value at any position exceeding `received[p] + tolerance` at that position. That device can never be part of a valid solution.

4. **Remaining sum check**: At each step, calculate the minimum possible remaining contribution needed. If even including all remaining devices can't reach the target minus tolerance, prune.

**Wrap the solve in a Stopwatch to measure and return `solveTimeMs`.**

### Validation (create a ValidationService or use FluentValidation)

Validate all requests:
- `DecodeRequest`: devices dict not empty, all patterns same length, receivedSignal same length as patterns, tolerance >= 0, all signal values >= 0
- `GenerateDevicesRequest`: count between 1 and 100, signalLength between 1 and 20, maxSignalStrength between 1 and 100
- `SimulateRequest`: devices dict not empty, all patterns same length

Return 400 with clear error messages for validation failures.

## API Layer (SignalDecoder.Api)

### Controllers

**DevicesController** — `[Route("api/devices")]`
- `GET /api/devices/generate?count={n}&signalLength={len}&maxStrength={max}`
  - Query params with defaults: count=5, signalLength=4, maxStrength=9
  - Returns the generated devices dictionary

**SignalController** — `[Route("api/signal")]`
- `POST /api/signal/simulate`
  - Body: SimulateRequest
  - Returns: SimulateResponse

- `POST /api/signal/decode`
  - Body: DecodeRequest
  - Returns: DecodeResponse

### Configuration

- Enable Swagger/OpenAPI with XML comments
- Add CORS (allow all origins for local dev)
- Register services via DI
- Use `System.Text.Json` with camelCase naming
- Add global exception handler middleware

## Unit Tests (SignalDecoder.UnitTests)

Use xUnit. Test the decoder service thoroughly:

```csharp
// Test cases to implement:

[Fact] SimpleCase_TwoDevicesFromFive_FindsCorrectPair()
// 5 devices, signal length 4, tolerance 0
// Pre-compute which 2 devices sum to the target
// Assert the solver finds exactly that pair

[Fact] NoSolution_ImpossibleSignal_ReturnsEmpty()
// Give a received signal that no subset can produce
// Assert empty results

[Fact] MultipleSolutions_ShortSignal_ReturnsAll()
// Use signal length 2 with values designed so multiple subsets match
// Assert all solutions are returned

[Fact] SingleDevice_ExactMatch_Works()
// One device whose pattern equals the received signal
// Assert it's found

[Fact] AllDevices_SumOfAll_Works()
// Received signal is sum of ALL devices
// Assert all devices returned

[Fact] NoDevicesActive_ZeroSignal_ReturnsEmpty()
// Received signal is all zeros, tolerance 0
// Assert empty selected set is returned (or handle as edge case)

[Fact] WithTolerance_FuzzyMatch_FindsSolution()
// Set up devices where exact match fails but tolerance=1 finds it
// Assert the fuzzy match works

[Fact] WithTolerance_StillPrunes_DoesntReturnEverything()
// Verify that tolerance doesn't cause ALL subsets to match
// A subset that's way off should still be rejected

[Fact] LargerInput_15Devices_CompletesUnderOneSecond()
// 15 devices, signal length 5
// Assert solve completes in < 1000ms

[Fact] Validation_MismatchedLengths_Throws()
// Devices with different pattern lengths
// Assert appropriate exception/validation error

[Fact] Validation_NegativeValues_Throws()
// Negative signal values
// Assert rejection

[Fact] Validation_EmptyDevices_Throws()
// Empty device dictionary
// Assert rejection
```

### Test the generator and simulator too:

```csharp
[Fact] GenerateDevices_ReturnsCorrectCount()
[Fact] GenerateDevices_PatternsHaveCorrectLength()
[Fact] GenerateDevices_ValuesWithinRange()
[Fact] Simulate_ReturnsSameLengthSignal()
[Fact] Simulate_ActiveCountWithinRange()
```

## Integration Tests (SignalDecoder.IntegrationTests)

Use `WebApplicationFactory<Program>` to test the actual HTTP endpoints:

```csharp
[Fact] GenerateEndpoint_Returns200_WithValidDevices()
[Fact] SimulateEndpoint_Returns200_WithCombinedSignal()
[Fact] DecodeEndpoint_Returns200_WithCorrectSolution()
[Fact] DecodeEndpoint_Returns400_OnInvalidInput()
[Fact] FullWorkflow_Generate_Simulate_Decode_RoundTrips()
// Generate devices, simulate a transmission, decode the result,
// verify the decoded devices actually sum to the received signal
```

## README.md

Include:
- Project description
- Prerequisites (.NET 8 SDK)
- How to build: `dotnet build`
- How to run: `dotnet run --project src/SignalDecoder.Api`
- How to test: `dotnet test`
- Swagger URL: `https://localhost:5001/swagger`
- Brief explanation of the API endpoints
- Do NOT include algorithm explanation (that's the candidate's job to figure out)

## Important Notes

- Use `dotnet new sln`, `dotnet new webapi`, `dotnet new classlib`, `dotnet new xunit` to scaffold
- Add project references: Api references Application, Application references Domain, Tests reference all
- Make sure `dotnet build` and `dotnet test` pass with zero errors before finishing
- The solver should handle up to 25 devices comfortably (under 5 seconds)
- Use `System.Diagnostics.Stopwatch` for timing
- Keep the code clean and well-commented — this is a reference implementation
