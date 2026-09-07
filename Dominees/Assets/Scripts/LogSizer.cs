using UnityEngine;
using TMPro;

[ExecuteAlways]
public class LogSizer : MonoBehaviour
{
    public TMP_Text target;
    public float Additioner = 50f;
    public float minSize;

    private RectTransform rt;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (rt == null || target == null) return;

        // Bewaar huidige onderkant in lokale ruimte
        float bottomY = rt.anchoredPosition.y - rt.rect.height * rt.pivot.y;

        // Update text
        target.ForceMeshUpdate();
        float newHeight = Mathf.Max(target.preferredHeight + Additioner, minSize);

        // Pas de grootte aan
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        // Herpositioneer zodat de onderkant op dezelfde plek blijft
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, bottomY + newHeight * rt.pivot.y);
    }
}