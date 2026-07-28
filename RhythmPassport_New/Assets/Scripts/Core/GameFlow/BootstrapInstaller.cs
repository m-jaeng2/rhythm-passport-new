using UnityEngine;

namespace RhythmPassport.Core.GameFlow
{
    public sealed class BootstrapInstaller : MonoBehaviour
    {
        private static bool s_initialized;

        private void Awake()
        {
            if (s_initialized)
            {
                Destroy(gameObject);
                return;
            }

            s_initialized = true;
            DontDestroyOnLoad(gameObject);
        }
    }
}
