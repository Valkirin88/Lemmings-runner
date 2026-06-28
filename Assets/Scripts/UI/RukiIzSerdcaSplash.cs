using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Заставка "Руки из сердца". Повесь компонент на пустой GameObject в первой
// сцене (MainMenu) — она проиграется один раз поверх сцены, сразу после
// логотипа Unity. Параметры настраиваются в инспекторе, картинка логотипа
// показывается живым превью (см. RukiIzSerdcaSplashEditor).
public class RukiIzSerdcaSplash : MonoBehaviour
{
    [Header("Экран / размеры")]
    [Tooltip("Высота логотипа на экране, в пикселях reference 1080x1920")]
    [SerializeField] private float _logoHeight = 900f;
    [SerializeField] private Vector2 _logoPos = new Vector2(0, 160);
    [SerializeField] private float _taglineFont = 120f;
    [SerializeField] private float _taglineY = -380f;
    [SerializeField] private float _brandFont = 42f;
    [SerializeField] private float _brandTopY = 740f;
    [SerializeField] private float _brandBottomGap = 140f;
    [SerializeField] private TMP_FontAsset _font;

    [Header("Сердце")]
    [Tooltip("Размер сердца относительно рук")]
    [Range(0.6f, 2.0f)] [SerializeField] private float _heartScale = 1.45f;
    [SerializeField] private Color _heartColor = new Color32(0xE2, 0x4B, 0x4A, 0xff);
    [SerializeField] private Color _heartColorEnd = new Color32(0xE8, 0xA8, 0x7C, 0xff);
    [Range(0f, 1f)] [SerializeField] private float _dimpleDarkness = 0.42f;
    [SerializeField] private Vector2 _dimpleCenter = new Vector2(160, 138);
    [SerializeField] private Vector2 _dimpleRadius = new Vector2(16, 8);

    [Header("Руки (координаты в дизайн-пространстве 320x300, центр 160,150)")]
    [SerializeField] private Color _skinColor = new Color32(0xE0, 0xA5, 0x79, 0xff);
    [SerializeField] private Vector2 _leftAnchor = new Vector2(156, 122);
    [SerializeField] private Vector2 _leftPalm = new Vector2(126, 64);
    [SerializeField] private float _armThickness = 16f;
    [SerializeField] private float _palmRadius = 12f;
    [SerializeField] private float _fingerLength = 26f;
    [SerializeField] private float _fingerThickness = 8f;

    [Header("Фон / тайминг")]
    [SerializeField] private Color _background = new Color32(0x16, 0x16, 0x1c, 0xff);
    [Tooltip("Задержка перед началом анимации — пауза перед отвалом слов, сек")]
    [SerializeField] private float _startDelay = 0.6f;
    [SerializeField] private float _splashDuration = 6f;
    [SerializeField] private float _hold = 0.5f;
    [SerializeField] private float _fade = 0.45f;
    [SerializeField] private bool _playOnStart = true;
    [Tooltip("Показывать только один раз за запуск игры (не при повторном входе в меню)")]
    [SerializeField] private bool _showOncePerLaunch = true;

    private static bool _shownThisSession;

    private const float DCX = 160f, DCY = 150f; // центр дизайн-пространства

    private GameObject _root;
    private CanvasGroup _group;
    private RectTransform _figure;
    private Image _heart;
    private RectTransform _tile;
    private CanvasGroup _tileGroup;
    private Vector2 _tileBase;
    private float _k = 1f;

    // кэш раскладки текстуры
    private int _texW, _texH;
    private float _offX, _offY;
    private Vector2 _heartCenterTex;

    private void Start()
    {
        if (!_playOnStart) return;
        if (_showOncePerLaunch && _shownThisSession) return;
        _shownThisSession = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        BuildUI();
        Apply(0f);
        if (_startDelay > 0f)
            yield return new WaitForSecondsRealtime(_startDelay);
        float t = 0f;
        while (t < _splashDuration)
        {
            t += Time.unscaledDeltaTime;
            Apply(Mathf.Clamp01(t / _splashDuration));
            yield return null;
        }
        Apply(1f);
        yield return new WaitForSecondsRealtime(_hold);
        float f = 0f;
        while (f < _fade)
        {
            f += Time.unscaledDeltaTime;
            _group.alpha = 1f - Mathf.Clamp01(f / _fade);
            yield return null;
        }
        if (_root != null) Destroy(_root);
    }

    // -------------------------------------------------------------------------
    private void BuildUI()
    {
        _root = new GameObject("RukiIzSerdcaSplashOverlay");
        DontDestroyOnLoad(_root);
        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        _root.AddComponent<GraphicRaycaster>();
        _group = _root.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = true;

        var bg = NewRect("BG", _root.transform);
        Stretch(bg);
        bg.gameObject.AddComponent<Image>().color = _background;

        ComputeLayout();
        var heartTex = BuildHeart();
        var armsTex = BuildArms();

        float aspect = _texW / (float)_texH;
        _figure = NewRect("Figure", _root.transform);
        _figure.anchorMin = _figure.anchorMax = new Vector2(0.5f, 0.5f);
        _figure.pivot = new Vector2(_heartCenterTex.x / _texW, 1f - _heartCenterTex.y / _texH);
        _figure.sizeDelta = new Vector2(_logoHeight * aspect, _logoHeight);
        _figure.anchoredPosition = _logoPos;

        _heart = AddSprite("Heart", _figure, heartTex, _heartColor);
        AddSprite("Arms", _figure, armsTex, _skinColor);

        var font = _font != null ? _font : FindCyrillicFont();

        MakeText("компания", _brandFont, MUTED(), new Vector2(0, _brandTopY), font, FontStyles.UpperCase, 12f);
        MakeText("представляет", _brandFont, MUTED(), new Vector2(0, _taglineY - _brandBottomGap), font, FontStyles.UpperCase, 12f);

        var ruki = MakeText("Руки", _taglineFont, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        ruki.alignment = TextAlignmentOptions.Left;
        ruki.rectTransform.pivot = new Vector2(0f, 0.5f);
        ruki.ForceMeshUpdate();
        float wRuki = ruki.preferredWidth;

        var probe = MakeText("от сердца", _taglineFont, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        probe.ForceMeshUpdate();
        float wTile = probe.preferredWidth;
        Destroy(probe.gameObject);

        float space = _taglineFont * 0.30f;
        float total = wRuki + space + wTile;
        float startX = -total * 0.5f;
        ruki.rectTransform.anchoredPosition = new Vector2(startX, _taglineY);
        float slotLeft = startX + wRuki + space;

        var reveal = MakeText("из …", _taglineFont, TEXT(), new Vector2(slotLeft, _taglineY), font, FontStyles.Normal, 0f);
        reveal.alignment = TextAlignmentOptions.Left;
        reveal.rectTransform.pivot = new Vector2(0f, 0.5f);

        const float pad = 8f;
        _tile = NewRect("Tile", _root.transform);
        _tile.anchorMin = _tile.anchorMax = new Vector2(0.5f, 0.5f);
        _tile.pivot = new Vector2(0f, 1f);
        _tile.sizeDelta = new Vector2(wTile + pad * 2f, _taglineFont * 1.35f);
        _tileBase = new Vector2(slotLeft - pad, _taglineY + _taglineFont * 0.62f);
        _tile.anchoredPosition = _tileBase;
        var tileBg = _tile.gameObject.AddComponent<Image>();
        tileBg.color = _background;
        tileBg.raycastTarget = false;
        _tileGroup = _tile.gameObject.AddComponent<CanvasGroup>();
        var tileTxt = MakeText("от сердца", _taglineFont, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        tileTxt.transform.SetParent(_tile, false);
        tileTxt.alignment = TextAlignmentOptions.Left;
        tileTxt.rectTransform.anchorMin = tileTxt.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        tileTxt.rectTransform.pivot = new Vector2(0f, 0.5f);
        tileTxt.rectTransform.anchoredPosition = new Vector2(pad, 0f);

        _k = _taglineFont / 30f;
    }

    private void Apply(float p)
    {
        _figure.localRotation = Quaternion.Euler(0, 0, -Eval(FlipKeys, p));
        float c = Mathf.Clamp01((p - 0.86f) / (0.94f - 0.86f));
        _heart.color = Color.Lerp(_heartColor, _heartColorEnd, c);

        float tx = Eval(TileX, p), ty = Eval(TileY, p), tr = Eval(TileRot, p), ta = Eval(TileA, p);
        _tile.anchoredPosition = _tileBase + new Vector2(tx * _k, -ty * _k);
        _tile.localRotation = Quaternion.Euler(0, 0, -tr);
        _tileGroup.alpha = ta;
    }

    private static Color TEXT() { return new Color32(0xF3, 0xF0, 0xEA, 0xff); }
    private static Color MUTED() { return new Color32(0x9A, 0x96, 0x8D, 0xff); }

    // ===== Кадры анимации =====
    private static readonly float[,] FlipKeys = {
        {0f,0f},{0.55f,0f},{0.60f,8f},{0.64f,20f},{0.68f,40f},{0.71f,70f},
        {0.74f,110f},{0.765f,160f},{0.785f,205f},{0.81f,168f},{0.84f,188f},
        {0.87f,176f},{0.91f,183f},{0.95f,179f},{1f,180f}
    };
    private static readonly float[,] TileX = { {0f,0f},{0.16f,0f},{0.26f,2f},{0.33f,5f},{0.44f,46f},{1f,46f} };
    private static readonly float[,] TileY = { {0f,0f},{0.16f,0f},{0.26f,5f},{0.33f,10f},{0.44f,210f},{1f,210f} };
    private static readonly float[,] TileRot = { {0f,0f},{0.16f,0f},{0.26f,20f},{0.33f,24f},{0.44f,82f},{1f,82f} };
    private static readonly float[,] TileA = { {0f,1f},{0.16f,1f},{0.33f,1f},{0.44f,0f},{1f,0f} };

    private static float Eval(float[,] keys, float p)
    {
        int n = keys.GetLength(0);
        if (p <= keys[0, 0]) return keys[0, 1];
        if (p >= keys[n - 1, 0]) return keys[n - 1, 1];
        for (int i = 0; i < n - 1; i++)
        {
            float a = keys[i, 0], b = keys[i + 1, 0];
            if (p >= a && p <= b)
            {
                float u = Mathf.Approximately(b, a) ? 0f : (p - a) / (b - a);
                return Mathf.Lerp(keys[i, 1], keys[i + 1, 1], u);
            }
        }
        return keys[n - 1, 1];
    }

    // ===== UI хелперы =====
    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static Image AddSprite(string name, RectTransform parent, Texture2D tex, Color color)
    {
        var rt = NewRect(name, parent);
        Stretch(rt);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private TMP_Text MakeText(string text, float size, Color color, Vector2 pos,
        TMP_FontAsset font, FontStyles style, float spacing)
    {
        var rt = NewRect("T_" + text, _root.transform);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200, size * 1.6f);
        rt.anchoredPosition = pos;
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.characterSpacing = spacing;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static TMP_FontAsset FindCyrillicFont()
    {
        var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        TMP_FontAsset any = null;
        foreach (var f in all)
        {
            if (f == null) continue;
            any = f;
            if (f.name.ToLower().Contains("cyr")) return f;
        }
        if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
        return any;
    }

    // ===== Геометрия / текстуры =====
    private Vector2 Scaled(Vector2 p)
    {
        return new Vector2(DCX + (p.x - DCX) * _heartScale, DCY + (p.y - DCY) * _heartScale);
    }

    private List<Vector2> HeartPoly()
    {
        var poly = new List<Vector2>();
        AddCubic(poly, S(160, 128), S(152, 106), S(120, 106), S(114, 132));
        AddCubic(poly, S(114, 132), S(109, 149), S(128, 170), S(160, 194));
        AddCubic(poly, S(160, 194), S(192, 170), S(211, 149), S(206, 132));
        AddCubic(poly, S(206, 132), S(200, 106), S(168, 106), S(160, 128));
        return poly;
    }

    private Vector2 S(float x, float y) { return Scaled(new Vector2(x, y)); }

    // Считаем общий размер текстуры по содержимому (сердце + руки) с запасом
    private void ComputeLayout()
    {
        var poly = HeartPoly();
        float minX = 1e9f, minY = 1e9f, maxX = -1e9f, maxY = -1e9f;
        foreach (var p in poly) Expand(ref minX, ref minY, ref maxX, ref maxY, p, 0f);

        float armPad = Mathf.Max(_armThickness, _palmRadius) * 0.5f + 2f;
        float handPad = _palmRadius + _fingerLength + _fingerThickness * 0.5f + 2f;
        Vector2 rAnchor = new Vector2(320 - _leftAnchor.x, _leftAnchor.y);
        Vector2 rPalm = new Vector2(320 - _leftPalm.x, _leftPalm.y);
        Expand(ref minX, ref minY, ref maxX, ref maxY, _leftAnchor, armPad);
        Expand(ref minX, ref minY, ref maxX, ref maxY, rAnchor, armPad);
        Expand(ref minX, ref minY, ref maxX, ref maxY, _leftPalm, handPad);
        Expand(ref minX, ref minY, ref maxX, ref maxY, rPalm, handPad);

        const float PAD = 8f;
        minX -= PAD; minY -= PAD; maxX += PAD; maxY += PAD;
        _texW = Mathf.CeilToInt(maxX - minX);
        _texH = Mathf.CeilToInt(maxY - minY);
        _offX = -minX;
        _offY = -minY;
        _heartCenterTex = new Vector2(DCX + _offX, DCY + _offY);
    }

    private static void Expand(ref float minX, ref float minY, ref float maxX, ref float maxY, Vector2 p, float r)
    {
        minX = Mathf.Min(minX, p.x - r); maxX = Mathf.Max(maxX, p.x + r);
        minY = Mathf.Min(minY, p.y - r); maxY = Mathf.Max(maxY, p.y + r);
    }

    public Texture2D BuildHeart()
    {
        if (_texW == 0) ComputeLayout();
        var poly = HeartPoly();
        for (int i = 0; i < poly.Count; i++) poly[i] += new Vector2(_offX, _offY);

        var px = new Color[_texW * _texH];
        Vector2 dc = Scaled(_dimpleCenter) + new Vector2(_offX, _offY);
        float drx = _dimpleRadius.x * _heartScale, dry = _dimpleRadius.y * _heartScale;

        for (int y = 0; y < _texH; y++)
        for (int x = 0; x < _texW; x++)
        {
            float cov = 0f;
            for (int sy = 0; sy < 2; sy++)
            for (int sx = 0; sx < 2; sx++)
                if (InPoly(poly, x + 0.25f + sx * 0.5f, y + 0.25f + sy * 0.5f)) cov += 0.25f;
            if (cov <= 0f) continue;
            float rgb = 1f;
            float dx = (x - dc.x) / drx, dy = (y - dc.y) / dry;
            if (dx * dx + dy * dy < 1f) rgb = _dimpleDarkness;
            Set(px, _texW, _texH, x, y, new Color(rgb, rgb, rgb, cov));
        }
        return ToTexture(px, _texW, _texH);
    }

    public Texture2D BuildArms()
    {
        if (_texW == 0) ComputeLayout();
        var px = new Color[_texW * _texH];
        Vector2 o = new Vector2(_offX, _offY);
        Vector2 la = _leftAnchor + o, lp = _leftPalm + o;
        Vector2 ra = new Vector2(320 - _leftAnchor.x, _leftAnchor.y) + o;
        Vector2 rp = new Vector2(320 - _leftPalm.x, _leftPalm.y) + o;

        Limb(px, _texW, _texH, la.x, la.y, lp.x, lp.y, _armThickness);
        Limb(px, _texW, _texH, ra.x, ra.y, rp.x, rp.y, _armThickness);
        Hand(px, _texW, _texH, lp.x, lp.y, -1);
        Hand(px, _texW, _texH, rp.x, rp.y, +1);
        return ToTexture(px, _texW, _texH);
    }

    public Texture2D BuildCompositePreview()
    {
        ComputeLayout();
        var heart = BuildHeart();
        var arms = BuildArms();
        var hp = heart.GetPixels();
        var ap = arms.GetPixels();
        var op = new Color[hp.Length];
        for (int i = 0; i < op.Length; i++)
        {
            Color c = _background; c.a = 1f;
            float ha = hp[i].a;
            if (ha > 0f)
            {
                Color hc = new Color(_heartColor.r * hp[i].r, _heartColor.g * hp[i].r, _heartColor.b * hp[i].r);
                c = c * (1f - ha) + hc * ha;
            }
            float aa = ap[i].a;
            if (aa > 0f)
                c = c * (1f - aa) + (Color)_skinColor * aa;
            c.a = 1f;
            op[i] = c;
        }
        var outTex = new Texture2D(_texW, _texH, TextureFormat.RGBA32, false);
        outTex.SetPixels(op);
        outTex.Apply();
        Object.DestroyImmediate(heart);
        Object.DestroyImmediate(arms);
        return outTex;
    }

    private void Hand(Color[] px, int W, int H, float palmX, float palmY, int side)
    {
        Disc(px, W, H, palmX, palmY, _palmRadius);
        float[] baseAng = { -30, -10, 10, 30 };
        foreach (var ba in baseAng)
        {
            float ang = (ba + side * 15f) * Mathf.Deg2Rad;
            Limb(px, W, H, palmX, palmY,
                palmX + _fingerLength * Mathf.Sin(ang),
                palmY - _fingerLength * Mathf.Cos(ang), _fingerThickness);
        }
        float th = side * 78f * Mathf.Deg2Rad;
        Limb(px, W, H, palmX, palmY,
            palmX + _fingerLength * 0.72f * Mathf.Sin(th),
            palmY - _fingerLength * 0.72f * Mathf.Cos(th), _fingerThickness);
    }

    private static void Limb(Color[] px, int W, int H, float x0, float y0, float x1, float y1, float thick)
    {
        float len = Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
        int steps = Mathf.CeilToInt(len);
        for (int i = 0; i <= steps; i++)
        {
            float u = i / (float)steps;
            Disc(px, W, H, Mathf.Lerp(x0, x1, u), Mathf.Lerp(y0, y1, u), thick * 0.5f);
        }
    }

    private static void Disc(Color[] px, int W, int H, float cx, float cy, float r)
    {
        int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
        int x1 = Mathf.Min(W - 1, Mathf.CeilToInt(cx + r + 1));
        int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
        int y1 = Mathf.Min(H - 1, Mathf.CeilToInt(cy + r + 1));
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            float a = Mathf.Clamp01(r - d + 0.5f);
            if (a <= 0f) continue;
            Color cur = Get(px, W, H, x, y);
            if (a > cur.a) Set(px, W, H, x, y, new Color(1, 1, 1, a));
        }
    }

    private static void AddCubic(List<Vector2> poly, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        const int N = 24;
        int start = poly.Count == 0 ? 0 : 1;
        for (int i = start; i <= N; i++)
        {
            float t = i / (float)N, u = 1f - t;
            float bx = u * u * u * p0.x + 3f * u * u * t * p1.x + 3f * u * t * t * p2.x + t * t * t * p3.x;
            float by = u * u * u * p0.y + 3f * u * u * t * p1.y + 3f * u * t * t * p2.y + t * t * t * p3.y;
            poly.Add(new Vector2(bx, by));
        }
    }

    private static bool InPoly(List<Vector2> poly, float px, float py)
    {
        bool inside = false;
        int n = poly.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 a = poly[i], b = poly[j];
            if (((a.y > py) != (b.y > py)) &&
                (px < (b.x - a.x) * (py - a.y) / (b.y - a.y) + a.x))
                inside = !inside;
        }
        return inside;
    }

    private static void Set(Color[] px, int W, int H, int x, int y, Color c) { px[(H - 1 - y) * W + x] = c; }
    private static Color Get(Color[] px, int W, int H, int x, int y) { return px[(H - 1 - y) * W + x]; }

    private static Texture2D ToTexture(Color[] px, int W, int H)
    {
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}
