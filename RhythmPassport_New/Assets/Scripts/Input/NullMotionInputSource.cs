using System;

namespace RhythmPassport.Input
{
    public sealed class NullMotionInputSource : IMotionInputSource
    {
        public static NullMotionInputSource Instance { get; } = new NullMotionInputSource();

        public string SourceName => "None";

        public string StatusText => "입력 소스를 준비 중입니다.";

        public bool IsReady => false;

        public event Action<MotionInputFrame> MotionDetected
        {
            add { }
            remove { }
        }

        private NullMotionInputSource()
        {
        }
    }
}
