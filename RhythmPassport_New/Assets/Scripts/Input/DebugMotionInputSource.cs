using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RhythmPassport.Input
{
    public sealed class DebugMotionInputSource : MonoBehaviour, IMotionInputSource
    {
        [SerializeField] private float confidence = 1f;

        public string SourceName => "Debug Keyboard";

        public string StatusText { get; private set; } = "키보드 대기 중";

        public bool IsReady => Keyboard.current != null;

        public event Action<MotionInputFrame> MotionDetected;

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                Publish(MotionActionType.RaiseBothHands);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                Publish(MotionActionType.ReachLeft);
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                Publish(MotionActionType.ReachRight);
            }
        }

        private void Publish(MotionActionType actionType)
        {
            StatusText = $"최근 입력: {actionType}";
            MotionDetected?.Invoke(new MotionInputFrame(
                actionType,
                confidence,
                Time.timeAsDouble));
        }
    }
}
