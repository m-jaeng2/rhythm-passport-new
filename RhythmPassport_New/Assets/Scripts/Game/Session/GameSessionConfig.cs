using UnityEngine;

namespace RhythmPassport.Game.Session
{
    [CreateAssetMenu(
        fileName = "GameSessionConfig",
        menuName = "RhythmPassport/Session Config")]
    public sealed class GameSessionConfig : ScriptableObject
    {
        [Min(10f)]
        public float sessionDurationSeconds = 60f;

        [Min(1f)]
        public float actionWindowSeconds = 3f;

        [Min(0f)]
        public float inputCooldownSeconds = 0.3f;

        [Min(0)]
        public int scorePerSuccess = 100;

        [Min(0)]
        public int comboBonusStep = 25;
    }
}
