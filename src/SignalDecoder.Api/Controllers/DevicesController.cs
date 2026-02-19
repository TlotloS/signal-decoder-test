using Microsoft.AspNetCore.Mvc;
using SignalDecoder.Domain.Interfaces;

namespace SignalDecoder.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceGeneratorService _deviceGenerator;

    public DevicesController(IDeviceGeneratorService deviceGenerator)
    {
        _deviceGenerator = deviceGenerator;
    }

    /// <summary>
    /// Generates a random set of devices with signal patterns
    /// </summary>
    /// <param name="count">Number of devices to generate (default: 5)</param>
    /// <param name="signalLength">Length of each signal pattern (default: 4)</param>
    /// <param name="maxStrength">Maximum signal strength value (default: 9)</param>
    /// <returns>Dictionary of device IDs and their signal patterns</returns>
    [HttpGet("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Dictionary<string, int[]>> Generate(
        [FromQuery] int count = 5,
        [FromQuery] int signalLength = 4,
        [FromQuery] int maxStrength = 9)
    {
    
        // Validation
        if (count < 1 || count > 100)
            return BadRequest("Count must be between 1 and 100.");

        if (signalLength < 1 || signalLength > 20)
            return BadRequest("Signal length must be between 1 and 20.");

        if (maxStrength < 1 || maxStrength > 100)
            return BadRequest("Max signal strength must be between 1 and 100.");

        var devices = _deviceGenerator.GenerateDevices(count, signalLength, maxStrength);
        return Ok(devices);
    }
}
