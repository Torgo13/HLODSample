using UnityEngine;
#if USING_TMPRO
using Text = TMPro.TMP_Text;
#else
using UnityEngine.UI;
#endif // USING_TMPRO

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        public int TargetFps;
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        private Text m_GuiText;

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_GuiText = GetComponent<Text>();

            if (TargetFps > 0)
            {
                Application.targetFrameRate = TargetFps;
            }
            else
            {
                Application.targetFrameRate = int.MaxValue;
            }
        }

        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator/fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
#if USING_TMPRO
                m_GuiText.SetText(display, m_CurrentFps);
#else
                m_GuiText.text = string.Format(display, m_CurrentFps);
#endif // USING_TMPRO
            }
        }
    }
}
