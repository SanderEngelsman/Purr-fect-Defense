using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform shopButtonRect;
    [SerializeField] private float slideDistance = 200f; // Pixels to slide left
    [SerializeField] private float slideDuration = 0.5f; // Seconds
    private Vector2 panelClosedPos;
    private Vector2 panelOpenPos;
    private Vector2 buttonClosedPos;
    private Vector2 buttonOpenPos;

    private void OnValidate()
    {
        if (panelRect == null)
            Debug.LogWarning("PanelRect is not assigned in ShopPanelAnimator.", this);
        if (shopButtonRect == null)
            Debug.LogWarning("ShopButtonRect is not assigned in ShopPanelAnimator.", this);
    }

    private void Awake()
    {
        if (panelRect != null)
        {
            panelClosedPos = panelRect.anchoredPosition;
            panelOpenPos = panelClosedPos + new Vector2(-slideDistance, 0);
        }
        if (shopButtonRect != null)
        {
            buttonClosedPos = shopButtonRect.anchoredPosition;
            buttonOpenPos = buttonClosedPos + new Vector2(-slideDistance, 0);
        }
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelOpenPos, buttonOpenPos));
    }

    public void ClosePanel()
    {
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelClosedPos, buttonClosedPos, true));
    }

    private IEnumerator SlidePanel(Vector2 targetPanelPos, Vector2 targetButtonPos, bool deactivateOnComplete = false)
    {
        float elapsed = 0f;
        Vector2 startPanelPos = panelRect.anchoredPosition;
        Vector2 startButtonPos = shopButtonRect.anchoredPosition;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            if (panelRect != null)
            {
                panelRect.anchoredPosition = Vector2.Lerp(startPanelPos, targetPanelPos, t);
            }
            if (shopButtonRect != null)
            {
                shopButtonRect.anchoredPosition = Vector2.Lerp(startButtonPos, targetButtonPos, t);
            }
            yield return null;
        }

        // Ensure final positions
        if (panelRect != null)
        {
            panelRect.anchoredPosition = targetPanelPos;
        }
        if (shopButtonRect != null)
        {
            shopButtonRect.anchoredPosition = targetButtonPos;
        }

        if (deactivateOnComplete)
        {
            gameObject.SetActive(false);
        }
    }
}
