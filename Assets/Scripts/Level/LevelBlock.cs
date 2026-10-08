using UnityEngine;

// Блок уровня с размером, задаваемым в Inspector (поле Size): коллайдер и визуал подгоняются автоматически.
//  Ground / Platform — верх (трава) в локальном y = 0, блок уходит вниз на Size.y.
//  Water             — поверхность в y = 0, вода уходит вниз на Size.y.
//  Wall / Spikes     — основание в y = 0, растёт вверх на Size.y.
public class LevelBlock : MonoBehaviour
{
    public enum Kind { Ground, Water, Wall, Spikes }

    [SerializeField] private Kind kind = Kind.Ground;
    [SerializeField] private Vector2 size = new Vector2(10f, 4f);

    [Header("Visual parts")]
    [SerializeField] private SpriteRenderer top;
    [SerializeField] private SpriteRenderer fill;
    [Tooltip("Длинная полоса земли (ширина >= longTopMinWidth).")]
    [SerializeField] private Sprite longTopSprite;
    [Tooltip("Короткий блок (узкие платформы и стены).")]
    [SerializeField] private Sprite shortTopSprite;
    [SerializeField] private float longTopMinWidth = 6f;

    // Размеры исходных спрайтов (Art/Background/Мультяшный набор лесных платформ 1 и 3).
    private const float LongTopBodyWidth = 15.1f;   // ширина полосы без прозрачных краёв
    private const float LongTopCenterBelowGrass = 0.355f;
    private const float LongTopBottomBelowGrass = 1.63f;
    private const float ShortTopBodyWidth = 3.27f;
    private const float ShortTopCenterBelowGrass = 0.505f;
    private const float ShortTopBottomBelowGrass = 1.86f;

    public Vector2 Size
    {
        get => size;
        set { size = value; Apply(); }
    }

    public Kind BlockKind => kind;

    public void Apply()
    {
        size = new Vector2(Mathf.Max(0.2f, size.x), Mathf.Max(0.2f, size.y));
        float w = size.x, h = size.y;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            switch (kind)
            {
                case Kind.Ground:
                    box.size = new Vector2(w, h);
                    box.offset = new Vector2(0f, -h / 2f);
                    break;
                case Kind.Water:
                    box.size = new Vector2(w, h);
                    box.offset = new Vector2(0f, -h / 2f);
                    box.isTrigger = true;
                    break;
                case Kind.Wall:
                    box.size = new Vector2(w, h);
                    box.offset = new Vector2(0f, h / 2f);
                    break;
                case Kind.Spikes:
                    box.size = new Vector2(w * 0.92f, h * 0.6f);
                    box.offset = new Vector2(0f, h * 0.3f);
                    box.isTrigger = true;
                    break;
            }
        }

        switch (kind)
        {
            case Kind.Ground: ApplyTerrain(w, h, 0f); break;
            case Kind.Wall: ApplyTerrain(w, h, h); break;
            case Kind.Water: ApplyStretched(top, w, h, -h / 2f); break;
            case Kind.Spikes: ApplyTiled(top, w, h, h / 2f); break;
        }
    }

    // Земля: полоса с травой сверху + кирпичная заливка ниже.
    private void ApplyTerrain(float w, float h, float surfaceY)
    {
        float topBottom = 0f;
        if (top != null)
        {
            bool useLong = w >= longTopMinWidth && longTopSprite != null;
            Sprite s = useLong ? longTopSprite : (shortTopSprite != null ? shortTopSprite : longTopSprite);
            if (s != null) top.sprite = s;
            float bodyW = useLong ? LongTopBodyWidth : ShortTopBodyWidth;
            float centerBelow = useLong ? LongTopCenterBelowGrass : ShortTopCenterBelowGrass;
            topBottom = useLong ? LongTopBottomBelowGrass : ShortTopBottomBelowGrass;
            top.drawMode = SpriteDrawMode.Simple;
            top.transform.localScale = new Vector3(w / bodyW, 1f, 1f);
            top.transform.localPosition = new Vector3(0f, surfaceY - centerBelow, 0f);
        }

        if (fill != null)
        {
            float fillTop = surfaceY - Mathf.Min(1.2f, topBottom - 0.2f);
            float fillBottom = surfaceY - h;
            float fh = fillTop - fillBottom;
            fill.enabled = fh > 0.05f;
            if (fill.enabled)
            {
                fill.drawMode = SpriteDrawMode.Tiled;
                fill.transform.localScale = Vector3.one;
                fill.size = new Vector2(w - 0.1f, fh);
                fill.transform.localPosition = new Vector3(0f, (fillTop + fillBottom) / 2f, 0f);
            }
        }
    }

    private static void ApplyStretched(SpriteRenderer r, float w, float h, float centerY)
    {
        if (r == null || r.sprite == null) return;
        r.drawMode = SpriteDrawMode.Simple;
        Vector2 s = r.sprite.bounds.size;
        r.transform.localScale = new Vector3(w / s.x, h / s.y, 1f);
        r.transform.localPosition = new Vector3(0f, centerY, 0f);
    }

    private static void ApplyTiled(SpriteRenderer r, float w, float h, float centerY)
    {
        if (r == null || r.sprite == null) return;
        Vector2 s = r.sprite.bounds.size;
        float k = h / s.y;
        r.drawMode = SpriteDrawMode.Tiled;
        r.transform.localScale = new Vector3(k, k, 1f);
        r.size = new Vector2(w / k, s.y);
        r.transform.localPosition = new Vector3(0f, centerY, 0f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Apply();
        };
    }
#endif
}
