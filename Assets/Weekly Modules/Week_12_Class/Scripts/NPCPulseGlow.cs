using UnityEngine;

public class NPCPulseGlow : MonoBehaviour
{
    [Header("Enable What?")]
    [SerializeField] private bool pulseLight = true;
    [SerializeField] private bool pulseEmission = false;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 3f;

    [Header("Light Pulse (Guaranteed Visible)")]
    [SerializeField] private Light targetLight;
    [SerializeField] private float lightMinIntensity = 0f;
    [SerializeField] private float lightMaxIntensity = 3f;

    [Header("Emission Pulse (Needs shader emission + usually Bloom)")]
    [SerializeField] private Renderer targetRenderer;   // if null, auto-find
    [SerializeField] private Color emissionColor = Color.cyan;
    [SerializeField] private float emissionMinIntensity = 0f;
    [SerializeField] private float emissionMaxIntensity = 3f;

    private bool pulsing;

    // MaterialPropertyBlock lets us change emission without duplicating materials.
    private MaterialPropertyBlock mpb;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponentInChildren<Light>(true);

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>(true);

        mpb = new MaterialPropertyBlock();

        // Ensure light starts off
        if (targetLight != null)
            targetLight.intensity = 0f;

        // Ensure emission starts off (if enabled)
        if (pulseEmission && targetRenderer != null)
            SetEmissionIntensity(0f);
    }

    private void Update()
    {
        if (!pulsing) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1

        if (pulseLight && targetLight != null)
        {
            targetLight.intensity = Mathf.Lerp(lightMinIntensity, lightMaxIntensity, t);
        }

        if (pulseEmission && targetRenderer != null)
        {
            float e = Mathf.Lerp(emissionMinIntensity, emissionMaxIntensity, t);
            SetEmissionIntensity(e);
        }
    }

    public void StartPulse()
    {
        pulsing = true;
        // Debug to confirm it’s being called
        // Debug.Log($"StartPulse called on {name}");
    }

    public void StopPulse()
    {
        pulsing = false;

        if (targetLight != null)
            targetLight.intensity = 0f;

        if (pulseEmission && targetRenderer != null)
            SetEmissionIntensity(0f);
    }

    private void SetEmissionIntensity(float intensity)
    {
        // NOTE: This only works if the shader supports _EmissionColor
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(EmissionColorID, emissionColor * intensity);
        targetRenderer.SetPropertyBlock(mpb);

        // Some shaders require the keyword. (Doesn't hurt if unsupported.)
        var mat = targetRenderer.sharedMaterial;
        if (mat != null) mat.EnableKeyword("_EMISSION");
    }
}