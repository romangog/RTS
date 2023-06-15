using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnTargetPositionsMarkersPool : MonoBehaviour
{
    public GameObject ForwardArrow => _forwardArrow;
    [SerializeField] private int _startSize;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private GameObject _forwardArrow;

    private List<GameObject> _pool;
    
    private void Start()
    {
        _pool = new List<GameObject>(_startSize);
        for (int i = 0; i < _startSize; i++)
        {
            var element = Expand();
            element.SetActive(false);
        }
    }

    public GameObject Get()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].activeSelf)
            {
                _pool[i].SetActive(true);
                return _pool[i];
            }
        }
        return Expand();
    }

    public void Return(GameObject element)
    {
        if (_pool.Contains(element))
            element.SetActive(false);
    }

    private GameObject Expand()
    {
        GameObject element = GameObject.Instantiate(_prefab, this.transform);
        _pool.Add(element);
        return element;
    }

    internal void ReturnAll()
    {
        foreach (var element in _pool)
        {
            element.SetActive(false);
        }
    }
}
