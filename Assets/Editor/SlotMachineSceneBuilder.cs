
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SlotMachineSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/RoyalSpin.unity";
    private const float MachineScale = 1.05f;
    private const float MachineCenterX = -350f;
    private const float MachineCenterY = -15f;

    [MenuItem("Tools/Royal Spin/Build Demo Scene", priority = 1)]
    public static void BuildDemoScene()
    {
        Sprite backgroundSprite = LoadSprite("bg_gradient.png");
        Sprite cabinetSprite = LoadSprite("slot-machine1.png");

        Sprite[] symbols =
        {
            LoadSprite("slot-symbol1.png"),
            LoadSprite("slot-symbol2.png"),
            LoadSprite("slot-symbol3.png"),
            LoadSprite("slot-symbol4.png")
        };

        if (backgroundSprite == null || cabinetSprite == null)
        {
            EditorUtility.DisplayDialog(
                "Royal Spin",
                "Background or cabinet artwork is missing from Assets/Art.",
                "OK");
            return;
        }

        for (int i = 0; i < symbols.Length; i++)
        {
            if (symbols[i] == null)
            {
                EditorUtility.DisplayDialog(
                    "Royal Spin",
                    "One or more symbol sprites are missing from Assets/Art.",
                    "OK");
                return;
            }
        }

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        GameObject canvasObject = new GameObject(
            "Main Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage(
            "Purple Halftone Background",
            canvasObject.transform,
            backgroundSprite,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);

        StretchToParent(background.rectTransform);
        background.raycastTarget = false;

        Image cabinet = CreateImage(
            "Pixel Art Slot Machine",
            canvasObject.transform,
            cabinetSprite,
            new Vector2(0.5f, 0.5f),
            new Vector2(MachineCenterX, MachineCenterY),
            new Vector2(816f * MachineScale, 624f * MachineScale));

        cabinet.preserveAspect = true;
        cabinet.raycastTarget = false;

        // Create the three reels.
        ReelController[] reels = new ReelController[3];

        float[] sourceCentersX = { 282.5f, 412.5f, 542.5f };
        float sourceWindowTop = 246f;
        float sourceWindowHeight = 210f;
        float sourceWindowWidth = 109f;

        float machineLeft =
            MachineCenterX - (816f * MachineScale / 2f);

        float machineTop =
            MachineCenterY + (624f * MachineScale / 2f);

        float reelCenterY = machineTop -
            (sourceWindowTop + sourceWindowHeight / 2f) * MachineScale;

        for (int i = 0; i < reels.Length; i++)
        {
            float reelCenterX =
                machineLeft + sourceCentersX[i] * MachineScale;

            GameObject reelObject = new GameObject(
                "Reel " + (i + 1),
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask),
                typeof(ReelController));

            reelObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = reelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(reelCenterX, reelCenterY);
            rect.sizeDelta = new Vector2(
                sourceWindowWidth * MachineScale,
                sourceWindowHeight * MachineScale);

            Image maskImage = reelObject.GetComponent<Image>();
            maskImage.color = Color.white;
            maskImage.raycastTarget = false;

            reelObject.GetComponent<Mask>().showMaskGraphic = false;
            reels[i] = reelObject.GetComponent<ReelController>();
        }

        // Player console panel.
        Image panel = CreateImage(
            "Player Console Panel",
            canvasObject.transform,
            null,
            new Vector2(0.5f, 0.5f),
            new Vector2(520f, 0f),
            new Vector2(590f, 850f));

        panel.color = new Color(0.105f, 0.065f, 0.17f, 0.94f);
        panel.raycastTarget = false;

        AddAccentLine(
            canvasObject.transform,
            new Vector2(520f, 422f),
            new Vector2(548f, 3f),
            new Color(1f, 0.77f, 0.33f, 0.95f));

        CreateText(
            "Console Eyebrow",
            canvasObject.transform,
            "YOUR SESSION / ARCADE EDITION",
            new Vector2(520f, 378f),
            new Vector2(530f, 24f),
            13,
            new Color(0.72f, 0.63f, 0.91f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        CreateText(
            "Console Heading",
            canvasObject.transform,
            "Player console",
            new Vector2(520f, 342f),
            new Vector2(530f, 45f),
            30,
            new Color(0.98f, 0.94f, 1f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        Image balanceCard = CreateImage(
            "Credit Display",
            canvasObject.transform,
            null,
            new Vector2(0.5f, 0.5f),
            new Vector2(520f, 255f),
            new Vector2(540f, 112f));

        balanceCard.color = new Color(0.18f, 0.12f, 0.27f, 1f);

        CreateText(
            "Credits Caption",
            canvasObject.transform,
            "AVAILABLE CREDITS",
            new Vector2(405f, 288f),
            new Vector2(300f, 20f),
            12,
            new Color(0.72f, 0.67f, 0.82f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        Text balanceText = CreateText(
            "Credit Amount",
            canvasObject.transform,
            "1,000 CR",
            new Vector2(520f, 253f),
            new Vector2(500f, 42f),
            31,
            new Color(1f, 0.84f, 0.53f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        CreateText(
            "Credit Note",
            canvasObject.transform,
            "VIRTUAL CREDITS - NO CASH VALUE",
            new Vector2(520f, 220f),
            new Vector2(500f, 18f),
            10,
            new Color(0.63f, 0.57f, 0.75f),
            TextAnchor.MiddleCenter,
            FontStyle.Normal);

        Text betLabel = CreateText(
            "Bet Caption",
            canvasObject.transform,
            "BET: 10 CREDITS / SPIN",
            new Vector2(520f, 165f),
            new Vector2(530f, 24f),
            13,
            new Color(0.91f, 0.86f, 0.97f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        // Bet buttons.
        Button[] betButtons = new Button[3];
        Text[] betTexts = new Text[3];
        int[] betAmounts = { 10, 50, 100 };
        float[] betX = { 340f, 520f, 700f };

        for (int i = 0; i < betButtons.Length; i++)
        {
            CreateBetButton(
                canvasObject.transform,
                "Bet " + betAmounts[i],
                betX[i],
                117f,
                betAmounts[i],
                new Color(0.24f, 0.17f, 0.37f, 1f),
                out betButtons[i],
                out betTexts[i]);
        }

        // No Spin button is created here.
        // The red lever is the only spin control.

        Image resultCard = CreateImage(
            "Last Result Card",
            canvasObject.transform,
            null,
            new Vector2(0.5f, 0.5f),
            new Vector2(520f, -65f),
            new Vector2(540f, 80f));

        resultCard.color = new Color(0.075f, 0.045f, 0.13f, 0.96f);

        Text resultLabel = CreateText(
            "Last Result",
            canvasObject.transform,
            "Ready when you are",
            new Vector2(520f, -55f),
            new Vector2(500f, 29f),
            18,
            new Color(1f, 0.86f, 0.58f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        Text resultDetail = CreateText(
            "Result Detail",
            canvasObject.transform,
            "Match all three center symbols to win.",
            new Vector2(520f, -85f),
            new Vector2(500f, 22f),
            12,
            new Color(0.76f, 0.71f, 0.84f),
            TextAnchor.MiddleCenter,
            FontStyle.Normal);

        CreateText(
            "Paytable Caption",
            canvasObject.transform,
            "MATCHING CENTER LINE / PAYOUTS",
            new Vector2(520f, -130f),
            new Vector2(530f, 23f),
            12,
            new Color(0.81f, 0.74f, 0.94f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        CreatePayout(
            canvasObject.transform, symbols[0],
            "TRIPLE 7", "x25 JACKPOT",
            new Vector2(385f, -170f));

        CreatePayout(
            canvasObject.transform, symbols[3],
            "TRIPLE BAR", "x10 BET",
            new Vector2(655f, -170f));

        CreatePayout(
            canvasObject.transform, symbols[2],
            "TRIPLE BELL", "x8 BET",
            new Vector2(385f, -217f));

        CreatePayout(
            canvasObject.transform, symbols[1],
            "TRIPLE CHERRY", "x6 BET",
            new Vector2(655f, -217f));

        // Reset and statistics.
        Button resetButton;
        Text resetText;

        CreateActionButton(
            canvasObject.transform,
            "Reset Session",
            "RESET SESSION",
            new Vector2(520f, -295f),
            new Vector2(260f, 43f),
            new Color(0.27f, 0.19f, 0.42f, 1f),
            new Color(0.94f, 0.89f, 1f, 1f),
            out resetButton,
            out resetText);

        Text spinsText = CreateText(
            "Spin Count",
            canvasObject.transform,
            "SPINS 0",
            new Vector2(365f, -345f),
            new Vector2(130f, 22f),
            11,
            new Color(0.68f, 0.62f, 0.79f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        Text winsText = CreateText(
            "Win Count",
            canvasObject.transform,
            "WINS 0",
            new Vector2(520f, -345f),
            new Vector2(130f, 22f),
            11,
            new Color(0.68f, 0.62f, 0.79f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        Text bestWinText = CreateText(
            "Best Win",
            canvasObject.transform,
            "BEST WIN -",
            new Vector2(675f, -345f),
            new Vector2(155f, 22f),
            11,
            new Color(0.68f, 0.62f, 0.79f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        // Game manager.
        GameObject managerObject = new GameObject("Slot Game Manager");
        SlotGameManager manager =
            managerObject.AddComponent<SlotGameManager>();

        manager.Reels = reels;
        manager.SymbolSprites = symbols;
        manager.BalanceLabel = balanceText;
        manager.BetLabel = betLabel;
        manager.ResultLabel = resultLabel;
        manager.ResultDetailLabel = resultDetail;
        manager.SpinCountLabel = spinsText;
        manager.WinCountLabel = winsText;
        manager.BestWinLabel = bestWinText;

        // Create the transparent hit target over the red lever.
        GameObject leverObject = new GameObject(
            "Pull Red Lever",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        leverObject.transform.SetParent(canvasObject.transform, false);

        RectTransform leverRect =
            leverObject.GetComponent<RectTransform>();

        leverRect.anchorMin = new Vector2(0.5f, 0.5f);
        leverRect.anchorMax = new Vector2(0.5f, 0.5f);
        leverRect.pivot = new Vector2(0.5f, 0.5f);

        float leverX = machineLeft + 735f * MachineScale;
        float leverY = machineTop - 360f * MachineScale;

        leverRect.anchoredPosition = new Vector2(leverX, leverY);
        leverRect.sizeDelta = new Vector2(100f, 110f);

        Image leverHitImage = leverObject.GetComponent<Image>();
        leverHitImage.color = new Color(1f, 1f, 1f, 0f);
        leverHitImage.raycastTarget = true;

        Button leverButton = leverObject.GetComponent<Button>();
        leverButton.transition = Selectable.Transition.None;

        manager.LeverButton = leverButton;
        manager.BetButtons = betButtons;
        manager.ResetButton = resetButton;

        // Lever animation and click listener.
        LeverPositionToggle leverToggle =
            leverObject.AddComponent<LeverPositionToggle>();

        leverToggle.manager = manager;

        UnityEventTools.AddPersistentListener(
            leverButton.onClick,
            leverToggle.ToggleLever);

        UnityEventTools.AddPersistentListener(
            resetButton.onClick,
            manager.ResetSession);

        for (int i = 0; i < betButtons.Length; i++)
        {
            BetButtonBinder binder =
                betButtons[i].gameObject.AddComponent<BetButtonBinder>();

            binder.Manager = manager;
            binder.OptionIndex = i;

            UnityEventTools.AddPersistentListener(
                betButtons[i].onClick,
                binder.SelectBet);
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));

            Undo.RegisterCreatedObjectUndo(
                eventSystem,
                "Create EventSystem");
        }

        Directory.CreateDirectory("Assets/Scenes");

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

        EditorUtility.DisplayDialog(
            "Royal Spin",
            "Demo scene created without a Spin button. Pull the red lever to spin.",
            "Let's Spin");
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = "Assets/Art/" + fileName;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer =
            AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
                importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite == null)
            Debug.LogError("Could not load sprite at " + path);

        return sprite;
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Sprite sprite,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));

        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = sprite != null;

        return image;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        string contents,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color,
        TextAnchor alignment,
        FontStyle fontStyle)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text));

        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = contents;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = fontStyle;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private static void CreateBetButton(
        Transform parent,
        string name,
        float x,
        float y,
        int amount,
        Color color,
        out Button button,
        out Text text)
    {
        CreateActionButton(
            parent,
            name,
            "BET " + amount,
            new Vector2(x, y),
            new Vector2(155f, 64f),
            color,
            new Color(0.96f, 0.90f, 1f),
            out button,
            out text);

        text.fontSize = 20;
    }

    private static void CreateActionButton(
        Transform parent,
        string name,
        string caption,
        Vector2 position,
        Vector2 size,
        Color backgroundColor,
        Color textColor,
        out Button button,
        out Text text)
    {
        GameObject buttonObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = backgroundColor;

        button = buttonObject.GetComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.4f, 0.37f, 0.48f, 0.72f);
        button.colors = colors;

        text = CreateText(
            name + " Label",
            buttonObject.transform,
            caption,
            Vector2.zero,
            size - new Vector2(12f, 8f),
            17,
            textColor,
            TextAnchor.MiddleCenter,
            FontStyle.Bold);

        StretchToParent(text.rectTransform);
    }

    private static void CreatePayout(
        Transform parent,
        Sprite icon,
        string name,
        string payout,
        Vector2 position)
    {
        Image image = CreateImage(
            name + " Icon",
            parent,
            icon,
            new Vector2(0.5f, 0.5f),
            new Vector2(position.x - 55f, position.y),
            new Vector2(37f, 37f));

        image.raycastTarget = false;

        CreateText(
            name + " Label",
            parent,
            name,
            new Vector2(position.x + 12f, position.y + 7f),
            new Vector2(150f, 20f),
            11,
            new Color(0.94f, 0.89f, 0.98f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        CreateText(
            name + " Multiplier",
            parent,
            payout,
            new Vector2(position.x + 12f, position.y - 9f),
            new Vector2(150f, 18f),
            10,
            new Color(1f, 0.81f, 0.45f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);
    }

    private static void AddAccentLine(
        Transform parent,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        Image line = CreateImage(
            "Console Gold Accent",
            parent,
            null,
            new Vector2(0.5f, 0.5f),
            position,
            size);

        line.color = color;
        line.raycastTarget = false;
    }
}
