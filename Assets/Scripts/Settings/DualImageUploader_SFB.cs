using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB; // StandaloneFileBrowser

public class DualImageUploader_SFB : MonoBehaviour
{
    [Header("Botones")]
    [Tooltip("Botón para subir el logotipo único (PNG/JPG)")]
    public Button uploadLogoButton;
    [Tooltip("Botón para subir la imagen del panel (PNG/JPG)")]
    public Button uploadPanelButton;

    [Header("Destinos")]
    [Tooltip("Image donde se mostrará el logotipo")]
    public Image logoImage;
    [Tooltip("Image donde se mostrará la imagen del panel")]
    public Image panelImage;

    [Header("Progreso y estado (Logo)")]
    public Slider logoProgress;
    public TMP_Text logoStatusText;

    [Header("Progreso y estado (Panel)")]
    public Slider panelProgress;
    public TMP_Text panelStatusText;

    [Header("Opciones")]
    [Tooltip("Precargar imágenes guardadas al iniciar")]
    public bool preloadSaved = true;

    [Tooltip("Límite máximo absoluto de lado para cualquier textura (evita gigantes). 4096 suele ser seguro.")]
    public int absoluteMaxSide = 4096;

    [Tooltip("Margen extra sobre el tamaño de pantalla requerido (1.25 = 25% de holgura)")]
    [Range(1.0f, 2.0f)]
    public float requiredSizePadding = 1.25f;

    [Tooltip("Mínimo razonable de textura en px por lado")]
    public int minRequiredSide = 256;

    // Rutas de guardado local
    private string _logoPath;
    private string _panelPath;

    private void Awake()
    {
        // Inicializa rutas de guardado
        _logoPath = Path.Combine(Application.persistentDataPath, "Branding", "company_logo.png");
        _panelPath = Path.Combine(Application.persistentDataPath, "Panels", "panel_image.png");

        // Listeners de botones
        if (uploadLogoButton) uploadLogoButton.onClick.AddListener(() => OnClickUpload(isLogo: true));
        if (uploadPanelButton) uploadPanelButton.onClick.AddListener(() => OnClickUpload(isLogo: false));

        // Ajuste básico de los Image destino y UI inicial
        SetupImage(logoImage);
        SetupImage(panelImage);
        ResetProgress(logoProgress, logoStatusText);
        ResetProgress(panelProgress, panelStatusText);
    }

    private void Start()
    {
        if (!preloadSaved) return;

        // Precargar logo guardado (si existe)
        if (File.Exists(_logoPath))
        {
            try
            {
                var png = File.ReadAllBytes(_logoPath);
                ApplyPngToImage(png, logoImage);
                SetStatus(logoStatusText, "Logo precargado", true);
            }
            catch (Exception ex)
            {
                SetStatus(logoStatusText, $"No se pudo precargar logo: {ex.Message}", false);
            }
        }

        // Precargar imagen de panel (si existe)
        if (File.Exists(_panelPath))
        {
            try
            {
                var png = File.ReadAllBytes(_panelPath);
                ApplyPngToImage(png, panelImage);
                SetStatus(panelStatusText, "Panel precargado", true);
            }
            catch (Exception ex)
            {
                SetStatus(panelStatusText, $"No se pudo precargar panel: {ex.Message}", false);
            }
        }
    }

    private void OnClickUpload(bool isLogo)
    {
        try
        {
            // Acepta PNG + JPG/JPEG. Si quieres que el logo sea solo PNG, cambia el filtro aquí.
            var extensions = new[] { new ExtensionFilter("Imágenes", "png", "jpg", "jpeg") };

            var paths = StandaloneFileBrowser.OpenFilePanel(
                isLogo ? "Selecciona logotipo (PNG/JPG)" : "Selecciona imagen para panel (PNG/JPG)",
                "", extensions, false);

            if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            {
                SetStatus(GetStatusText(isLogo), "Selección cancelada", false);
                return;
            }

            string path = paths[0];
            if (!File.Exists(path))
            {
                SetStatus(GetStatusText(isLogo), "Ruta inválida", false);
                return;
            }

            StartCoroutine(UploadImageRoutine(
                filePath: path,
                targetImage: isLogo ? logoImage : panelImage,
                progress: GetProgressSlider(isLogo),
                status: GetStatusText(isLogo),
                savePath: isLogo ? _logoPath : _panelPath
            ));
        }
        catch (Exception ex)
        {
            SetStatus(GetStatusText(isLogo), $"Error: {ex.Message}", false);
        }
    }

    private IEnumerator UploadImageRoutine(string filePath, Image targetImage, Slider progress, TMP_Text status, string savePath)
    {
        ResetProgress(progress, status);
        SetStatus(status, "Leyendo archivo...", true);

        // Lectura por bloques para progreso visible
        byte[] fileBytes;
        long total = new FileInfo(filePath).Length;
        long read = 0;

        const int chunkSize = 64 * 1024;
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var ms = new MemoryStream((int)fs.Length))
        {
            var buffer = new byte[chunkSize];
            int n;
            while ((n = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, n);
                read += n;
                UpdateProgress(progress, Mathf.Lerp(0f, 0.7f, (float)read / total));
                yield return null;
            }
            fileBytes = ms.ToArray();
        }

        SetStatus(status, "Procesando imagen...", true);

        // Cargar textura en sRGB con mipmaps y buen filtrado
        var tex = LoadTextureFromBytes(fileBytes);
        if (!tex)
        {
            SetStatus(status, "Archivo de imagen inválido", false);
            yield break;
        }
        UpdateProgress(progress, 0.80f);
        yield return null;

        // Resize inteligente según tamaño real del Image en pantalla
        var optimal = SmartResizeForTarget(tex, targetImage, absoluteMaxSide, requiredSizePadding, minRequiredSide);
        if (optimal != tex) Destroy(tex);
        UpdateProgress(progress, 0.90f);
        yield return null;

        // Guardamos como PNG (conserva transparencia cuando la haya)
        var pngBytes = optimal.EncodeToPNG();

        // Aplicar a la UI
        ApplyPngToImage(pngBytes, targetImage);
        UpdateProgress(progress, 0.97f);
        yield return null;

        // Guardar persistente
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllBytes(savePath, pngBytes);
        }
        catch (Exception ex)
        {
            SetStatus(status, $"Guardado con advertencia: {ex.Message}", false);
            UpdateProgress(progress, 1f);
            yield break;
        }

        UpdateProgress(progress, 1f);
        SetStatus(status, "¡Carga completa!", true);
    }

    // ==========================
    // Utilidades de UI / Estado
    // ==========================

    private void SetupImage(Image img)
    {
        if (!img) return;

        img.preserveAspect = true;

        // Asegurar multiplicador de color blanco y alfa 1
        var c = img.color;
        c = Color.white; c.a = 1f;
        img.color = c;

        // Asegurar material por defecto (evitar shaders de transparencia raros)
        img.material = null;
    }

    private void ResetProgress(Slider slider, TMP_Text text)
    {
        if (slider) slider.value = 0f;
        if (text) text.text = "Listo";
        if (text) text.color = new Color(0.9f, 0.9f, 0.9f);
    }

    private void UpdateProgress(Slider slider, float v)
    {
        if (slider) slider.value = Mathf.Clamp01(v);
    }

    private void SetStatus(TMP_Text text, string msg, bool ok)
    {
        if (!text) return;
        text.text = msg;
        text.color = ok ? new Color(0.12f, 0.65f, 0.25f) : new Color(0.8f, 0.2f, 0.2f);
    }

    private Slider GetProgressSlider(bool isLogo) => isLogo ? logoProgress : panelProgress;
    private TMP_Text GetStatusText(bool isLogo) => isLogo ? logoStatusText : panelStatusText;

    // ==========================
    // Carga y Procesado Imagen
    // ==========================

    // Crea textura en sRGB con mipmaps y buen muestreo
    private static Texture2D CreateTexture(bool mipmaps = true)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipmaps, false); // sRGB
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 4;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private static void ApplySampling(Texture2D tex)
    {
        if (!tex) return;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 4;
        tex.wrapMode = TextureWrapMode.Clamp;
    }

    private static Texture2D LoadTextureFromBytes(byte[] bytes)
    {
        var tex = CreateTexture(true);
        if (!tex.LoadImage(bytes, markNonReadable: false))
            return null;
        ApplySampling(tex);
        tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        return tex;
    }

    // Calcula el tamaño en pixeles que realmente necesita el Image en pantalla,
    // considerando el CanvasScaler (scaleFactor) y un padding configurable.
    private static Vector2Int ComputeRequiredPixelSize(Image img, float padding, int minSide)
    {
        if (!img) return new Vector2Int(512, 512);

        var rt = img.rectTransform;
        var size = rt.rect.size; // unidades UI (no pixeles)

        // Escala real del Canvas (Screen Space)
        var canvas = img.canvas ? img.canvas : img.GetComponentInParent<Canvas>();
        float scaleFactor = 1f;
        if (canvas && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            scaleFactor = canvas.scaleFactor;

        int pxW = Mathf.CeilToInt(size.x * scaleFactor);
        int pxH = Mathf.CeilToInt(size.y * scaleFactor);

        // Padding (p.ej., 1.25 = 25% extra de holgura)
        pxW = Mathf.CeilToInt(pxW * Mathf.Max(1f, padding));
        pxH = Mathf.CeilToInt(pxH * Mathf.Max(1f, padding));

        // Mínimo razonable
        pxW = Mathf.Max(pxW, minSide);
        pxH = Mathf.Max(pxH, minSide);

        return new Vector2Int(pxW, pxH);
    }

    // Resize inteligente: limita por tope duro (absoluteMaxSide) y ajusta a lo requerido por el Image.
    private static Texture2D SmartResizeForTarget(Texture2D src, Image target, int absoluteMax, float padding, int minSide)
    {
        if (!src) return null;

        // Límite duro superior (evita texturas gigantes)
        int srcMax = Mathf.Max(src.width, src.height);
        if (srcMax > absoluteMax)
        {
            float s = (float)absoluteMax / srcMax;
            int nw = Mathf.RoundToInt(src.width * s);
            int nh = Mathf.RoundToInt(src.height * s);
            return BlitResize(src, nw, nh, mipmaps: true);
        }

        // Tamaño requerido real del Image en pantalla
        var need = ComputeRequiredPixelSize(target, padding, minSide);

        // Si la fuente ya cubre el tamaño requerido, no reducimos (evita perder detalle)
        if (src.width >= need.x && src.height >= need.y)
            return src;

        // Si la fuente es menor que lo requerido, escalamos hacia arriba suavemente
        float sw = (float)need.x / src.width;
        float sh = (float)need.y / src.height;
        float scale = Mathf.Max(sw, sh);

        int newW = Mathf.RoundToInt(src.width * scale);
        int newH = Mathf.RoundToInt(src.height * scale);

        return BlitResize(src, newW, newH, mipmaps: true);
    }

    private static Texture2D BlitResize(Texture2D src, int newW, int newH, bool mipmaps)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            newW, newH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        var prev = RenderTexture.active;

        Graphics.Blit(src, rt);
        RenderTexture.active = rt;

        var dst = new Texture2D(newW, newH, TextureFormat.RGBA32, mipmaps, false); // sRGB
        dst.ReadPixels(new Rect(0, 0, newW, newH), 0, 0, false);
        dst.Apply(updateMipmaps: mipmaps, makeNoLongerReadable: false);
        ApplySampling(dst);

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }

    private static void ApplyPngToImage(byte[] png, Image target)
    {
        if (!target) return;

        var tex = CreateTexture(true);
        if (!tex.LoadImage(png, markNonReadable: false))
        {
            Debug.LogError("[DualImageUploader] Imagen inválida al aplicar.");
            return;
        }
        ApplySampling(tex);
        tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);

        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.Tight
        );

        target.sprite = sprite;
        target.preserveAspect = true;

        // Asegurar color blanco y alfa 1
        var c = Color.white; c.a = 1f;
        target.color = c;

        // Material por defecto UI
        target.material = null;
    }
}
