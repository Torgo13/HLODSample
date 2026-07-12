using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

public class UIVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public float joystickRange = 50f;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    [Header("Output")]
    public Vector2Event joystickOutputEvent;

    void Start()
    {
        SetupHandle();
    }

    private void SetupHandle()
    {
        UpdateHandleRectPosition(Vector2.zero, handleRect);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out Vector2 position);
        position = ApplySizeDelta(position, containerRect.sizeDelta);
        Vector2 clampedPosition = ClampValuesToMagnitude(position);
        Vector2 outputPosition = ApplyInversionFilter(position, invertXOutputValue, invertYOutputValue);
        OutputPointerEventValue(outputPosition * magnitudeMultiplier, joystickOutputEvent);
        UpdateHandleRectPosition(clampedPosition * joystickRange, handleRect);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OutputPointerEventValue(Vector2.zero, joystickOutputEvent);
        UpdateHandleRectPosition(Vector2.zero, handleRect);
    }

    public static void OutputPointerEventValue(Vector2 pointerPosition, Vector2Event outputEvent)
    {
        outputEvent.Invoke(pointerPosition);
    }
    
    private static void UpdateHandleRectPosition(Vector2 newPosition, RectTransform handleRect)
    {
        if (handleRect != null)
        {
            handleRect.anchoredPosition = newPosition;
        }
    }

    static Vector2 ApplySizeDelta(Vector2 position, Vector2 sizeDelta)
    {
        return 2.5f * position / sizeDelta;
    }

    public static
    Vector2 ClampValuesToMagnitude(Vector2 position)
    {
        return Vector2.ClampMagnitude(position, 1);
    }

    public static Vector2 ApplyInversionFilter(Vector2 position, bool invertX, bool invertY)
    {
        return new Vector2(
            invertX ? -position.x : position.x,
            invertY ? -position.y : position.y);
    }
}
