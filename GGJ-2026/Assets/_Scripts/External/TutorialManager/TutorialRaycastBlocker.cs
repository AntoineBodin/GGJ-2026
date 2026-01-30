using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TutorialRaycastBlocker : MonoBehaviour, ICanvasRaycastFilter, IPointerClickHandler
{
    [Tooltip("Whitelisted RectTransforms that let clicks pass through.")]
    public List<RectTransform> whitelist = new();

    [Tooltip("Whether the blocker is currently active.")]
    public bool isActive = true;

    public UnityEvent OnOverlayClicked;

    // Replace the whitelist with a new set (null/empty => block everything)
    public void SetWhitelist(IEnumerable<RectTransform> allowed)
    {
        whitelist.Clear();
        if (allowed == null) return;
        foreach (var rt in allowed) if (rt != null) whitelist.Add(rt);
    }

    public void AllowOnly(RectTransform allowed)
    {
        whitelist.Clear();
        if (allowed != null) whitelist.Add(allowed);
    }

    public void BlockAll()
    {
        whitelist.Clear();
        isActive = true;
    }

    public void DisableBlocker()
    {
        isActive = false;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (!isActive || !isActiveAndEnabled) return false;

        foreach (var rt in whitelist)
        {
            if (rt == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, sp, eventCamera))
                return false; // don't consume raycast -> allowed UI receives it
        }

        return true; // consume -> block underlying UI/world
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnOverlayClicked?.Invoke();
    }
}
