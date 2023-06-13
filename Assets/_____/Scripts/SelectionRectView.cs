using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionRectView : MonoBehaviour
{
    public RectTransform SelectionRect => _selectionRect;

    [SerializeField] private RectTransform _selectionRect;
}
