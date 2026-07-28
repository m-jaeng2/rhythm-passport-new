using RhythmPassport.Core.GameFlow;

namespace RhythmPassport.Game.Session
{
    public sealed class GameSessionController
    {
        private readonly GameSessionConfig sessionConfig;

        public GameSessionController(GameSessionConfig sessionConfig)
        {
            this.sessionConfig = sessionConfig;
        }

        public GameSessionState CurrentState { get; private set; } = GameSessionState.Idle;

        public int Score { get; private set; }

        public int Combo { get; private set; }

        public int SuccessCount { get; private set; }

        public int FailureCount { get; private set; }

        public float ElapsedSeconds { get; private set; }

        public void BeginSession()
        {
            Score = 0;
            Combo = 0;
            SuccessCount = 0;
            FailureCount = 0;
            ElapsedSeconds = 0f;
            CurrentState = GameSessionState.Playing;
        }

        public void PauseSession()
        {
            if (CurrentState == GameSessionState.Playing)
            {
                CurrentState = GameSessionState.Paused;
            }
        }

        public void ResumeSession()
        {
            if (CurrentState == GameSessionState.Paused)
            {
                CurrentState = GameSessionState.Playing;
            }
        }

        public void CompleteSession()
        {
            CurrentState = GameSessionState.Completed;
        }

        public void AbortSession()
        {
            CurrentState = GameSessionState.Aborted;
        }

        public void RegisterSuccess()
        {
            SuccessCount += 1;
            Combo += 1;

            var baseScore = sessionConfig != null ? sessionConfig.scorePerSuccess : 100;
            var comboStep = sessionConfig != null ? sessionConfig.comboBonusStep : 25;

            Score += baseScore + ((Combo - 1) * comboStep);
        }

        public void RegisterFailure()
        {
            FailureCount += 1;
            Combo = 0;
        }

        public void Tick(float deltaTime)
        {
            if (CurrentState == GameSessionState.Playing)
            {
                ElapsedSeconds += deltaTime;
            }
        }
    }
}
