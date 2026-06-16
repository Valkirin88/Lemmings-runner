using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Плавно меняет цвет заданных объектов в зависимости от счёта.
/// Каждые _scorePerColor очков берётся следующий цвет из списка; между ступенями цвет интерполируется по текущему счёту.
/// </summary>
public class ScoreColorChanger : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Источник очков (UIHandler). Если null — цвет берётся как при 0 очков")]
    private UIHandler _scoreProvider;

    [SerializeField]
    [Tooltip("Объекты, которым меняем цвет (например два — прокинь оба сюда)")]
    private List<Renderer> _targets = new List<Renderer>();

    [SerializeField]
    [Tooltip("Список цветов. Цвет 0 — при 0 очков, цвет 1 — при _scorePerColor очков, и т.д.")]
    private List<Color> _colors = new List<Color>();

    [SerializeField]
    [Min(1)]
    [Tooltip("Сколько очков на один переход к следующему цвету")]
    private int _scorePerColor = 1000;

    [SerializeField]
    [Tooltip("Зациклить палитру: после последнего цвета снова идёт первый. Иначе — остаётся последний")]
    private bool _loopColors = false;

    [SerializeField]
    [Tooltip("Имя свойства цвета в материале. Standard/legacy — _Color, URP/Lit — _BaseColor")]
    private string _colorProperty = "_Color";

    [SerializeField]
    [Tooltip("Использовать material (своя копия у каждого объекта) вместо MaterialPropertyBlock. " +
             "MaterialPropertyBlock не плодит копии материалов и не ломает батчинг")]
    private bool _useMaterialInstance = false;

    private int _colorPropertyId;
    private MaterialPropertyBlock _propertyBlock;
    private int _lastScore = int.MinValue;

    private void Awake()
    {
        _colorPropertyId = Shader.PropertyToID(_colorProperty);
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        Apply(force: true);
    }

    private void Update()
    {
        Apply(force: false);
    }

    private void Apply(bool force)
    {
        if (_colors == null || _colors.Count == 0)
            return;

        int score = _scoreProvider != null ? _scoreProvider.SpawnScore : 0;
        if (!force && score == _lastScore)
            return;
        _lastScore = score;

        Color color = EvaluateColor(score);
        ApplyColor(color);
    }

    /// <summary>
    /// Возвращает цвет для текущего счёта с плавной интерполяцией между ступенями.
    /// </summary>
    private Color EvaluateColor(int score)
    {
        if (_colors.Count == 1)
            return _colors[0];

        if (score <= 0)
            return _colors[0];

        int step = score / _scorePerColor;
        float t = (score % _scorePerColor) / (float)_scorePerColor;

        int fromIndex;
        int toIndex;

        if (_loopColors)
        {
            fromIndex = step % _colors.Count;
            toIndex = (step + 1) % _colors.Count;
        }
        else
        {
            // Без зацикливания: после последнего цвета держим его
            if (step >= _colors.Count - 1)
                return _colors[_colors.Count - 1];

            fromIndex = step;
            toIndex = step + 1;
        }

        return Color.Lerp(_colors[fromIndex], _colors[toIndex], t);
    }

    private void ApplyColor(Color color)
    {
        if (_targets == null)
            return;

        for (int i = 0; i < _targets.Count; i++)
        {
            var renderer = _targets[i];
            if (renderer == null)
                continue;

            if (_useMaterialInstance)
            {
                var material = renderer.material;
                if (material.HasProperty(_colorPropertyId))
                    material.SetColor(_colorPropertyId, color);
            }
            else
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(_colorPropertyId, color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
