using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class ScrollingPatternUI : MonoBehaviour
{
    [Header("Scroll Speed")]
    [SerializeField] private float speedX = 0.01f;
    [SerializeField] private float speedY = -0.01f;

    [Header("Tiling")]
    [SerializeField] private float tileX = 4f;
    [SerializeField] private float tileY = 4f;

    [Header("Offset")]
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;

    [Header("Options")]
    [SerializeField] private bool useUnscaledTime = true;

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        ApplyUV();
    }

    private void OnEnable()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyUV();
    }

    private void OnValidate()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyUV();
    }

    private void Update()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (Application.isPlaying)
        {
            offsetX += speedX * dt;
            offsetY += speedY * dt;
        }

        ApplyUV();
    }

    private void ApplyUV()
    {
        rawImage.uvRect = new Rect(offsetX, offsetY, tileX, tileY);
    }
}