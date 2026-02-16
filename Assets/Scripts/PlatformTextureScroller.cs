using UnityEngine;

/// <summary>
/// Сдвигает offset материала по скорости прокрутки мира (ScrollSpeedProvider),
/// чтобы создать эффект движения платформы, когда лемминг стоит на месте, а мир едет в -Z.
/// Вешай на объект с Renderer (платформа/пол). Используй sharedMaterial, если один материал на много платформ.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlatformTextureScroller : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Имя свойства текстуры (например _MainTex для стандартного шейдера)")]
    private string _textureProperty = "_MainTex";

    [SerializeField]
    [Tooltip("Ось UV для скролла «вперёд» (мир в -Z). 0 = U (offset.x), 1 = V (offset.y)")]
    private int _scrollAxis = 1;

    [SerializeField]
    [Tooltip("Масштаб: насколько сильно сдвигать текстуру при заданной скорости. Подбери по размеру платформы и тайлинга")]
    private float _scrollScale = 0.01f;

    [SerializeField]
    [Tooltip("Направление скролла. Если текстура едет не в ту сторону — поставь -1")]
    private float _scrollDirection = 1f;

    [SerializeField]
    [Tooltip("Использовать sharedMaterial (влияет на все объекты с этим материалом) или material (своя копия)")]
    private bool _useSharedMaterial = false;

    [SerializeField]
    private Renderer _renderer;
    private Material _material;
    private int _propertyId;
    private Vector2 _currentOffset;

    private void Awake()
    {
        _propertyId = Shader.PropertyToID(_textureProperty);
        _material = _useSharedMaterial ? _renderer.sharedMaterial : _renderer.material;
        if (_material.HasProperty(_propertyId))
            _currentOffset = _material.GetTextureOffset(_propertyId);
    }

    private void Update()
    {
        float speed = ScrollSpeedProvider.CurrentSpeed;
        if (speed <= 0f || !_material.HasProperty(_propertyId))
            return;

        // Мир движется в -Z — сдвигаем текстуру так, чтобы казалось, что пол уезжает назад
        float delta = _scrollDirection * speed * _scrollScale * Time.deltaTime;
        if (_scrollAxis == 0)
            _currentOffset.x += delta;
        else
            _currentOffset.y += delta;

        _material.SetTextureOffset(_propertyId, _currentOffset);
    }
}
