using System.Collections.Generic;

public interface INetworkFrameProducer
{
    List<NetStateFrame> GetAndClearFrames();
}