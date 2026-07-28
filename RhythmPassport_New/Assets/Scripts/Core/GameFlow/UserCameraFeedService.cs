using UnityEngine;

namespace RhythmPassport.Core.GameFlow
{
    public sealed class UserCameraFeedService : MonoBehaviour
    {
        private WebCamTexture webCamTexture;
        private int lastUpdateFrame = -1;
        private float nextRecoveryTime;
        private string activeDeviceName;

        public Texture PreviewTexture => webCamTexture;

        public WebCamTexture WebCamTexture => webCamTexture;

        public bool IsReady =>
            webCamTexture != null &&
            webCamTexture.isPlaying &&
            webCamTexture.width > 16 &&
            webCamTexture.height > 16;

        public string StatusText { get; private set; } = "카메라 대기 중";

        private void Awake()
        {
            InitializeCamera();
        }

        private void Update()
        {
            if (webCamTexture == null)
            {
                if (Time.time >= nextRecoveryTime)
                {
                    nextRecoveryTime = Time.time + 1f;
                    RecreateCamera();
                }

                return;
            }

            if (!webCamTexture.isPlaying)
            {
                StatusText = "카메라 재시작 중";
                if (Time.time >= nextRecoveryTime)
                {
                    nextRecoveryTime = Time.time + 1f;
                    RecreateCamera();
                }

                return;
            }

            if (webCamTexture.didUpdateThisFrame)
            {
                lastUpdateFrame = Time.frameCount;
                StatusText = $"카메라 연결됨: {webCamTexture.width}x{webCamTexture.height}";
                return;
            }

            if (lastUpdateFrame >= 0 && Time.frameCount - lastUpdateFrame > 20)
            {
                StatusText = "카메라 프레임 갱신 대기 중";

                if (Time.time >= nextRecoveryTime)
                {
                    nextRecoveryTime = Time.time + 1.25f;
                    RecreateCamera();
                }
            }
        }

        private void OnEnable()
        {
            ForceRefresh();
        }

        private void OnDisable()
        {
            DisposeCamera();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                DisposeCamera();
                return;
            }

            ForceRefresh();
        }

        public void ForceRefresh()
        {
            if (webCamTexture == null)
            {
                RecreateCamera();
                return;
            }

            if (!webCamTexture.isPlaying)
            {
                RecreateCamera();
            }
        }

        private void InitializeCamera()
        {
            if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0)
            {
                StatusText = "카메라를 찾을 수 없습니다.";
                return;
            }

            activeDeviceName = WebCamTexture.devices[0].name;
            CreateAndPlayCamera(activeDeviceName);
        }

        private void RecreateCamera()
        {
            if (string.IsNullOrWhiteSpace(activeDeviceName))
            {
                if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0)
                {
                    StatusText = "카메라를 찾을 수 없습니다.";
                    return;
                }

                activeDeviceName = WebCamTexture.devices[0].name;
            }

            DisposeCamera();
            CreateAndPlayCamera(activeDeviceName);
        }

        private void CreateAndPlayCamera(string deviceName)
        {
            webCamTexture = new WebCamTexture(deviceName, 1280, 720, 30);
            webCamTexture.Play();
            lastUpdateFrame = -1;
            StatusText = $"카메라 재연결 중: {deviceName}";
        }

        private void DisposeCamera()
        {
            if (webCamTexture == null)
            {
                return;
            }

            try
            {
                if (webCamTexture.isPlaying)
                {
                    webCamTexture.Stop();
                }
            }
            catch
            {
            }

            Destroy(webCamTexture);
            webCamTexture = null;
        }
    }
}
