using UnityEngine;

/// <summary>
/// This class is responsible for checking if the player is looking at the control panel hologram and triggering a transition if so
/// </summary>
public class LoadingBar : MonoBehaviour
{
    [SerializeField] private bool m_Armed;
    [SerializeField] private Transform m_LookAtTransform;
    [SerializeField] private float m_ActivationDistance = 3;
    [SerializeField] private float m_LookSize;
    [SerializeField] private bool m_AlwaysOn;

    private bool m_Loading;
    private TransformHandle m_BaseCam;
#if USING_ANIMATOR
    private Animator m_ControlPanelAnimator;
#endif // USING_ANIMATOR

    void Start()
    {
        if (SceneTransitionManager.IsAvailable())
        {
            m_BaseCam = SceneTransitionManager.GetMainCamera().transformHandle;
        }
        else
        {
            Destroy(this);
        }
    }

#if USING_ANIMATOR
    private void OnEnable()
    {
        if (m_ControlPanelAnimator == null)
        {
            m_ControlPanelAnimator = GetComponent<Animator>();
        }

        if (m_AlwaysOn) TurnOn();
    }
#endif // USING_ANIMATOR

    void Update()
    {
        m_BaseCam.GetPositionAndRotation(out Vector3 cameraPosition, out var rot);
        Vector3 cameraLookDirection = rot * Vector3.forward;
        UpdateRotation(cameraPosition);

        if (m_Armed && PointOfViewWithinArea(cameraPosition, cameraLookDirection))
        {
            StartLoading();
        } else if (m_Loading)
        {
            StopLoading();
        }
    }

    private void UpdateRotation(Vector3 cameraPosition)
    {
        if (m_LookAtTransform != null)
        {
            m_LookAtTransform.GetPositionAndRotation(out var pos, out var rot);
            var targetRotation = Quaternion.LookRotation(cameraPosition - pos);
            m_LookAtTransform.rotation = Quaternion.Slerp(rot, targetRotation, 1 * Time.deltaTime);
        }
    }

    //This function determines if the player is close enough and looking at the hologram
    private bool PointOfViewWithinArea(Vector3 cameraPosition, Vector3 cameraLookDirection)
    {
        Vector3 pos = m_LookAtTransform.position;
        float distance = Vector3.Distance(pos, cameraPosition);

        if (distance > m_ActivationDistance) return false;

        float activationAngle = Mathf.Atan(m_LookSize * 0.5f / distance) * Mathf.Rad2Deg;
        Vector3 directionToLoader = (pos - m_BaseCam.position).normalized;

        if (Vector3.Angle(directionToLoader, cameraLookDirection) < activationAngle)
        {
            return true;
        }

        return false;
    }

    public void StartLoading()
    {
        if (m_Loading) return;

#if USING_ANIMATOR
        if (m_ControlPanelAnimator != null)
        {
            m_ControlPanelAnimator.SetBool("Loading", true);
        }
#endif // USING_ANIMATOR

        m_Loading = true;

        SceneTransitionManager.StartTransition();
    }

    public void StopLoading()
    {
        if (!m_Loading) return;

#if USING_ANIMATOR
        if (m_ControlPanelAnimator != null)
        {
            m_ControlPanelAnimator.SetBool("Loading", false);
        }
#endif // USING_ANIMATOR

        m_Loading = false;

        SceneTransitionManager.StopTransition();
    }

    [System.Diagnostics.Conditional("USING_ANIMATOR")]
    public void TurnOn()
    {
#if USING_ANIMATOR
        if (m_ControlPanelAnimator != null)
        {
            m_ControlPanelAnimator.SetBool("On", true);
        }
#endif // USING_ANIMATOR
    }

    [System.Diagnostics.Conditional("USING_ANIMATOR")]
    public void TurnOff()
    {
#if USING_ANIMATOR
        if (m_ControlPanelAnimator != null)
        {
            m_ControlPanelAnimator.SetBool("On", false);
        }
#endif // USING_ANIMATOR
    }
}