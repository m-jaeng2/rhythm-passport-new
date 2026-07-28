using System;
using RhythmPassport.Core.GameFlow;
using UnityEngine;

namespace RhythmPassport.Input
{
    public sealed class CameraMotionInputSource : MonoBehaviour, IMotionInputSource
    {
        [SerializeField] private float sampleIntervalSeconds = 0.06f;
        [SerializeField] private float motionThreshold = 4f;
        [SerializeField] private float emitCooldownSeconds = 0.45f;

        private UserCameraFeedService cameraFeed;
        private Color32[] previousPixels;
        private float nextSampleTime;
        private float nextEmitTime;

        public string SourceName => "Camera Motion";

        public string StatusText { get; private set; } = "카메라 움직임 분석 대기 중";

        public bool IsReady => cameraFeed != null && cameraFeed.IsReady;

        public float LastUpperEnergy { get; private set; }

        public float LastLeftEnergy { get; private set; }

        public float LastRightEnergy { get; private set; }

        public float StrongestMotionValue => Mathf.Max(LastUpperEnergy, Mathf.Max(LastLeftEnergy, LastRightEnergy));

        public string LastDetectedActionName { get; private set; } = "없음";

        public event Action<MotionInputFrame> MotionDetected;

        private void Awake()
        {
            cameraFeed = AppRuntime.Instance != null ? AppRuntime.Instance.UserCameraFeed : null;
        }

        private void Update()
        {
            if (cameraFeed == null || cameraFeed.WebCamTexture == null)
            {
                StatusText = "카메라 입력을 기다리는 중";
                return;
            }

            if (!cameraFeed.IsReady)
            {
                StatusText = cameraFeed.StatusText;
                return;
            }

            if (Time.time < nextSampleTime)
            {
                return;
            }

            nextSampleTime = Time.time + sampleIntervalSeconds;
            AnalyzeFrame(cameraFeed.WebCamTexture);
        }

        private void AnalyzeFrame(WebCamTexture texture)
        {
            var currentPixels = texture.GetPixels32();
            if (currentPixels == null || currentPixels.Length == 0)
            {
                StatusText = "카메라 프레임을 읽는 중";
                return;
            }

            if (previousPixels == null || previousPixels.Length != currentPixels.Length)
            {
                previousPixels = currentPixels;
                StatusText = "기준 프레임 준비 완료";
                return;
            }

            var width = texture.width;
            var height = texture.height;
            var leftEnergy = 0f;
            var rightEnergy = 0f;
            var upperEnergy = 0f;
            var leftCount = 0;
            var rightCount = 0;
            var upperCount = 0;

            var stepX = Mathf.Max(1, width / 24);
            var stepY = Mathf.Max(1, height / 18);

            for (var y = 0; y < height; y += stepY)
            {
                for (var x = 0; x < width; x += stepX)
                {
                    var index = y * width + x;
                    var current = currentPixels[index];
                    var previous = previousPixels[index];

                    var delta =
                        Mathf.Abs(current.r - previous.r) +
                        Mathf.Abs(current.g - previous.g) +
                        Mathf.Abs(current.b - previous.b);

                    var normalizedDelta = delta / 3f;

                    if (x < width / 2)
                    {
                        leftEnergy += normalizedDelta;
                        leftCount += 1;
                    }
                    else
                    {
                        rightEnergy += normalizedDelta;
                        rightCount += 1;
                    }

                    if (y < height / 2)
                    {
                        upperEnergy += normalizedDelta;
                        upperCount += 1;
                    }
                }
            }

            previousPixels = currentPixels;

            LastLeftEnergy = leftCount > 0 ? leftEnergy / leftCount : 0f;
            LastRightEnergy = rightCount > 0 ? rightEnergy / rightCount : 0f;
            LastUpperEnergy = upperCount > 0 ? upperEnergy / upperCount : 0f;

            StatusText = $"분석 중 - 상:{LastUpperEnergy:F1} 좌:{LastLeftEnergy:F1} 우:{LastRightEnergy:F1}";

            if (Time.time < nextEmitTime)
            {
                return;
            }

            if (StrongestMotionValue < motionThreshold)
            {
                return;
            }

            MotionActionType actionType;
            if (LastUpperEnergy >= LastLeftEnergy * 0.9f && LastUpperEnergy >= LastRightEnergy * 0.9f)
            {
                actionType = MotionActionType.RaiseBothHands;
            }
            else if (LastLeftEnergy >= LastRightEnergy)
            {
                actionType = MotionActionType.ReachLeft;
            }
            else
            {
                actionType = MotionActionType.ReachRight;
            }

            nextEmitTime = Time.time + emitCooldownSeconds;
            LastDetectedActionName = GetDisplayName(actionType);
            StatusText = $"자동 인식: {LastDetectedActionName}";
            MotionDetected?.Invoke(new MotionInputFrame(actionType, 0.8f, Time.timeAsDouble));
        }

        private static string GetDisplayName(MotionActionType actionType)
        {
            switch (actionType)
            {
                case MotionActionType.RaiseBothHands:
                    return "양팔 올리기";
                case MotionActionType.ReachLeft:
                    return "왼쪽 팔 뻗기";
                case MotionActionType.ReachRight:
                    return "오른쪽 팔 뻗기";
                default:
                    return "없음";
            }
        }
    }
}
