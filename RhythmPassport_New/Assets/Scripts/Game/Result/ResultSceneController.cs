using System.Text;
using RhythmPassport.Core.GameFlow;
using RhythmPassport.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RhythmPassport.Game.Result
{
    public sealed class ResultSceneController : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                SceneLoader.LoadCalibration();
            }

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                SceneLoader.LoadTitle();
            }
        }

        private void OnGUI()
        {
            var result = AppRuntime.Instance.LastResult;
            var panel = new Rect((Screen.width - 900) * 0.5f, (Screen.height - 440) * 0.5f, 900, 440);
            GUI.Box(panel, GUIContent.none, SimpleScreenStyles.PanelStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 28, panel.width - 48, 48), "Result", SimpleScreenStyles.TitleStyle);

            var builder = new StringBuilder();
            builder.AppendLine(result.WasAborted ? "세션 상태: 중단됨" : "세션 상태: 완료");
            builder.AppendLine($"점수: {result.Score}");
            builder.AppendLine($"성공 횟수: {result.SuccessCount}");
            builder.AppendLine($"실패 횟수: {result.FailureCount}");
            builder.AppendLine($"플레이 시간: {result.ElapsedSeconds:F1}초");
            builder.AppendLine();
            builder.AppendLine($"요약: {result.Summary}");
            builder.AppendLine();
            builder.AppendLine("R : 다시 하기");
            builder.AppendLine("T : 타이틀로 이동");

            GUI.Label(new Rect(panel.x + 48, panel.y + 110, panel.width - 96, 250), builder.ToString(), SimpleScreenStyles.BodyStyle);
            GUI.Label(new Rect(panel.x + 48, panel.y + 320, panel.width - 96, 60), "Calibration부터 다시 시작해 같은 흐름으로 검증할 수 있습니다.", SimpleScreenStyles.AccentStyle);
            GUI.Label(new Rect(panel.x + 48, panel.y + 368, panel.width - 96, 40), GetRecommendation(result), SimpleScreenStyles.CenteredBodyStyle);
        }

        private static string GetRecommendation(SessionResultData result)
        {
            if (result.WasAborted)
            {
                return "휴식 후 다시 시도해도 괜찮습니다.";
            }

            if (result.SuccessCount >= result.FailureCount)
            {
                return "좋아요. 현재 난이도로 한 세션 더 진행해볼 수 있습니다.";
            }

            return "다음 시도에서는 동작 속도를 조금 더 천천히 맞춰보세요.";
        }
    }
}
