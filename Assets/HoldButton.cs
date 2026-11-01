using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool isHeld;

    public void OnPointerDown(PointerEventData e) { isHeld = true; }
    public void OnPointerUp(PointerEventData e) { isHeld = false; }
    public void OnPointerExit(PointerEventData e) { isHeld = false; }
}