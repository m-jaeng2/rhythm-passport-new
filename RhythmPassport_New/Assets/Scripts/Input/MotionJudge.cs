namespace RhythmPassport.Input
{
    public sealed class MotionJudge
    {
        private readonly float acceptedInputCooldownSeconds;
        private double lastAcceptedTimestamp = -999d;

        public MotionJudge(float acceptedInputCooldownSeconds)
        {
            this.acceptedInputCooldownSeconds = acceptedInputCooldownSeconds;
        }

        public MotionJudgementResult Evaluate(MotionActionType expectedAction, MotionInputFrame frame)
        {
            if (frame.ActionType == MotionActionType.None)
            {
                return MotionJudgementResult.Ignored;
            }

            if ((frame.Timestamp - lastAcceptedTimestamp) < acceptedInputCooldownSeconds)
            {
                return MotionJudgementResult.CooldownBlocked;
            }

            if (frame.ActionType != expectedAction)
            {
                return MotionJudgementResult.WrongAction;
            }

            lastAcceptedTimestamp = frame.Timestamp;
            return MotionJudgementResult.Success;
        }
    }
}
