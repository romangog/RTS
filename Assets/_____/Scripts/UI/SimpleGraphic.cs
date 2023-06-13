using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleGraphic : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action PointerDownLeftEvent;
    public Action PointerDownRightEvent;
    public Action PointerUpLeftEvent;
    public Action PointerUpRightEvent;
    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                PointerUpLeftEvent?.Invoke();
                break;
            case PointerEventData.InputButton.Right:
                PointerUpRightEvent?.Invoke();
                break;
            case PointerEventData.InputButton.Middle:
                break;
            default:
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                PointerDownLeftEvent?.Invoke();
                break;
            case PointerEventData.InputButton.Right:
                PointerDownRightEvent?.Invoke();
                break;
            case PointerEventData.InputButton.Middle:
                break;
            default:
                break;
        }
    }
}
