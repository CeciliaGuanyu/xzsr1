using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

/// <summary>
/// One-click setup for the Object Viewer system.
/// Run via menu: Tools → Setup Object Viewer
/// </summary>
public class ViewerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Object Viewer")]
    static void Setup()
    {
        // Remove existing setup if present
        var existingCanvas = GameObject.Find("Canvas");
        if (existingCanvas != null) DestroyImmediate(existingCanvas);
        var existingPreview = GameObject.Find("PreviewCameraContainer");
        if (existingPreview != null) DestroyImmediate(existingPreview);

        // 1. Create Canvas
        var canvasGO = CreateCanvas();
        CreateEventSystem(canvasGO);
        var panel = CreateViewerPanel(canvasGO);
        CreateCrosshair(canvasGO);
        CreateHintText(canvasGO);

        // 2. Create PreviewCameraContainer
        var previewContainer = CreatePreviewSetup();

        // 3. Configure Player
        ConfigurePlayer(panel);

        // 4. Configure Main Camera
        ConfigureMainCamera();

        // 5. Configure Directional Light
        ConfigureDirectionalLight();

        // 6. Ensure RenderTexture
        EnsureRenderTexture(previewContainer);

        Selection.activeGameObject = canvasGO;
        Debug.Log("[ViewerSetup] Setup complete! Select scene objects and add 'InteractableObject' component to make them inspectable.");
    }

    static GameObject CreateCanvas()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.layer = LayerMask.NameToLayer("UI");
        return canvasGO;
    }

    static void CreateEventSystem(GameObject canvasGO)
    {
        var esGO = new GameObject("EventSystem");
        esGO.transform.SetParent(canvasGO.transform);
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    static ObjectViewerPanel CreateViewerPanel(GameObject canvasGO)
    {
        // ViewerPanel — always active, hosts the script
        var panelGO = new GameObject("ViewerPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        var panel = panelGO.AddComponent<ObjectViewerPanel>();

        // PanelRoot — child that gets toggled on/off
        var rootGO = new GameObject("PanelRoot");
        rootGO.transform.SetParent(panelGO.transform, false);
        var rootRect = rootGO.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        panel.panelRoot = rootGO;

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(rootGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        var bgImg = AddImage(bgGO, new Color(0, 0, 0, 0.85f));
        bgImg.raycastTarget = true;

        // PreviewRawImage
        var previewGO = new GameObject("PreviewRawImage");
        previewGO.transform.SetParent(rootGO.transform, false);
        var previewRect = previewGO.AddComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.5f, 0.5f);
        previewRect.anchorMax = new Vector2(0.5f, 0.5f);
        previewRect.sizeDelta = new Vector2(600, 600);
        previewRect.anchoredPosition = new Vector2(0, 80);
        var rawImg = previewGO.AddComponent<RawImage>();
        rawImg.color = Color.white;
        panel.previewRawImage = rawImg;
        panel.previewArea = previewRect;

        // TitleText
        // 1. 创建游戏物体并设置 UI 布局布局
        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(rootGO.transform, false);
        var titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(600, 40);
        titleRect.anchoredPosition = new Vector2(0, -260);

        // 2. 【核心重构】：改用 TextMeshProUGUI 组件
        var titleText = titleGO.AddComponent<TMPro.TextMeshProUGUI>();

        // 3. 字体处理：尝试自动获取 TMP 默认字体（防止旧的 GetDefaultFont() 报错）
        titleText.font = TMPro.TMP_Settings.defaultFontAsset;

        // 4. 对齐样式属性重构
        titleText.fontSize = 32;
        titleText.alignment = TMPro.TextAlignmentOptions.Center; // TMP 的对齐枚举与原生不同
        titleText.color = Color.white;
        titleText.fontStyle = TMPro.FontStyles.Bold;             // TMP 的加粗枚举

        // 5. 成功赋值给全新重构后的面板
        panel.titleText = titleText;

        // DescriptionText
        var descGO = new GameObject("DescriptionText");
        descGO.transform.SetParent(rootGO.transform, false);
        var descRect = descGO.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 0.5f);
        descRect.anchorMax = new Vector2(0.5f, 0.5f);
        descRect.sizeDelta = new Vector2(600, 60);
        descRect.anchoredPosition = new Vector2(0, -310);

        // 2. 【核心重构】：改用 TextMeshProUGUI 组件
        var descText = descGO.AddComponent<TMPro.TextMeshProUGUI>();

        // 3. 字体处理：自动获取 TMP 默认字体资产
        descText.font = TMPro.TMP_Settings.defaultFontAsset;

        // 4. 对齐样式属性重构
        descText.fontSize = 20;
        descText.alignment = TMPro.TextAlignmentOptions.Center; // 使用 TMP 专属对齐枚举
        descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // 5. 成功赋值给重构后的面板变量
        panel.descriptionText = descText;

        // CloseButton
        var closeGO = new GameObject("CloseButton");
        closeGO.transform.SetParent(rootGO.transform, false);
        var closeRect = closeGO.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.5f);
        closeRect.anchorMax = new Vector2(0.5f, 0.5f);
        closeRect.sizeDelta = new Vector2(160, 50);
        closeRect.anchoredPosition = new Vector2(0, -370);
        var closeImg = AddImage(closeGO, new Color(0.3f, 0.3f, 0.3f, 1f));
        var closeBtn = closeGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        panel.closeButton = closeBtn;

        var closeLabelGO = new GameObject("Label");
        closeLabelGO.transform.SetParent(closeGO.transform, false);
        var labelRect = closeLabelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        var labelText = closeLabelGO.AddComponent<Text>();
        labelText.font = GetDefaultFont();
        labelText.fontSize = 24;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.text = "关闭";

        rootGO.SetActive(false);
        return panel;
    }

    static void CreateCrosshair(GameObject canvasGO)
    {
        var chGO = new GameObject("Crosshair");
        chGO.transform.SetParent(canvasGO.transform, false);
        var chRect = chGO.AddComponent<RectTransform>();
        chRect.anchorMin = new Vector2(0.5f, 0.5f);
        chRect.anchorMax = new Vector2(0.5f, 0.5f);
        chRect.sizeDelta = new Vector2(12, 12);
        chRect.anchoredPosition = Vector2.zero;
        var chImg = AddImage(chGO, Color.white);
        chImg.raycastTarget = false;

        // Store crosshair reference in panel (find it later)
        var panel = canvasGO.GetComponentInChildren<ObjectViewerPanel>(true);
        if (panel != null) panel.crosshair = chImg;
    }

    static void CreateHintText(GameObject canvasGO)
    {
        // 1. 创建游戏物体并设置 UI 布局布局
        var hintGO = new GameObject("HintText");
        hintGO.transform.SetParent(canvasGO.transform, false);
        var hintRect = hintGO.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 1f);
        hintRect.anchorMax = new Vector2(0.5f, 1f);
        hintRect.sizeDelta = new Vector2(400, 40);
        hintRect.anchoredPosition = new Vector2(0, -60);

        // 2. 【核心重构】：改用 TextMeshProUGUI 组件
        var hintText = hintGO.AddComponent<TMPro.TextMeshProUGUI>();

        // 3. 字体处理：自动获取 TMP 默认字体资产
        hintText.font = TMPro.TMP_Settings.defaultFontAsset;

        // 4. 对齐样式属性重构
        hintText.fontSize = 22;
        hintText.alignment = TMPro.TextAlignmentOptions.Center; // 使用 TMP 专属对齐枚举
        hintText.color = new Color(1, 1, 1, 0.9f);
        hintText.fontStyle = TMPro.FontStyles.Bold;             // 使用 TMP 专属加粗枚举

        // 5. 默认关闭提示，等待射线击中线索时由代码动态唤醒
        hintGO.SetActive(false);

        // 6. 成功赋值给重构后的面板变量
        var panel = canvasGO.GetComponentInChildren<ObjectViewerPanel>(true);
        if (panel != null) panel.hintText = hintText;
    }

    static GameObject CreatePreviewSetup()
    {
        var containerGO = new GameObject("PreviewCameraContainer");
        containerGO.transform.position = new Vector3(100, 100, 100);

        // Preview Camera
        var camGO = new GameObject("PreviewCamera");
        camGO.transform.SetParent(containerGO.transform);
        camGO.transform.localPosition = new Vector3(0, 0, -2.5f);
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<UniversalAdditionalCameraData>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.13f);
        cam.fieldOfView = 40;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 50f;
        cam.cullingMask = 1 << LayerMask.NameToLayer("ObjectPreview");
        cam.depth = 1;
        cam.enabled = true;

        // Preview Light
        var lightGO = new GameObject("PreviewLight");
        lightGO.transform.SetParent(containerGO.transform);
        lightGO.transform.localRotation = Quaternion.Euler(50, -30, 0);
        var light = lightGO.AddComponent<Light>();
        lightGO.AddComponent<UniversalAdditionalLightData>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.color = Color.white;
        light.shadows = LightShadows.None;
        light.cullingMask = 1 << LayerMask.NameToLayer("ObjectPreview");

        // Wire references
        var panel = FindObjectOfType<ObjectViewerPanel>(true);
        if (panel != null)
        {
            panel.previewCamera = cam;
            panel.previewLight = light;
            panel.previewSpawnPoint = containerGO.transform;
        }

        return containerGO;
    }

    static void ConfigurePlayer(ObjectViewerPanel panel)
    {
        var playerGO = GameObject.Find("Player");
        if (playerGO == null)
        {
            Debug.LogWarning("[ViewerSetup] 'Player' GameObject not found in scene. Please add FirstPersonController manually.");
            return;
        }

        var controller = playerGO.GetComponent<FirstPersonController>();
        if (controller == null)
            controller = playerGO.AddComponent<FirstPersonController>();

        var cam = Camera.main;
        if (cam != null)
        {
            controller.playerCamera = cam;
            controller.viewerPanel = panel;
        }
    }

    static void ConfigureMainCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        int previewLayer = LayerMask.NameToLayer("ObjectPreview");
        if (previewLayer >= 0)
            cam.cullingMask &= ~(1 << previewLayer);
    }

    static void ConfigureDirectionalLight()
    {
        var lights = FindObjectsOfType<Light>();
        int previewLayer = LayerMask.NameToLayer("ObjectPreview");
        if (previewLayer < 0) return;

        foreach (var light in lights)
        {
            if (light.type == LightType.Directional && light.cullingMask == -1)
                light.cullingMask &= ~(1 << previewLayer);
        }
    }

    static void EnsureRenderTexture(GameObject previewContainer)
    {
        var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Texture/PreviewRT.renderTexture");
        if (rt == null)
        {
            rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            rt.name = "PreviewRT";
            rt.filterMode = FilterMode.Bilinear;
            rt.antiAliasing = 1;
            rt.Create();
            AssetDatabase.CreateAsset(rt, "Assets/Texture/PreviewRT.renderTexture");
            AssetDatabase.SaveAssets();
        }

        var cam = previewContainer.GetComponentInChildren<Camera>();
        if (cam != null) cam.targetTexture = rt;

        var panel = FindObjectOfType<ObjectViewerPanel>(true);
        if (panel != null && panel.previewRawImage != null)
            panel.previewRawImage.texture = rt;
    }

    static Font GetDefaultFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    static Sprite GetUISprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
    }

    static Image AddImage(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>();
        img.sprite = GetUISprite();
        img.color = color;
        return img;
    }
}
