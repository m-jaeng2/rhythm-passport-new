using System;

namespace RhythmPassport.Input
{
    [Serializable]
    public sealed class WebSocketMotionMessage
    {
        public string type;
        public string motion;
        public float confidence = 1f;
        public double timestamp;
        public string status;
    }
}
