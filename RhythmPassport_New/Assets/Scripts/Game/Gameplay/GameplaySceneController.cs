using System.Text;
using RhythmPassport.Core.GameFlow;
using RhythmPassport.Game.Session;
using RhythmPassport.Input;
using RhythmPassport.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RhythmPassport.Game.Gameplay
{
    public sealed class GameplaySceneController : MonoBehaviour
    {
        private readonly MotionActionType[] gameplayActions =
        {
            MotionActionType.RaiseBothHands,
            MotionActionType.ReachLeft,
            MotionActionType.ReachRight,
        };

        private IMotionInputSource inputSource;
        private UserCameraFeedService cameraFeed;
        private GameSessionController sessionController;
        private MotionJudge motionJudge;
        private GameSessionConfig sessionConfig;

        private MotionActionType currentTarget;
        private float sessionRemainingTime;
        private float targetRemainingTime;
        private bool isPaused;
        private string feedbackMessage = "목표 동작이 제시되면 해당 입력을 수행하세요.";

        private void Awake()
        {
            var runtime = AppRuntime.Instance;
            inputSource = runtime != null ? runtime.MotionInputSource : NullMotionInputSource.Instance;
            cameraFeed = runtime != null ? runtime.UserCameraFeed : null;
            sessionConfig = runtime != null && runtime.SessionConfig != null
                ? runtime.SessionConfig
                : ScriptableObject.CreateInstance<GameSessionConfig>();

            sessionController = new GameSessionController(sessionConfig);
            motionJudge = new MotionJudge(sessionConfig.inputCooldownSeconds);
            sessionController.BeginSession();
            sessionRemainingTime = sessionConfig.sessionDurationSeconds;
            AssignNextTarget("첫 번째 목표가 준비되었습니다.");
        }

        private void OnEnable()
        {
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
            if (Keyboard.current != null)
            {
                if (Keyboard.current.pKey.wasPressedThisFrame)
                {
                    TogglePause();
                }

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    FinishSession(true, "사용자가 세션을 중단했습니다.");
                    return;
                }
            }

            if (isPaused || sessionController.CurrentState != GameSessionState.Playing)
            {
                return;
            }

            sessionController.Tick(Time.deltaTime);
            sessionRemainingTime -= Time.deltaTime;
            targetRemainingTime -= Time.deltaTime;

            if (targetRemainingTime <= 0f)
            {
                sessionController.RegisterFailure();
                AssignNextTarget("시간 초과입니다. 다음 동작으로 넘어갑니다.");
            }

            if (sessionRemainingTime <= 0f)
            {
                FinishSession(false, "세션 시간이 종료되었습니다.");
            }
        }

        private void OnGUI()
        {
            var topLeftCard = new Rect(32, 28, 300, 120);
            var topRightCard = new Rect(Screen.width - 332, 28, 300, 120);
            var previewCard = new Rect(Screen.width - 404, 172, 372, 280);
            var centerTargetCard = new Rect((Screen.width - 460) * 0.5f, 26, 460, 146);
            var bottomScoreCard = new Rect((Screen.width - 420) * 0.5f, Screen.height - 188, 420, 148);
            var feedbackCard = new Rect(32, Screen.height - 178, 500, 138);

            GUI.Label(new Rect((Screen.width - 360) * 0.5f, 16, 360, 48), "Gameplay", SimpleScreenStyles.TitleStyle);

            SimpleScreenStyles.DrawInfoCard(
                topLeftCard,
                "현재 목표 제한 시간",
                $"{Mathf.Max(0, Mathf.CeilToInt(targetRemainingTime))}초\n\n입력 상태\n{inputSource?.StatusText ?? "연결 전"}");

            SimpleScreenStyles.DrawInfoCard(
                topRightCard,
                "세션 타이머",
                $"{Mathf.CeilToInt(sessionRemainingTime)}초\n\n카메라 상태\n{cameraFeed?.StatusText ?? "확인 중"}");

            GUI.Box(centerTargetCard, GUIContent.none, SimpleScreenStyles.PanelStyle);
            GUI.Label(new Rect(centerTargetCard.x + 20, centerTargetCard.y + 18, centerTargetCard.width - 40, 28), "현재 목표", SimpleScreenStyles.CenteredBodyStyle);
            GUI.Label(new Rect(centerTargetCard.x + 20, centerTargetCard.y + 56, centerTargetCard.width - 40, 44), GetDisplayName(currentTarget), SimpleScreenStyles.ScoreStyle);
            SimpleScreenStyles.DrawProgressBar(
                new Rect(centerTargetCard.x + 34, centerTargetCard.y + 112, centerTargetCard.width - 68, 14),
                targetRemainingTime / Mathf.Max(0.01f, sessionConfig.actionWindowSeconds));

            SimpleScreenStyles.DrawCameraFrame(
                previewCard,
                cameraFeed?.PreviewTexture,
                "내 카메라 보기",
                cameraFeed?.StatusText ?? "카메라를 준비 중입니다.");

            GUI.Box(bottomScoreCard, GUIContent.none, SimpleScreenStyles.PanelStyle);
            GUI.Label(new Rect(bottomScoreCard.x + 20, bottomScoreCard.y + 18, bottomScoreCard.width - 40, 28), "점수", SimpleScreenStyles.CenteredBodyStyle);
            GUI.Label(new Rect(bottomScoreCard.x + 20, bottomScoreCard.y + 46, bottomScoreCard.width - 40, 40), sessionController.Score.ToString(), SimpleScreenStyles.ScoreStyle);
            GUI.Label(
                new Rect(bottomScoreCard.x + 30, bottomScoreCard.y + 98, bottomScoreCard.width - 60, 32),
                $"콤보 {sessionController.Combo}   성공 {sessionController.SuccessCount}   실패 {sessionController.FailureCount}",
                SimpleScreenStyles.CenteredBodyStyle);

            var feedback = new StringBuilder();
            feedback.AppendLine(isPaused ? "일시정지 상태입니다. P를 눌러 재개하세요." : feedbackMessage);
            feedback.AppendLine();
            feedback.AppendLine($"입력 소스: {inputSource?.SourceName ?? "없음"}");
            feedback.AppendLine("P : 일시정지 / Esc : 중단");
            SimpleScreenStyles.DrawInfoCard(feedbackCard, "진행 상태", feedback.ToString());

            SimpleScreenStyles.DrawProgressBar(
                new Rect(32, 160, 300, 14),
                sessionRemainingTime / Mathf.Max(0.01f, sessionConfig.sessionDurationSeconds));

            SimpleScreenStyles.DrawStatusBadge(
                new Rect(Screen.width - 260, 462, 228, 38),
                inputSource != null && inputSource.IsReady ? "센서 연결 준비 완료" : "센서 준비 중",
                inputSource != null && inputSource.IsReady ? new Color(0.53f, 0.9f, 0.72f) : new Color(0.97f, 0.78f, 0.37f));
        }

        private void HandleMotionDetected(MotionInputFrame frame)
        {
            if (isPaused || sessionController.CurrentState != GameSessionState.Playing)
            {
                return;
            }

            var result = motionJudge.Evaluate(currentTarget, frame);

            switch (result)
            {
                case MotionJudgementResult.Success:
                    sessionController.RegisterSuccess();
                    AssignNextTarget("정확합니다. 다음 동작을 준비하세요.");
                    break;
                case MotionJudgementResult.WrongAction:
                    sessionController.RegisterFailure();
                    AssignNextTarget("다른 동작이 입력되었습니다. 다음 목표로 넘어갑니다.");
                    break;
                case MotionJudgementResult.CooldownBlocked:
                    feedbackMessage = "입력 쿨다운 중입니다. 잠시 후 다시 시도하세요.";
                    break;
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                sessionController.PauseSession();
                feedbackMessage = "세션이 일시정지되었습니다.";
            }
            else
            {
                sessionController.ResumeSession();
                feedbackMessage = "세션을 재개합니다.";
            }
        }

        private void AssignNextTarget(string message)
        {
            currentTarget = gameplayActions[Random.Range(0, gameplayActions.Length)];
            targetRemainingTime = sessionConfig.actionWindowSeconds;
            feedbackMessage = message;
        }

        private void FinishSession(bool aborted, string summary)
        {
            if (aborted)
            {
                sessionController.AbortSession();
            }
            else
            {
                sessionController.CompleteSession();
            }

            if (AppRuntime.Instance != null)
            {
                AppRuntime.Instance.SetLastResult(new SessionResultData
                {
                    Score = sessionController.Score,
                    SuccessCount = sessionController.SuccessCount,
                    FailureCount = sessionController.FailureCount,
                    WasAborted = aborted,
                    ElapsedSeconds = sessionController.ElapsedSeconds,
                    Summary = summary,
                });
            }

            SceneLoader.LoadResult();
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
