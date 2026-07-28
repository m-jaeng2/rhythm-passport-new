namespace RhythmPassport.Input
{
    public static class MotionActionMapper
    {
        public static MotionActionType FromExternalName(string motionName)
        {
            if (string.IsNullOrWhiteSpace(motionName))
            {
                return MotionActionType.None;
            }

            switch (motionName.Trim().ToLowerInvariant())
            {
                case "raise_both_hands":
                case "raisebothhands":
                case "hands_up":
                    return MotionActionType.RaiseBothHands;
                case "reach_left":
                case "left_reach":
                case "move_left":
                    return MotionActionType.ReachLeft;
                case "reach_right":
                case "right_reach":
                case "move_right":
                    return MotionActionType.ReachRight;
                default:
                    return MotionActionType.None;
            }
        }
    }
}
