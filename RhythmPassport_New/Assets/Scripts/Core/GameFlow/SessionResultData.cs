namespace RhythmPassport.Core.GameFlow
{
    public sealed class SessionResultData
    {
        public int Score { get; set; }

        public int SuccessCount { get; set; }

        public int FailureCount { get; set; }

        public bool WasAborted { get; set; }

        public float ElapsedSeconds { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
