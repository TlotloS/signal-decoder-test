using System.Diagnostics;
using SignalDecoder.Domain.Interfaces;
using SignalDecoder.Domain.Models;

namespace SignalDecoder.Application.Services;

public class SignalDecoderService : ISignalDecoderService
{
    public DecodeResponse Decode(DecodeRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        // Validate input
        ValidateRequest(request);

        // Pre-filter: Remove devices that have any value exceeding received[p] + tolerance
        var validDevices = request.Devices
            .Where(kvp => IsDeviceValid(kvp.Value, request.ReceivedSignal, request.Tolerance))
            .ToList();

        // Pre-sort: Sort devices by max signal value descending for better pruning
        var sortedDevices = validDevices
            .OrderByDescending(kvp => kvp.Value.Max())
            .Select(kvp => new Device(kvp.Key, kvp.Value))
            .ToArray();

        // Initialize backtracking
        var results = new List<DecodeResult>();
        var currentSum = new int[request.ReceivedSignal.Length];
        var selected = new List<string>();

        // Run backtracking algorithm
        Backtrack(sortedDevices, request.ReceivedSignal, request.Tolerance,
                  0, currentSum, selected, results);

        stopwatch.Stop();

        return new DecodeResponse
        {
            Solutions = results,
            SolutionCount = results.Count,
            SolveTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    private void Backtrack(Device[] devices, int[] received, int tolerance,
                           int index, int[] currentSum, List<string> selected,
                           List<DecodeResult> results)
    {
        // PRUNE 1: Upper bound - Check if current sum overshoots at any position
        for (int i = 0; i < currentSum.Length; i++)
        {
            if (currentSum[i] > received[i] + tolerance)
            {
                return; // No point continuing - can only get worse
            }
        }

        // PRUNE 2: Lower bound - Check if even adding all remaining devices can reach target
        if (index < devices.Length)
        {
            var maxPossible = new int[currentSum.Length];
            Array.Copy(currentSum, maxPossible, currentSum.Length);

            // Add all remaining devices' signals
            for (int deviceIdx = index; deviceIdx < devices.Length; deviceIdx++)
            {
                for (int i = 0; i < maxPossible.Length; i++)
                {
                    maxPossible[i] += devices[deviceIdx].SignalPattern[i];
                }
            }

            // If even with all remaining devices we can't reach target - tolerance, prune
            for (int i = 0; i < maxPossible.Length; i++)
            {
                if (maxPossible[i] < received[i] - tolerance)
                {
                    return; // Can't possibly reach the target
                }
            }
        }

        // BASE CASE: All devices considered
        if (index == devices.Length)
        {
            // Check if current sum matches received within tolerance
            if (IsMatchWithinTolerance(currentSum, received, tolerance))
            {
                var result = BuildDecodeResult(selected, currentSum, received, devices);
                results.Add(result);
            }
            return;
        }

        // BRANCH 1: INCLUDE current device
        var device = devices[index];
        var newSum = AddArrays(currentSum, device.SignalPattern);
        selected.Add(device.Id);
        Backtrack(devices, received, tolerance, index + 1, newSum, selected, results);
        selected.RemoveAt(selected.Count - 1);

        // BRANCH 2: EXCLUDE current device
        Backtrack(devices, received, tolerance, index + 1, currentSum, selected, results);
    }

    private bool IsMatchWithinTolerance(int[] currentSum, int[] received, int tolerance)
    {
        for (int i = 0; i < received.Length; i++)
        {
            if (Math.Abs(currentSum[i] - received[i]) > tolerance)
            {
                return false;
            }
        }
        return true;
    }

    private int[] AddArrays(int[] a, int[] b)
    {
        var result = new int[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
        return result;
    }

    private DecodeResult BuildDecodeResult(List<string> selectedIds, int[] computedSum,
                                           int[] received, Device[] devices)
    {
        var decodedSignals = new Dictionary<string, int[]>();
        foreach (var deviceId in selectedIds)
        {
            var device = devices.First(d => d.Id == deviceId);
            decodedSignals[deviceId] = device.SignalPattern;
        }

        return new DecodeResult
        {
            TransmittingDevices = new List<string>(selectedIds),
            DecodedSignals = decodedSignals,
            ComputedSum = computedSum,
            MatchesReceived = IsMatchWithinTolerance(computedSum, received, 0) // Exact match check
        };
    }

    private bool IsDeviceValid(int[] devicePattern, int[] received, int tolerance)
    {
        for (int i = 0; i < devicePattern.Length; i++)
        {
            if (devicePattern[i] > received[i] + tolerance)
            {
                return false;
            }
        }
        return true;
    }

    private void ValidateRequest(DecodeRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Devices == null || request.Devices.Count == 0)
            throw new ArgumentException("Devices dictionary cannot be null or empty.", nameof(request.Devices));

        if (request.ReceivedSignal == null || request.ReceivedSignal.Length == 0)
            throw new ArgumentException("ReceivedSignal cannot be null or empty.", nameof(request.ReceivedSignal));

        if (request.Tolerance < 0)
            throw new ArgumentException("Tolerance cannot be negative.", nameof(request.Tolerance));

        // Validate all patterns have same length as received signal
        var expectedLength = request.ReceivedSignal.Length;
        foreach (var device in request.Devices)
        {
            if (device.Value.Length != expectedLength)
                throw new ArgumentException($"All signal patterns must have the same length. Device {device.Key} has length {device.Value.Length}, expected {expectedLength}.");

            // Validate non-negative values
            foreach (var value in device.Value)
            {
                if (value < 0)
                    throw new ArgumentException($"Signal values cannot be negative. Device {device.Key} has negative value.");
            }
        }

        // Validate received signal non-negative
        foreach (var value in request.ReceivedSignal)
        {
            if (value < 0)
                throw new ArgumentException("Received signal values cannot be negative.");
        }
    }
}
