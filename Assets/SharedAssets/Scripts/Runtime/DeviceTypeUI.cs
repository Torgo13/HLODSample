using UnityEngine;
#if USING_TMPRO
using Text = TMPro.TMP_Text;
#else
using UnityEngine.UI;
#endif // USING_TMPRO

[RequireComponent(typeof(Text))]
public class DeviceTypeUI : MonoBehaviour
{
    void Start()
    {
        string deviceName = SystemInfo.graphicsDeviceName;
        string deviceTypeName = SystemInfo.graphicsDeviceType.ToString();
        GetComponent<Text>().text = deviceName + "\n" + deviceTypeName;
    }
}
