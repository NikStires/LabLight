using System;

public interface IVideoCamera
{
    FlipCode Flip { get; }
    PixelFormat Format { get; }
    IObservable<VideoFrame> GetFrames();
    IObservable<bool> Running { get; }
}