using SignalDecoder.Domain.Models;

namespace SignalDecoder.Domain.Interfaces;

public interface ISignalDecoderService
{
    DecodeResponse Decode(DecodeRequest request);
}
