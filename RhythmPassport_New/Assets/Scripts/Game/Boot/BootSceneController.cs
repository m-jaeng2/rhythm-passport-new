using RhythmPassport.Core.GameFlow;
using RhythmPassport.UI;
using UnityEngine;

namespace RhythmPassport.Game.Boot
{
    public sealed class BootSceneController : MonoBehaviour
    {
        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;

            if (elapsed >= 0.25f)
            {
                SceneLoader.LoadTitle();
            }
        }

        private void OnGUI()
        {
            var panel = new Rect((Screen.width - 680) * 0.5f, (Screen.height - 220) * 0.5f, 680, 220);
            GUI.Box(panel, GUIContent.none, SimpleScreenStyles.PanelStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 28, panel.width - 48, 48), "Rhythm Passport", SimpleScreenStyles.TitleStyle);
            GUI.Label(new Rect(panel.x + 40, panel.y + 105, panel.width - 80, 60), "세션을 준비하고 있습니다.\n곧 타이틀 화면으로 이동합니다.", SimpleScreenStyles.BodyStyle);
        }
    }
}
