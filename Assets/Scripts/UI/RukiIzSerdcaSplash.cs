using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Заставка "Руки из сердца". Висит на пустом GameObject в сцене MainMenu
// (грузится первой, сразу после логотипа Unity). Параметры зашиты —
// показывается один раз за запуск игры, поверх сцены, затем исчезает.
public class RukiIzSerdcaSplash : MonoBehaviour
{
    // ===== Экран / размеры =====
    private const float LOGO_HEIGHT = 900f;
    private static readonly Vector2 LOGO_POS = new Vector2(0f, 160f);
    private const float TAGLINE_FONT = 120f;
    private const float TAGLINE_Y = -380f;
    private const float BRAND_FONT = 42f;
    private const float BRAND_TOP_Y = 740f;
    private const float BRAND_BOTTOM_GAP = 140f;

    // ===== Сердце =====
    private const float HEART_SCALE = 1.45f;
    private static readonly Color HEART_COLOR = new Color(0.8862745f, 0.29411766f, 0.2901961f, 1f);
    private static readonly Color HEART_COLOR_END = new Color(0.9098039f, 0.65882355f, 0.4862745f, 1f);
    private const float DIMPLE_DARKNESS = 0.675f;
    private static readonly Vector2 DIMPLE_CENTER = new Vector2(160f, 124.2f);
    private static readonly Vector2 DIMPLE_RADIUS = new Vector2(10.2f, 7.3f);

    // ===== Руки (координаты в дизайн-пространстве 320x300, центр 160,150) =====
    private static readonly Color SKIN_COLOR = new Color(0.8784314f, 0.64705884f, 0.4745098f, 1f);
    private static readonly Vector2 LEFT_ANCHOR = new Vector2(156f, 109.6f);
    private static readonly Vector2 LEFT_PALM = new Vector2(133.5f, 60.4f);
    private const float ARM_THICKNESS = 16f;
    private const float PALM_RADIUS = 12f;
    private const float FINGER_LENGTH = 26f;
    private const float FINGER_THICKNESS = 8f;

    // ===== Фон / тайминг =====
    private static readonly Color BACKGROUND = new Color(0.08627451f, 0.08627451f, 0.10980392f, 1f);
    private const float START_DELAY = 0.5f;
    private const float SPLASH_DURATION = 6f;
    private const float HOLD = 0.6f;
    private const float FADE = 0.45f;
    // ограничение шага времени: фризы загрузки не «проматывают» анимацию
    private const float MAX_DT = 1f / 30f;

    private const string BOOT_SCENE = "Boot";     // лёгкая стартовая сцена
    private const string TARGET_SCENE = "MainMenu"; // что грузим в фоне

    private const float DCX = 160f, DCY = 150f; // центр дизайн-пространства

    [SerializeField] private TMP_FontAsset _font; // шрифт как во всей игре (Softie Cyr SDF 1)

    private static bool _shownThisSession;
    private bool _bootMode;

    private GameObject _root;
    private CanvasGroup _group;
    private RectTransform _figure;
    private Image _heart;
    private RectTransform _tile;
    private CanvasGroup _tileGroup;
    private Vector2 _tileBase;
    private float _k = 1f;

    private int _texW, _texH;
    private float _offX, _offY;
    private Vector2 _heartCenterTex;

    // Висит на объекте в лёгкой сцене Boot. В Boot показывает заставку сразу
    // после лого Unity и асинхронно догружает MainMenu в фоне (без чёрного экрана).
    private void Start()
    {
        if (_shownThisSession) return;
        _shownThisSession = true;
        _bootMode = SceneManager.GetActiveScene().name == BOOT_SCENE;
        if (_bootMode) DontDestroyOnLoad(gameObject);
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        BuildUI();
        Apply(0f);
        yield return null; // показать первый кадр заставки

        // пауза перед стартом (шаг ограничен, чтобы фризы не «проматывали»)
        float d = 0f;
        while (d < START_DELAY) { d += Step(); yield return null; }

        float t = 0f;
        while (t < SPLASH_DURATION)
        {
            t += Step();
            Apply(Mathf.Clamp01(t / SPLASH_DURATION));
            yield return null;
        }
        Apply(1f);

        float h = 0f;
        while (h < HOLD) { h += Step(); yield return null; }

        // меню грузится быстро — грузим обычным способом под заставкой
        if (_bootMode)
        {
            SceneManager.LoadScene(TARGET_SCENE);
            yield return null; // кадр на инициализацию меню под оверлеем
        }

        float f = 0f;
        while (f < FADE)
        {
            f += Step();
            _group.alpha = 1f - Mathf.Clamp01(f / FADE);
            yield return null;
        }
        if (_root != null) Destroy(_root);
        if (_bootMode) Destroy(gameObject);
    }

    private static float Step() { return Mathf.Min(Time.unscaledDeltaTime, MAX_DT); }

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
        bg.gameObject.AddComponent<Image>().color = BACKGROUND;

        ComputeLayout();
        var heartTex = BuildHeart();
        var armsTex = BuildArms();

        float aspect = _texW / (float)_texH;
        _figure = NewRect("Figure", _root.transform);
        _figure.anchorMin = _figure.anchorMax = new Vector2(0.5f, 0.5f);
        _figure.pivot = new Vector2(_heartCenterTex.x / _texW, 1f - _heartCenterTex.y / _texH);
        _figure.sizeDelta = new Vector2(LOGO_HEIGHT * aspect, LOGO_HEIGHT);
        _figure.anchoredPosition = LOGO_POS;

        _heart = AddSprite("Heart", _figure, heartTex, HEART_COLOR);
        AddSprite("Arms", _figure, armsTex, SKIN_COLOR);

        var font = _font != null ? _font : FindCyrillicFont();

        MakeText("компания", BRAND_FONT, MUTED(), new Vector2(0, BRAND_TOP_Y), font, FontStyles.UpperCase, 12f);
        MakeText("представляет", BRAND_FONT, MUTED(), new Vector2(0, TAGLINE_Y - BRAND_BOTTOM_GAP), font, FontStyles.UpperCase, 12f);

        var ruki = MakeText("Руки", TAGLINE_FONT, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        ruki.alignment = TextAlignmentOptions.Left;
        ruki.rectTransform.pivot = new Vector2(0f, 0.5f);
        ruki.ForceMeshUpdate();
        float wRuki = ruki.preferredWidth;

        var probe = MakeText("от сердца", TAGLINE_FONT, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        probe.ForceMeshUpdate();
        float wTile = probe.preferredWidth;
        Destroy(probe.gameObject);

        float space = TAGLINE_FONT * 0.30f;
        float total = wRuki + space + wTile;
        float startX = -total * 0.5f;
        ruki.rectTransform.anchoredPosition = new Vector2(startX, TAGLINE_Y);
        float slotLeft = startX + wRuki + space;

        var reveal = MakeText("из …", TAGLINE_FONT, TEXT(), new Vector2(slotLeft, TAGLINE_Y), font, FontStyles.Normal, 0f);
        reveal.alignment = TextAlignmentOptions.Left;
        reveal.rectTransform.pivot = new Vector2(0f, 0.5f);

        const float pad = 8f;
        _tile = NewRect("Tile", _root.transform);
        _tile.anchorMin = _tile.anchorMax = new Vector2(0.5f, 0.5f);
        _tile.pivot = new Vector2(0f, 1f);
        _tile.sizeDelta = new Vector2(wTile + pad * 2f, TAGLINE_FONT * 1.35f);
        _tileBase = new Vector2(slotLeft - pad, TAGLINE_Y + TAGLINE_FONT * 0.62f);
        _tile.anchoredPosition = _tileBase;
        var tileBg = _tile.gameObject.AddComponent<Image>();
        tileBg.color = BACKGROUND;
        tileBg.raycastTarget = false;
        _tileGroup = _tile.gameObject.AddComponent<CanvasGroup>();
        var tileTxt = MakeText("от сердца", TAGLINE_FONT, TEXT(), Vector2.zero, font, FontStyles.Normal, 0f);
        tileTxt.transform.SetParent(_tile, false);
        tileTxt.alignment = TextAlignmentOptions.Left;
        tileTxt.rectTransform.anchorMin = tileTxt.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        tileTxt.rectTransform.pivot = new Vector2(0f, 0.5f);
        tileTxt.rectTransform.anchoredPosition = new Vector2(pad, 0f);

        _k = TAGLINE_FONT / 30f;
    }

    private void Apply(float p)
    {
        _figure.localRotation = Quaternion.Euler(0, 0, -Eval(FlipKeys, p));
        float c = Mathf.Clamp01((p - 0.86f) / (0.94f - 0.86f));
        _heart.color = Color.Lerp(HEART_COLOR, HEART_COLOR_END, c);

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
    private static Vector2 Scaled(Vector2 p)
    {
        return new Vector2(DCX + (p.x - DCX) * HEART_SCALE, DCY + (p.y - DCY) * HEART_SCALE);
    }

    private static Vector2 S(float x, float y) { return Scaled(new Vector2(x, y)); }

    private static List<Vector2> HeartPoly()
    {
        var poly = new List<Vector2>();
        AddCubic(poly, S(160, 128), S(152, 106), S(120, 106), S(114, 132));
        AddCubic(poly, S(114, 132), S(109, 149), S(128, 170), S(160, 194));
        AddCubic(poly, S(160, 194), S(192, 170), S(211, 149), S(206, 132));
        AddCubic(poly, S(206, 132), S(200, 106), S(168, 106), S(160, 128));
        return poly;
    }

    private void ComputeLayout()
    {
        var poly = HeartPoly();
        float minX = 1e9f, minY = 1e9f, maxX = -1e9f, maxY = -1e9f;
        foreach (var p in poly) Expand(ref minX, ref minY, ref maxX, ref maxY, p, 0f);

        float armPad = Mathf.Max(ARM_THICKNESS, PALM_RADIUS) * 0.5f + 2f;
        float handPad = PALM_RADIUS + FINGER_LENGTH + FINGER_THICKNESS * 0.5f + 2f;
        Vector2 rAnchor = new Vector2(320 - LEFT_ANCHOR.x, LEFT_ANCHOR.y);
        Vector2 rPalm = new Vector2(320 - LEFT_PALM.x, LEFT_PALM.y);
        Expand(ref minX, ref minY, ref maxX, ref maxY, LEFT_ANCHOR, armPad);
        Expand(ref minX, ref minY, ref maxX, ref maxY, rAnchor, armPad);
        Expand(ref minX, ref minY, ref maxX, ref maxY, LEFT_PALM, handPad);
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

    private Texture2D BuildHeart()
    {
        var poly = HeartPoly();
        Vector2 o = new Vector2(_offX, _offY);
        float minX = 1e9f, minY = 1e9f, maxX = -1e9f, maxY = -1e9f;
        for (int i = 0; i < poly.Count; i++)
        {
            poly[i] += o;
            minX = Mathf.Min(minX, poly[i].x); maxX = Mathf.Max(maxX, poly[i].x);
            minY = Mathf.Min(minY, poly[i].y); maxY = Mathf.Max(maxY, poly[i].y);
        }

        var px = new Color[_texW * _texH];
        Vector2 dc = Scaled(DIMPLE_CENTER) + o;
        float drx = DIMPLE_RADIUS.x * HEART_SCALE, dry = DIMPLE_RADIUS.y * HEART_SCALE;

        int X0 = Mathf.Max(0, Mathf.FloorToInt(minX));
        int X1 = Mathf.Min(_texW - 1, Mathf.CeilToInt(maxX));
        int Y0 = Mathf.Max(0, Mathf.FloorToInt(minY));
        int Y1 = Mathf.Min(_texH - 1, Mathf.CeilToInt(maxY));
        int n = poly.Count;
        var rowCov = new float[Mathf.Max(1, X1 - X0 + 1)];
        var xs = new List<float>(8);

        // построчная заливка (scanline) с 2x анти-алиасингом по вертикали
        // и дробным покрытием по горизонтали
        for (int y = Y0; y <= Y1; y++)
        {
            for (int r = 0; r < rowCov.Length; r++) rowCov[r] = 0f;

            for (int s = 0; s < 2; s++)
            {
                float yy = y + 0.25f + s * 0.5f;
                xs.Clear();
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    Vector2 a = poly[j], b = poly[i];
                    if ((a.y <= yy && b.y > yy) || (b.y <= yy && a.y > yy))
                        xs.Add(a.x + (yy - a.y) / (b.y - a.y) * (b.x - a.x));
                }
                xs.Sort();
                for (int c = 0; c + 1 < xs.Count; c += 2)
                {
                    float xa = xs[c], xb = xs[c + 1];
                    int pa = Mathf.Max(X0, Mathf.FloorToInt(xa));
                    int pb = Mathf.Min(X1, Mathf.CeilToInt(xb) - 1);
                    for (int x = pa; x <= pb; x++)
                        rowCov[x - X0] += Mathf.Clamp01(Mathf.Min(x + 1f, xb) - Mathf.Max((float)x, xa)) * 0.5f;
                }
            }

            for (int x = X0; x <= X1; x++)
            {
                float cov = Mathf.Clamp01(rowCov[x - X0]);
                if (cov <= 0f) continue;
                float rgb = 1f;
                float dx = (x - dc.x) / drx, dy = (y - dc.y) / dry;
                if (dx * dx + dy * dy < 1f) rgb = DIMPLE_DARKNESS;
                Set(px, _texW, _texH, x, y, new Color(rgb, rgb, rgb, cov));
            }
        }
        return ToTexture(px, _texW, _texH);
    }

    private Texture2D BuildArms()
    {
        var px = new Color[_texW * _texH];
        Vector2 o = new Vector2(_offX, _offY);
        Vector2 la = LEFT_ANCHOR + o, lp = LEFT_PALM + o;
        Vector2 ra = new Vector2(320 - LEFT_ANCHOR.x, LEFT_ANCHOR.y) + o;
        Vector2 rp = new Vector2(320 - LEFT_PALM.x, LEFT_PALM.y) + o;

        Limb(px, _texW, _texH, la.x, la.y, lp.x, lp.y, ARM_THICKNESS);
        Limb(px, _texW, _texH, ra.x, ra.y, rp.x, rp.y, ARM_THICKNESS);
        Hand(px, _texW, _texH, lp.x, lp.y, -1);
        Hand(px, _texW, _texH, rp.x, rp.y, +1);
        return ToTexture(px, _texW, _texH);
    }

    private static void Hand(Color[] px, int W, int H, float palmX, float palmY, int side)
    {
        Disc(px, W, H, palmX, palmY, PALM_RADIUS);
        float[] baseAng = { -30, -10, 10, 30 };
        foreach (var ba in baseAng)
        {
            float ang = (ba + side * 15f) * Mathf.Deg2Rad;
            Limb(px, W, H, palmX, palmY,
                palmX + FINGER_LENGTH * Mathf.Sin(ang),
                palmY - FINGER_LENGTH * Mathf.Cos(ang), FINGER_THICKNESS);
        }
        float th = side * 78f * Mathf.Deg2Rad;
        Limb(px, W, H, palmX, palmY,
            palmX + FINGER_LENGTH * 0.72f * Mathf.Sin(th),
            palmY - FINGER_LENGTH * 0.72f * Mathf.Cos(th), FINGER_THICKNESS);
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
