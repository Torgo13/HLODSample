using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
#if USING_TIMELINE
using UnityEngine.Timeline;
#endif // USING_TIMELINE
using UnityEngine.EventSystems;

/// <summary>
/// This class will enable the touch input canvas on handheld devices and will trigger the camera flythrough if the player is idle
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private bool m_FlythroughWhenIdle;
    [SerializeField] private float m_IdleTransitionTime;
    [SerializeField] private GameObject m_CrosshairCanvas;
    [SerializeField] private GameObject m_TouchInputCanvas;
    [SerializeField] private GameObject m_EventSystem;

#if ENABLED_DIRECTOR
    public PlayableDirector FlythroughDirector;
#endif // ENABLED_DIRECTOR

    private bool m_InFlythrough;
    private float m_TimeIdle;
    private CinemachineCamera m_VirtualCamera;
    private bool m_HasFocus;

    void Start()
    {
        if (EventSystem.current == null)
        {
            m_EventSystem.SetActive(true);
        }

        m_InFlythrough = false;

        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            m_TouchInputCanvas.SetActive(true);
        }

        m_VirtualCamera = GetComponentInChildren<CinemachineCamera>();
    }

    void Update()
    {
        if (m_FlythroughWhenIdle && m_TimeIdle > m_IdleTransitionTime && !m_InFlythrough)
        {
            m_TimeIdle = 0;
            EnableFlythrough();
        }
        
#if UNITY_EDITOR
        if (m_HasFocus)
#endif // UNITY_EDITOR
            m_TimeIdle += Time.unscaledDeltaTime;
    }

    private void Awake()
    {
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void EnableFlythrough()
    {
        if (SceneTransitionManager.IsAvailable())
        {
            SceneTransitionManager.DisableLoadedScene();
            SceneTransitionManager.StopTransition();
        }

#if ENABLED_DIRECTOR
        if (FlythroughDirector == null)
        {
            m_InFlythrough = true;
        }
        else
        {
            FlythroughDirector.gameObject.SetActive(true);

#if USING_TIMELINE
            TimelineAsset timeline = FlythroughDirector.playableAsset as TimelineAsset;
            FlythroughDirector.SetGenericBinding(timeline.GetOutputTrack(0), CinemachineCore.FindPotentialTargetBrain(m_VirtualCamera));
#endif // USING_TIMELINE

            FlythroughDirector.time = 0;
            FlythroughDirector.Play();
            m_InFlythrough = true;
            m_CrosshairCanvas.SetActive(false);

            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                m_TouchInputCanvas.SetActive(false);
            }
        }
#endif // ENABLED_DIRECTOR
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        m_HasFocus = hasFocus;
    }

    public void EnableFirstPersonController()
    {
        if (SceneTransitionManager.IsAvailable())
        {
            SceneTransitionManager.DisableLoadedScene();
            SceneTransitionManager.StopTransition();
        }

        m_CrosshairCanvas.SetActive(true);

#if ENABLED_DIRECTOR
        if (FlythroughDirector != null)
        {
            FlythroughDirector.gameObject.SetActive(false);
        }
#endif // ENABLED_DIRECTOR
        
        m_InFlythrough = false;
    }

    public void NotifyPlayerMoved()
    {
        m_TimeIdle = 0;
        if (m_InFlythrough)
        {
            EnableFirstPersonController();
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                m_TouchInputCanvas.SetActive(true);
            }
        }
    }
}
