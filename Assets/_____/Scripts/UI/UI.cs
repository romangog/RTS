using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public SimpleGraphic LevelInteractionGraphic => _pawnSelectionGraphic;
    public SelectionRectView SelectionRectView => _selectionRectView;
    public FinishScreenView FinishScreenView => _finishScreenView;

    [SerializeField] private SimpleGraphic _pawnSelectionGraphic;
    [SerializeField] private SelectionRectView _selectionRectView;
    [SerializeField] private FinishScreenView _finishScreenView;
}
