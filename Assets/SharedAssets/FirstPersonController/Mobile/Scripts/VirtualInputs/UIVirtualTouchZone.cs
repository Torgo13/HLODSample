using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using static UIVirtualJoystick;

public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public bool clampToMagnitude;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    //Stored Pointer Values
    private Vector2 pointerDownPosition;
    private Vector2 currentPointerPosition;

    [Header("Output")]
    public Vector2Event touchZoneOutputEvent;

    void Start()
    {
        SetupHandle();
    }

    private void SetupHandle()
    {
        _ = SetObjectActiveState(handleRect, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition);

        if (SetObjectActiveState(handleRect, true))
        {
            UpdateHandleRectPosition(pointerDownPosition, handleRect);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition);
        Vector2 positionDelta = GetDeltaBetweenPositions(pointerDownPosition, currentPointerPosition);
        Vector2 clampedPosition = ClampValuesToMagnitude(positionDelta);
        Vector2 outputPosition = ApplyInversionFilter(clampedPosition, invertXOutputValue, invertYOutputValue);
        OutputPointerEventValue(outputPosition * magnitudeMultiplier, touchZoneOutputEvent);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDownPosition = Vector2.zero;
        currentPointerPosition = Vector2.zero;

        OutputPointerEventValue(Vector2.zero, touchZoneOutputEvent);

        if (SetObjectActiveState(handleRect, false))
        {
            UpdateHandleRectPosition(Vector2.zero, handleRect);
        }
    }

    static void UpdateHandleRectPosition(Vector2 newPosition, RectTransform handleRect)
    {
        handleRect.anchoredPosition = newPosition;
    }
    
    static bool SetObjectActiveState(RectTransform handleRect, bool newState)
    {
        if (handleRect != null)
        {
            handleRect.gameObject.SetActive(newState);
            return true;
        }
        
        return false;
    }

    static
    Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
    {
        return secondPosition - firstPosition;
    }
}
