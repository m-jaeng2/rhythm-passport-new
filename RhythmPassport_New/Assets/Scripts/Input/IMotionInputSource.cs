using System;

namespace RhythmPassport.Input
{
    public interface IMotionInputSource
    {
        string SourceName { get; }

        string StatusText { get; }

        bool IsReady { get; }

        event Action<MotionInputFrame> MotionDetected;
    }
}
