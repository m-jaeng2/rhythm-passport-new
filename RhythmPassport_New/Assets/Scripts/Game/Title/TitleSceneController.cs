using RhythmPassport.Core.GameFlow;
using RhythmPassport.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RhythmPassport.Game.Title
{
    public sealed class TitleSceneController : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                AppRuntime.Instance.UseDebugInput();
            }

            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                AppRuntime.Instance.UseCameraMotionInput();
            }

            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                AppRuntime.Instance.UseWebSocketInput();
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                SceneLoader.LoadCalibration();
            }
        }

        private void OnGUI()
        {
            var panel = new Rect((Screen.width - 920) * 0.5f, (Screen.height - 470) * 0.5f, 920, 470);
            GUI.Box(panel, GUIContent.none, SimpleScreenStyles.PanelStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 28, panel.width - 48, 48), "Rhythm Passport", SimpleScreenStyles.TitleStyle);

            GUI.Label(
                new Rect(panel.x + 48, panel.y + 110, panel.width - 96, 210),
                "좌식 환경을 기준으로 만든 헬스케어 모션 게임 MVP입니다.\n\n" +
                "입력 모드\n" +
                "F1 : 키보드 디버그 입력\n" +
                "F2 : 카메라 움직임 자동 인식\n" +
                "F3 : MediaPipe WebSocket 입력\n\n" +
                "Enter : 시작",
                SimpleScreenStyles.BodyStyle);

            GUI.Label(
                new Rect(panel.x + 48, panel.y + 335, panel.width - 96, 32),
                $"현재 입력 소스: {AppRuntime.Instance.MotionInputSource.SourceName}",
                SimpleScreenStyles.AccentStyle);

            GUI.Label(
                new Rect(panel.x + 48, panel.y + 370, panel.width - 96, 32),
                $"입력 상태: {AppRuntime.Instance.MotionInputSource.StatusText}",
                SimpleScreenStyles.BodyStyle);

            GUI.Label(
                new Rect(panel.x + 48, panel.y + 405, panel.width - 96, 40),
                "기준 해상도 1920 x 1080 / Orthographic 카메라 / Unity 단일 환경",
                SimpleScreenStyles.AccentStyle);
        }
    }
}
