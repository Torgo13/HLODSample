using UnityEngine;
using UnityEngine.Profiling;
#if USING_TMPRO
using Text = TMPro.TMP_Text;
#else
using UnityEngine.UI;
#endif // USING_TMPRO

[RequireComponent(typeof(Text))]
public class MemoryCounter : MonoBehaviour
{
    private Text m_GuiText;
    private System.Text.StringBuilder sb;

    void Start()
    {
        m_GuiText = GetComponent<Text>();
        sb = new System.Text.StringBuilder();
    }
    
    void Update()
    {
#if USING_TMPRO
        m_GuiText.SetText(sb.Clear()
            .Append("Mono used size: ")
            .Append(Profiler.GetMonoUsedSizeLong() / (1024 * 1024))
            .Append("MB"));
#else
        Debug.Log("Mono used size" + Profiler.GetMonoUsedSizeLong()/1000000 + "Bytes");
        //m_GuiText.text = "" + megabytes;
#endif // USING_TMPRO
    }
}
