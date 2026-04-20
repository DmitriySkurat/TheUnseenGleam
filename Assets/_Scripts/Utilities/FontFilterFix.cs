using UnityEngine;

public class FontFilterFix : MonoBehaviour
{
    [SerializeField] private Font[] fonts;

    void Awake()
    {
        foreach (var f in fonts) Apply(f);
        Font.textureRebuilt += OnFontTextureRebuilt;
    }

    void OnDestroy()
    {
        Font.textureRebuilt -= OnFontTextureRebuilt;
    }

    void OnFontTextureRebuilt(Font rebuiltFont)
    {
        foreach (var f in fonts)
            if (f == rebuiltFont) Apply(f);
    }

    static void Apply(Font f)
    {
        if (f != null && f.material != null && f.material.mainTexture != null)
            f.material.mainTexture.filterMode = FilterMode.Point;
    }
}
