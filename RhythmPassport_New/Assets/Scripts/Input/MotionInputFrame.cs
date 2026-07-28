namespace RhythmPassport.Input
{
    public readonly struct MotionInputFrame
    {
        public MotionInputFrame(MotionActionType actionType, float confidence, double timestamp)
        {
            ActionType = actionType;
            Confidence = confidence;
            Timestamp = timestamp;
        }

        public MotionActionType ActionType { get; }

        public float Confidence { get; }

        public double Timestamp { get; }
    }
}
