using RhythmPassport.Game.Boot;
using RhythmPassport.Game.Calibration;
using RhythmPassport.Game.Gameplay;
using RhythmPassport.Game.Result;
using RhythmPassport.Game.Session;
using RhythmPassport.Game.Title;
using RhythmPassport.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmPassport.Core.GameFlow
{
    public sealed class AppRuntime : MonoBehaviour
    {
        private static AppRuntime instance;

        private GameObject activeSceneRoot;
        private DebugMotionInputSource debugMotionInputSource;
        private CameraMotionInputSource cameraMotionInputSource;
        private WebSocketMotionInputSource webSocketMotionInputSource;
        private UserCameraFeedService userCameraFeedService;
        private MotionInputSourceMode inputSourceMode = MotionInputSourceMode.CameraMotion;

        public static AppRuntime Instance => instance;

        public GameSessionConfig SessionConfig { get; private set; }

        public SessionResultData LastResult { get; } = new SessionResultData();

        public IMotionInputSource MotionInputSource
        {
            get
            {
                EnsureServices();

                if (inputSourceMode == MotionInputSourceMode.WebSocket)
                {
                    return webSocketMotionInputSource != null
                        ? webSocketMotionInputSource
                        : NullMotionInputSource.Instance;
                }

                if (inputSourceMode == MotionInputSourceMode.CameraMotion)
                {
                    return cameraMotionInputSource != null
                        ? cameraMotionInputSource
                        : NullMotionInputSource.Instance;
                }

                return debugMotionInputSource != null
                    ? debugMotionInputSource
                    : NullMotionInputSource.Instance;
            }
        }

        public MotionInputSourceMode InputSourceMode => inputSourceMode;

        public UserCameraFeedService UserCameraFeed => userCameraFeedService;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntime()
        {
            if (instance != null)
            {
                return;
            }

            var runtimeObject = new GameObject(nameof(AppRuntime));
            instance = runtimeObject.AddComponent<AppRuntime>();
            DontDestroyOnLoad(runtimeObject);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureServices();
            SessionConfig = CreateDefaultConfig();
            SceneManager.sceneLoaded += HandleSceneLoaded;
            AttachSceneController(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        public void SetLastResult(SessionResultData result)
        {
            LastResult.Score = result.Score;
            LastResult.SuccessCount = result.SuccessCount;
            LastResult.FailureCount = result.FailureCount;
            LastResult.WasAborted = result.WasAborted;
            LastResult.ElapsedSeconds = result.ElapsedSeconds;
            LastResult.Summary = result.Summary;
        }

        public void UseDebugInput()
        {
            EnsureServices();
            inputSourceMode = MotionInputSourceMode.Debug;
            if (debugMotionInputSource != null)
            {
                debugMotionInputSource.enabled = true;
            }

            if (webSocketMotionInputSource != null)
            {
                webSocketMotionInputSource.Disconnect();
                webSocketMotionInputSource.enabled = false;
            }

            if (cameraMotionInputSource != null)
            {
                cameraMotionInputSource.enabled = false;
            }
        }

        public void UseCameraMotionInput()
        {
            EnsureServices();
            inputSourceMode = MotionInputSourceMode.CameraMotion;

            if (debugMotionInputSource != null)
            {
                debugMotionInputSource.enabled = false;
            }

            if (webSocketMotionInputSource != null)
            {
                webSocketMotionInputSource.Disconnect();
                webSocketMotionInputSource.enabled = false;
            }

            if (cameraMotionInputSource != null)
            {
                cameraMotionInputSource.enabled = true;
            }
        }

        public void UseWebSocketInput(string url = null)
        {
            EnsureServices();
            inputSourceMode = MotionInputSourceMode.WebSocket;

            if (debugMotionInputSource != null)
            {
                debugMotionInputSource.enabled = false;
            }

            if (cameraMotionInputSource != null)
            {
                cameraMotionInputSource.enabled = false;
            }

            if (webSocketMotionInputSource != null)
            {
                webSocketMotionInputSource.enabled = true;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    webSocketMotionInputSource.SetEndpoint(url);
                }

                webSocketMotionInputSource.Connect();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachSceneController(scene);
        }

        private void EnsureServices()
        {
            if (debugMotionInputSource == null)
            {
                debugMotionInputSource = gameObject.GetComponent<DebugMotionInputSource>() ?? gameObject.AddComponent<DebugMotionInputSource>();
                debugMotionInputSource.enabled = inputSourceMode == MotionInputSourceMode.Debug;
            }

            if (cameraMotionInputSource == null)
            {
                cameraMotionInputSource = gameObject.GetComponent<CameraMotionInputSource>() ?? gameObject.AddComponent<CameraMotionInputSource>();
                cameraMotionInputSource.enabled = inputSourceMode == MotionInputSourceMode.CameraMotion;
            }

            if (webSocketMotionInputSource == null)
            {
                webSocketMotionInputSource = gameObject.GetComponent<WebSocketMotionInputSource>() ?? gameObject.AddComponent<WebSocketMotionInputSource>();
                webSocketMotionInputSource.enabled = inputSourceMode == MotionInputSourceMode.WebSocket;
            }

            if (userCameraFeedService == null)
            {
                userCameraFeedService = gameObject.GetComponent<UserCameraFeedService>() ?? gameObject.AddComponent<UserCameraFeedService>();
            }
        }

        private void AttachSceneController(Scene scene)
        {
            if (activeSceneRoot != null)
            {
                Destroy(activeSceneRoot);
            }

            activeSceneRoot = new GameObject($"{scene.name}_RuntimeRoot");

            switch (scene.name)
            {
                case SceneNames.Boot:
                case "SampleScene":
                    activeSceneRoot.AddComponent<BootSceneController>();
                    break;
                case SceneNames.Title:
                    activeSceneRoot.AddComponent<TitleSceneController>();
                    break;
                case SceneNames.Calibration:
                    activeSceneRoot.AddComponent<CalibrationSceneController>();
                    break;
                case SceneNames.Gameplay:
                    activeSceneRoot.AddComponent<GameplaySceneController>();
                    break;
                case SceneNames.Result:
                    activeSceneRoot.AddComponent<ResultSceneController>();
                    break;
            }
        }

        private static GameSessionConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<GameSessionConfig>();
            config.sessionDurationSeconds = 60f;
            config.actionWindowSeconds = 3f;
            config.inputCooldownSeconds = 0.3f;
            config.scorePerSuccess = 100;
            config.comboBonusStep = 25;
            return config;
        }
    }
}
