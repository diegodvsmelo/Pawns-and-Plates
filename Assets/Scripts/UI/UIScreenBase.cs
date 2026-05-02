using UnityEngine;

public class UIScreenBase : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] protected GameObject elements;

    public virtual void Show()
    {
        SetElementsActive(true);
    }

    public virtual void Hide()
    {
        SetElementsActive(false);
    }

    public virtual void Toggle()
    {
        if (elements == null)
            return;

        SetElementsActive(!elements.activeSelf);
    }

    public virtual bool IsVisible()
    {
        if (elements == null)
            return false;

        return elements.activeSelf;
    }

    protected void SetElementsActive(bool value)
    {
        if (elements != null)
            elements.SetActive(value);
    }
}