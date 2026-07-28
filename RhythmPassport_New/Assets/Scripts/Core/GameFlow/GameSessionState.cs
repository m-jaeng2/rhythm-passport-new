namespace RhythmPassport.Core.GameFlow
{
    public enum GameSessionState
    {
        Idle = 0,
        Calibrating = 1,
        Ready = 2,
        Playing = 3,
        Paused = 4,
        Completed = 5,
        Aborted = 6,
    }
}
