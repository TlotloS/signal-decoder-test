# Signal Decoder API

A comprehensive ASP.NET Core 8 Web API for device signal generation, simulation, and decoding. This API provides endpoints to generate random signal patterns, simulate signal transmissions, and decode received signals to identify which devices transmitted them.

## Overview

The Signal Decoder API helps solve the problem of identifying which devices are transmitting in a system where multiple devices can send signals simultaneously. The combined signal is the sum of all active device signals, and the decoder identifies all possible device combinations that could have produced the received signal.

## Project Structure

This solution follows clean architecture principles:

```
SignalDecoder/
├── src/
│   ├── SignalDecoder.Api/              # ASP.NET Core Web API
│   ├── SignalDecoder.Domain/           # Domain models and interfaces
│   ├── SignalDecoder.Application/      # Business logic and services
│   └── SignalDecoder.Infrastructure/   # Infrastructure (reserved for future use)
├── tests/
│   ├── SignalDecoder.UnitTests/        # xUnit unit tests
│   └── SignalDecoder.IntegrationTests/ # API integration tests
└── SignalDocoderAssessment.sln
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/SignalDecoder.Api
```

The API will start on `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP).

### Test

Run all tests (unit + integration):

```bash
dotnet test
```

Run only unit tests:

```bash
dotnet test tests/SignalDecoder.UnitTests
```

Run only integration tests:

```bash
dotnet test tests/SignalDecoder.IntegrationTests
```

## API Documentation

Once the application is running, visit the Swagger UI for interactive API documentation:

**Swagger URL:** `https://localhost:5001/swagger`

## API Endpoints

### Device Generation

**`GET /api/devices/generate`**

Generates a random set of devices with signal patterns.

**Query Parameters:**
- `count` (optional, default: 5) - Number of devices to generate (1-100)
- `signalLength` (optional, default: 4) - Length of each signal pattern (1-20)
- `maxStrength` (optional, default: 9) - Maximum signal strength value (1-100)

**Example:**
```
GET /api/devices/generate?count=5&signalLength=4&maxStrength=9
```

**Response:**
```json
{
  "D01": [3, 7, 2, 5],
  "D02": [1, 4, 8, 2],
  "D03": [6, 0, 3, 9],
  "D04": [2, 5, 1, 4],
  "D05": [9, 3, 7, 1]
}
```

### Signal Simulation

**`POST /api/signal/simulate`**

Simulates a signal transmission by randomly selecting active devices and computing their combined signal.

**Request Body:**
```json
{
  "devices": {
    "D01": [1, 2, 3],
    "D02": [4, 5, 6],
    "D03": [7, 8, 9]
  }
}
```

**Response:**
```json
{
  "receivedSignal": [12, 15, 18],
  "activeDeviceCount": 2,
  "signalLength": 3,
  "totalDevices": 3
}
```

### Signal Decoding

**`POST /api/signal/decode`**

Decodes a received signal to identify which device combinations could have transmitted it.

**Request Body:**
```json
{
  "devices": {
    "D01": [1, 2],
    "D02": [3, 4],
    "D03": [0, 1]
  },
  "receivedSignal": [4, 6],
  "tolerance": 0
}
```

**Response:**
```json
{
  "solutions": [
    {
      "transmittingDevices": ["D01", "D02"],
      "decodedSignals": {
        "D01": [1, 2],
        "D02": [3, 4]
      },
      "computedSum": [4, 6],
      "matchesReceived": true
    }
  ],
  "solutionCount": 1,
  "solveTimeMs": 2
}
```

**Parameters:**
- `devices` - Dictionary of device IDs and their signal patterns
- `receivedSignal` - The combined signal received
- `tolerance` (optional, default: 0) - Allowed difference at each position for fuzzy matching

## Features

- **Device Generation**: Create random device signal patterns for testing
- **Signal Simulation**: Randomly select active devices and compute combined signals
- **Signal Decoding**: Identify all possible device combinations that match a received signal
- **Tolerance Support**: Allow fuzzy matching with configurable tolerance
- **Performance Optimized**: Handles up to 25 devices efficiently (< 5 seconds)
- **Comprehensive Validation**: Clear error messages for invalid inputs
- **Full Test Coverage**: 30+ unit and integration tests

## Architecture Highlights

- **Clean Architecture**: Clear separation of concerns with Domain, Application, and API layers
- **Dependency Injection**: All services registered via DI container
- **CORS Enabled**: Configured for local development
- **Swagger/OpenAPI**: Interactive API documentation
- **Exception Handling**: Global exception handler middleware
- **JSON Serialization**: camelCase naming convention

## Performance

The decoder is optimized to handle:
- 15 devices: < 1 second
- 25 devices: < 5 seconds

The implementation uses backtracking with intelligent pruning to efficiently explore the solution space.

## License

This project is for educational and assessment purposes.
