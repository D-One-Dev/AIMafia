using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class InteractiveElementScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Button button;
    [SerializeField] private float activeElementScale = 1.1f;
    [SerializeField] private float inactiveElementScale = 1f;
    private Sequence _currentSequence;
    private bool _isSceneLoading;
    public bool ResetScaleOnEnable = true;

    private readonly float ANIMATION_DURATICON = 0.25f;

    private void OnEnable()
    {
        if (ResetScaleOnEnable) rectTransform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSceneLoading || (button != null && !button.interactable)) return;
        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(rectTransform.DOScale(Vector3.one * activeElementScale, ANIMATION_DURATICON));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSceneLoading) return;
        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(rectTransform.DOScale(Vector3.one * inactiveElementScale, ANIMATION_DURATICON));
    }

    private void StopAnimations(int i)
    {
        _isSceneLoading = true;
        _currentSequence.Kill();
    }

    public void SetElementScale(float activeScale = 1.1f, float inactiveScale = 1f)
    {
        if (activeScale <= 0 || inactiveScale <= 0 || inactiveScale >= activeScale) return;

        activeElementScale = activeScale;
        inactiveElementScale = inactiveScale;
    }
}
