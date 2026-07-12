using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ScreenController : MonoBehaviour
{
    public LoadingBar controlPanel;
    private Action m_Callback;
    private MeshRenderer m_MeshRenderer;
#if USING_ANIMATOR
    private Animator m_ScreenAnimator;
#endif // USING_ANIMATOR
    private Vector2 rtLastSize = Vector2.zero;

    private void Start()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
#if USING_ANIMATOR
        m_ScreenAnimator = GetComponent<Animator>();
#endif // USING_ANIMATOR
    }

    public void TurnScreenOn()
    {
        SetScreenRT();

#if USING_ANIMATOR
        if (m_ScreenAnimator != null)
        {
            m_ScreenAnimator.SetBool("ScreenOn", true);
        }

        if (controlPanel != null)
        {
            controlPanel.TurnOn();
        }
#endif // USING_ANIMATOR

        Shader.SetGlobalColor("_TransitionColor", m_MeshRenderer.material.GetColor("_TransitionEdgeColor"));
    }

    public void SetScreenRT()
    {
        if (SceneTransitionManager.IsAvailable())
        {
            var rt = SceneTransitionManager.GetScreenRT();
            if (rt == null)
                return;

            if (rt.width != rtLastSize.x || rt.height != rtLastSize.y)
            {
                rtLastSize.x = rt.width;
                rtLastSize.y = rt.height;

                m_MeshRenderer.material.SetTexture("_ScreenColor", SceneTransitionManager.GetScreenRT());
            }
        }
    }

    private void Update()
    {
        SetScreenRT();
    }

    public void TurnScreenOff(Action callback)
    {
        m_Callback = callback;
#if USING_ANIMATOR
        if (m_ScreenAnimator != null)
        {
            m_ScreenAnimator.SetBool("ScreenOn", false);
        }

        if (controlPanel != null)
        {
            controlPanel.TurnOff();
        }
#endif // USING_ANIMATOR
    }

    //This is called by the animation clip when the screens have switched off
    public void Callback()
    {
        if (m_Callback != null)
        {
            m_Callback();
        }
    }
}