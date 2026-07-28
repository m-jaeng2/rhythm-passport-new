using System.Collections.Generic;
using System.Text;
using RhythmPassport.Core.GameFlow;
using RhythmPassport.Input;
using RhythmPassport.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RhythmPassport.Game.Calibration
{
    public sealed class CalibrationSceneController : MonoBehaviour
    {
        private readonly HashSet<MotionActionType> verifiedActions = new HashSet<MotionActionType>();
        private IMotionInputSource inputSource;
        private CameraMotionInputSource cameraMotionInputSource;
        private UserCameraFeedService cameraFeed;
        private string helperMessage = "카메라 프리뷰 안에서 팔과 상체를 크게 움직여보세요.";
        private float nextFallbackCompleteTime;

        private bool IsReady => verifiedActions.Count >= 3;

        private void Awake()
        {
            var runtime = AppRuntime.Instance;
            inputSource = runtime != null ? runtime.MotionInputSource : NullMotionInputSource.Instance;
            cameraFeed = runtime != null ? runtime.UserCameraFeed : null;
            cameraMotionInputSource = inputSource as CameraMotionInputSource;
        }

        private void OnEnable()
        {
            if (cameraFeed != null)
            {
                cameraFeed.ForceRefresh();
            }

            if (inputSource != null)
            {
                inputSource.MotionDetected += HandleMotionDetected;
            }
        }

        private void OnDisable()
        {
            if (inputSource != null)
            {
                inputSource.MotionDetected -= HandleMotionDetected;
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (cameraMotionInputSource != null &&
                !IsReady &&
                cameraMotionInputSource.StrongestMotionValue >= 3.5f &&
                Time.time >= nextFallbackCompleteTime)
            {
                CompleteNextPendingAction();
                nextFallbackCompleteTime = Time.time + 0.8f;
            }

            if (IsReady && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                SceneLoader.LoadGameplay();
            }

            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                SceneLoader.LoadTitle();
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect((Screen.width - 420) * 0.5f, 28, 420, 48), "Calibration", SimpleScreenStyles.TitleStyle);

            var leftCard = new Rect(36, 92, 380, 520);
            var previewCard = new Rect(438, 92, 490, 520);
            var rightCard = new Rect(944, 92, 320, 520);

            var checklist = new StringBuilder();
            checklist.AppendLine("앉은 자세에서 상체가 정면을 향하도록 맞춰주세요.");
            checklist.AppendLine("자신의 모습이 프레임 안에 들어오는지 먼저 확인하세요.");
            checklist.AppendLine();
            checklist.AppendLine(BuildLine(MotionActionType.RaiseBothHands, "양팔 올리기"));
            checklist.AppendLine(BuildLine(MotionActionType.ReachLeft, "왼쪽 팔 뻗기"));
            checklist.AppendLine(BuildLine(MotionActionType.ReachRight, "오른쪽 팔 뻗기"));
            checklist.AppendLine();
            checklist.AppendLine($"안내: {helperMessage}");
            checklist.AppendLine("Backspace : 타이틀로 돌아가기");

            SimpleScreenStyles.DrawInfoCard(leftCard, "준비 안내", checklist.ToString());
            SimpleScreenStyles.DrawCameraFrame(
                previewCard,
                cameraFeed?.PreviewTexture,
                "내 카메라 보기",
                cameraFeed?.StatusText ?? "카메라를 준비 중입니다.");

            var status = new StringBuilder();
            status.AppendLine($"입력 소스: {inputSource?.SourceName ?? "없음"}");
            status.AppendLine($"입력 상태: {inputSource?.StatusText ?? "연결 전"}");
            status.AppendLine($"카메라 상태: {cameraFeed?.StatusText ?? "확인 중"}");
            if (cameraMotionInputSource != null)
            {
                status.AppendLine();
                status.AppendLine($"상단 수치: {cameraMotionInputSource.LastUpperEnergy:F1}");
                status.AppendLine($"좌측 수치: {cameraMotionInputSource.LastLeftEnergy:F1}");
                status.AppendLine($"우측 수치: {cameraMotionInputSource.LastRightEnergy:F1}");
                status.AppendLine($"최근 인식: {cameraMotionInputSource.LastDetectedActionName}");
            }

            status.AppendLine();
            status.AppendLine(IsReady
                ? "준비가 완료되었습니다.\nEnter를 눌러 Gameplay로 이동하세요."
                : "세 동작을 모두 인식하면\nEnter로 시작할 수 있습니다.");

            SimpleScreenStyles.DrawInfoCard(rightCard, "상태", status.ToString());
            SimpleScreenStyles.DrawStatusBadge(
                new Rect(rightCard.x + 40, rightCard.y + 420, rightCard.width - 80, 42),
                IsReady ? "입력 준비 완료" : "입력 준비 중",
                IsReady ? new Color(0.53f, 0.9f, 0.72f) : new Color(0.97f, 0.78f, 0.37f));
        }

        private void HandleMotionDetected(MotionInputFrame frame)
        {
            if (frame.ActionType == MotionActionType.None)
            {
                return;
            }

            verifiedActions.Add(frame.ActionType);
            helperMessage = $"{GetDisplayName(frame.ActionType)} 자동 인식 완료";
        }

        private void CompleteNextPendingAction()
        {
            if (!verifiedActions.Contains(MotionActionType.RaiseBothHands))
            {
                verifiedActions.Add(MotionActionType.RaiseBothHands);
                helperMessage = "움직임 감지로 양팔 올리기 자동 완료";
                return;
            }

            if (!verifiedActions.Contains(MotionActionType.ReachLeft))
            {
                verifiedActions.Add(MotionActionType.ReachLeft);
                helperMessage = "움직임 감지로 왼쪽 팔 뻗기 자동 완료";
                return;
            }

            if (!verifiedActions.Contains(MotionActionType.ReachRight))
            {
                verifiedActions.Add(MotionActionType.ReachRight);
                helperMessage = "움직임 감지로 오른쪽 팔 뻗기 자동 완료";
            }
        }

        private string BuildLine(MotionActionType actionType, string label)
        {
            return verifiedActions.Contains(actionType) ? $"[완료] {label}" : $"[대기] {label}";
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
                    return "알 수 없는 동작";
            }
        }
    }
}
