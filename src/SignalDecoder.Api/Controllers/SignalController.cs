using Microsoft.AspNetCore.Mvc;
using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

namespace SignalDecoder.Api.Controllers;

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
    /// Simulates a signal transmission by randomly selecting active devices
    /// </summary>
    /// <remarks>
    /// This endpoint simulates a real-world scenario where multiple devices can transmit simultaneously.
    ///
    /// **How it works:**
    /// 1. Provide a dictionary of devices with their signal patterns
    /// 2. The simulator randomly selects 1 to N devices to be "active"
    /// 3. It computes the combined signal by summing the patterns position by position
    /// 4. Returns the combined signal WITHOUT revealing which devices were active
    ///
    /// **Example Use Case:**
    /// You can use the generated devices from `/api/devices/generate` and pass them here
    /// to simulate a transmission, then use the result in `/api/signal/decode` to solve
    /// which devices were transmitting.
    ///
    /// **Signal Pattern Format:**
    /// - Each device has an array of integers (signal pattern)
    /// - All patterns must be the same length
    /// - Values are non-negative integers
    /// - The combined signal is the element-wise sum of active device patterns
    /// </remarks>
    /// <param name="request">Dictionary of devices with their signal patterns</param>
    /// <returns>Combined signal from randomly selected devices</returns>
    [HttpPost("simulate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<SimulateResponse> Simulate([FromBody] SimulateRequest request)
    {
        try
        {
            if (request?.Devices == null || request.Devices.Count == 0)
                return BadRequest("Devices dictionary cannot be null or empty.");

            // Validate all patterns have same length
            var firstLength = request.Devices.First().Value.Length;
            if (request.Devices.Any(d => d.Value.Length != firstLength))
                return BadRequest("All signal patterns must have the same length.");

            var response = _simulator.Simulate(request.Devices);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Decodes a received signal to identify which devices transmitted it
    /// </summary>
    /// <remarks>
    /// This endpoint solves the signal decoding problem: given a combined signal,
    /// find all possible combinations of devices that could have produced it.
    ///
    /// **How it works:**
    /// 1. Provide the dictionary of all possible devices
    /// 2. Provide the received (combined) signal
    /// 3. Optionally set a tolerance for fuzzy matching
    /// 4. The decoder finds ALL device combinations whose sum matches the received signal
    ///
    /// **Algorithm Features:**
    /// - Uses backtracking with intelligent pruning for efficiency
    /// - Returns ALL valid solutions (there may be multiple combinations)
    /// - Includes solve time in milliseconds for performance analysis
    /// - Handles up to 25 devices efficiently (under 5 seconds)
    ///
    /// **Tolerance Parameter:**
    /// - `tolerance = 0`: Exact match required (default)
    /// - `tolerance = 1`: Allows ±1 difference at each position
    /// - Higher tolerance finds more solutions but may include inexact matches
    ///
    /// **Example Workflow:**
    /// 1. Generate devices: `GET /api/devices/generate?count=5`
    /// 2. Simulate signal: `POST /api/signal/simulate` (with generated devices)
    /// 3. Decode signal: `POST /api/signal/decode` (with simulated signal)
    /// 4. Verify: One of the solutions should match the simulated transmission
    /// </remarks>
    /// <param name="request">Decode request with devices, received signal, and tolerance</param>
    /// <returns>All possible solutions showing which device combinations match the received signal</returns>
    [HttpPost("decode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DecodeResponse> Decode([FromBody] DecodeRequest request)
    {
        try
        {
            var response = _decoder.Decode(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
