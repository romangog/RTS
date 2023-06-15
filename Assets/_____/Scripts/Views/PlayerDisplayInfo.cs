using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplayInfo : MonoBehaviour
{
    private Transform _cameraTransform;

    [SerializeField] private Image _healthbar;
    [SerializeField] private Image _healAbilityProgress;
    [SerializeField] private Image _heavyAttackAbilityProgress;

    internal void SetCameraTranform(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
    }

    private void Update()
    {
        this.transform.forward = _cameraTransform.forward;
    }

    public void SetHealthbarPercent(float percent)
    {
        _healthbar.fillAmount = percent;
    }

    public void SetinfoVisible(bool v)
    {
        this.gameObject.SetActive(v);
    }

    public void SetHealAbilityCdPercent(float percent)
    {
        _healAbilityProgress.fillAmount = percent;
    }
    public void SetHeavyAttackAbilityCdPercent(float percent)
    {
        _heavyAttackAbilityProgress.fillAmount = percent;
    }
}
