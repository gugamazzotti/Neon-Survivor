using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSurvivor
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] Image healthFill;
        [SerializeField] Image xpFill;
        [SerializeField] Text levelText;
        [SerializeField] Text xpText;
        [SerializeField] GameObject levelUpPanel;
        [SerializeField] Text[] upgradeLabels;
        [SerializeField] Button[] upgradeButtons;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] Button restartButton;
        [SerializeField] Button menuButton;
        [SerializeField] Text scoreText;
        [SerializeField] Text coinText;
        [SerializeField] Text gameOverDetail;
        [SerializeField] Text toastText;
        GameObject toastRoot;

        float toastTimer;

        public void EnsureUi()
        {
            if (levelUpPanel == null)
                BuildUi();
            Wire();
        }

        void OnEnable()
        {
            AchievementCatalog.Unlocked += ShowToast;
            if (!Application.isPlaying)
                return;
            Wire();
        }

        void OnDisable()
        {
            AchievementCatalog.Unlocked -= ShowToast;
        }

        void Update()
        {
            if (levelUpPanel == null)
                return;

            Refresh();
            GameManager gm = GameManager.Instance;
            bool gameOver = gm != null && gm.IsGameOver;
            bool choosing = gm != null && gm.IsChoosingUpgrade && !gameOver;

            if (gameOverPanel != null && gameOverPanel.activeSelf != gameOver)
            {
                gameOverPanel.SetActive(gameOver);
                if (gameOver && gameOverDetail != null && gm != null)
                    gameOverDetail.text = Loc.Get("gameover.sub") + "\n" + Loc.Format("gameover.score", gm.RunScore);
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.unscaledDeltaTime;
                if (toastTimer <= 0f)
                {
                    if (toastRoot != null)
                        toastRoot.SetActive(false);
                    else if (toastText != null)
                        toastText.gameObject.SetActive(false);
                }
            }

            if (levelUpPanel.activeSelf != choosing)
            {
                levelUpPanel.SetActive(choosing);
                if (choosing)
                    ApplyLabels(gm.PendingChoices);
            }
        }

        public void Pick0()
        {
            Pick(0);
        }

        public void Pick1()
        {
            Pick(1);
        }

        public void Pick2()
        {
            Pick(2);
        }

        public void Restart()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Restart();
        }

        public void OpenMenu()
        {
            GameSession.OpenMenu();
        }

        public void ShowToast(string message)
        {
            if (toastText == null)
                return;

            if (toastRoot != null)
                toastRoot.SetActive(true);
            else
                toastText.gameObject.SetActive(true);
            toastText.text = message;
            toastTimer = 3.2f;
        }

        void Pick(int index)
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || !gm.IsChoosingUpgrade || gm.PendingChoices == null)
                return;
            if (index < 0 || index >= gm.PendingChoices.Length)
                return;
            if (UpgradeManager.Instance == null)
                return;

            UpgradeManager.Instance.Apply(gm.PendingChoices[index].Type);
        }

        void Wire()
        {
            if (upgradeButtons == null || upgradeButtons.Length < 3 || upgradeButtons[0] == null)
                return;

            Bind(upgradeButtons[0], Pick0);
            Bind(upgradeButtons[1], Pick1);
            Bind(upgradeButtons[2], Pick2);
            if (restartButton != null)
                Bind(restartButton, Restart);
            if (menuButton != null)
                Bind(menuButton, OpenMenu);
        }

        static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        void Refresh()
        {
            PlayerController player = PlayerController.Instance;
            GameManager gm = GameManager.Instance;
            if (healthFill != null)
                healthFill.fillAmount = player == null ? 0f : (float)player.Health / PlayerController.MaxHealth;
            if (levelText != null)
                levelText.text = Loc.Format("hud.level", gm == null ? 1 : gm.Level);
            if (gm == null)
                return;
            if (xpFill != null)
                xpFill.fillAmount = gm.XpToLevel <= 0 ? 0f : (float)gm.CurrentXp / gm.XpToLevel;
            if (xpText != null)
                xpText.text = Loc.Format("hud.xp", gm.CurrentXp, gm.XpToLevel);
            if (scoreText != null)
            {
                string score = Loc.Format("hud.score", gm.RunScore);
                int nextMap = GameSession.MapIndex + 1;
                if (nextMap < MapCatalog.Count)
                    score += " / " + MapCatalog.Maps[nextMap].RequiredScore;
                scoreText.text = Loc.Get(MapCatalog.Current.NameKey) + "   " + score;
            }

            if (coinText != null)
                coinText.text = Loc.Format("hud.coins", SaveProfile.Coins);
        }

        void ApplyLabels(UpgradeChoice[] choices)
        {
            if (choices == null || upgradeLabels == null)
                return;

            for (int i = 0; i < upgradeLabels.Length; i++)
            {
                bool show = i < choices.Length;
                if (upgradeButtons != null && i < upgradeButtons.Length && upgradeButtons[i] != null)
                    upgradeButtons[i].gameObject.SetActive(show);
                if (!show || upgradeLabels[i] == null)
                    continue;
                upgradeLabels[i].text = choices[i].Title + "\n" + choices[i].Description;
            }
        }

        public void BuildUi()
        {
            if (levelUpPanel != null)
                return;

            Loc.Ensure();
            Font font = BuiltinFont();
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2100f);
            scaler.matchWidthOrHeight = 0f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();

            GameObject plateGo = new GameObject("HudPlate", typeof(RectTransform));
            plateGo.transform.SetParent(transform, false);
            Image plate = plateGo.AddComponent<Image>();
            plate.raycastTarget = false;
            Sprite hudSprite = NeonArt.IsSlim(NeonArt.Card) ? NeonArt.Card : NeonArt.Panel;
            bool slimHud = NeonArt.IsSlim(hudSprite);
            if (hudSprite != null)
                NeonArt.Paint(plate, hudSprite, true);
            else
            {
                plate.sprite = NeonVisuals.White;
                plate.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            }

            Place(plate.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -16f), slimHud ? new Vector2(980f, 400f) : new Vector2(1032f, 580f));

            RectTransform well = new GameObject("Well", typeof(RectTransform)).GetComponent<RectTransform>();
            well.SetParent(plateGo.transform, false);
            well.anchorMin = Vector2.zero;
            well.anchorMax = Vector2.one;
            if (slimHud)
            {
                well.offsetMin = new Vector2(40f, 32f);
                well.offsetMax = new Vector2(-40f, -32f);
            }
            else
            {
                well.offsetMin = new Vector2(118f, 108f);
                well.offsetMax = new Vector2(-118f, -116f);
            }

            Text title = CreateText("Title", well, font, 28, FontStyle.Bold, new Color(0.55f, 1f, 1f, 0.9f), TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -2f), new Vector2(760f, 36f));
            title.text = "NEON SURVIVOR";

            levelText = CreateText("Level", well, font, 30, FontStyle.Bold, Color.white, TextAnchor.UpperLeft);
            Place(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -40f), new Vector2(760f, 36f));

            healthFill = CreateBar(well, "Health", new Vector2(8f, -86f), new Vector2(760f, 64f), new Color(1f, 0.28f, 0.48f, 1f));
            xpFill = CreateBar(well, "XP", new Vector2(8f, -160f), new Vector2(760f, 56f), new Color(0.25f, 0.95f, 1f, 1f));

            xpText = CreateText("XpLabel", well, font, 20, FontStyle.Normal, new Color(0.75f, 0.95f, 1f, 0.9f), TextAnchor.UpperLeft);
            Place(xpText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -222f), new Vector2(760f, 28f));

            scoreText = CreateText("Score", well, font, 22, FontStyle.Bold, new Color(0.95f, 0.85f, 0.45f, 0.95f), TextAnchor.UpperLeft);
            Place(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -252f), new Vector2(760f, 32f));

            coinText = CreateText("Coins", well, font, 22, FontStyle.Bold, new Color(1f, 0.86f, 0.4f, 1f), TextAnchor.UpperLeft);
            Place(coinText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -286f), new Vector2(760f, 32f));

            toastRoot = new GameObject("Toast", typeof(RectTransform));
            toastRoot.transform.SetParent(transform, false);
            Image toastImage = toastRoot.AddComponent<Image>();
            toastImage.raycastTarget = false;
            if (NeonArt.Toast != null)
                NeonArt.Paint(toastImage, NeonArt.Toast, true);
            else
            {
                toastImage.sprite = NeonVisuals.White;
                toastImage.color = new Color(0.03f, 0.07f, 0.09f, 0.88f);
            }

            Place(toastImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(960f, 168f));
            toastText = CreateText("Label", toastRoot.transform, font, 26, FontStyle.Bold, new Color(0.7f, 1f, 0.85f, 1f), TextAnchor.MiddleCenter);
            RectTransform toastLabel = toastText.rectTransform;
            toastLabel.anchorMin = Vector2.zero;
            toastLabel.anchorMax = Vector2.one;
            toastLabel.offsetMin = new Vector2(72f, 46f);
            toastLabel.offsetMax = new Vector2(-72f, -46f);
            toastRoot.SetActive(false);

            levelUpPanel = CreateModal(
                "LevelUp",
                font,
                Loc.Get("levelup.title"),
                Loc.Get("levelup.sub"),
                out upgradeButtons,
                out upgradeLabels);
            levelUpPanel.SetActive(false);

            Button[] ignoredButtons;
            Text[] ignoredLabels;
            gameOverPanel = CreateModal("GameOver", font, Loc.Get("gameover.title"), Loc.Get("gameover.sub"), out ignoredButtons, out ignoredLabels);
            restartButton = ignoredButtons[0];
            menuButton = ignoredButtons[1];
            restartButton.GetComponentInChildren<Text>().text = Loc.Get("gameover.retry");
            menuButton.GetComponentInChildren<Text>().text = Loc.Get("gameover.menu");
            ignoredButtons[2].gameObject.SetActive(false);
            bool slimButtons = NeonArt.IsSlim(NeonArt.Button);
            restartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, slimButtons ? -420f : -620f);
            menuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, slimButtons ? -590f : -860f);
            gameOverDetail = gameOverPanel.transform.Find("Card/Subtitle").GetComponent<Text>();
            gameOverDetail.horizontalOverflow = HorizontalWrapMode.Wrap;
            gameOverDetail.rectTransform.sizeDelta = new Vector2(700f, 110f);
            gameOverPanel.SetActive(false);
        }

        GameObject CreateModal(string name, Font font, string heading, string subtitle, out Button[] buttons, out Text[] labels)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image dim = panel.AddComponent<Image>();
            dim.sprite = NeonVisuals.White;
            dim.color = new Color(0.01f, 0.01f, 0.03f, 0.72f);
            dim.raycastTarget = true;

            GameObject card = new GameObject("Card", typeof(RectTransform));
            card.transform.SetParent(panel.transform, false);
            Image cardImage = card.AddComponent<Image>();
            Sprite cardSprite = NeonArt.IsSlim(NeonArt.Card) ? NeonArt.Card : NeonArt.Panel;
            if (cardSprite != null)
                NeonArt.Paint(cardImage, cardSprite, true);
            else
            {
                cardImage.sprite = NeonVisuals.White;
                cardImage.color = new Color(0.03f, 0.05f, 0.1f, 0.96f);
            }
            bool slim = NeonArt.IsSlim(NeonArt.Button);
            Place(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, slim ? new Vector2(900f, 980f) : new Vector2(960f, 1120f));

            Text title = CreateText("Heading", card.transform, font, 48, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), slim ? new Vector2(0f, -72f) : new Vector2(0f, -140f), new Vector2(700f, 72f));
            title.text = heading;

            Text sub = CreateText("Subtitle", card.transform, font, 24, FontStyle.Normal, new Color(0.8f, 0.9f, 1f, 0.85f), TextAnchor.MiddleCenter);
            Place(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), slim ? new Vector2(0f, -148f) : new Vector2(0f, -220f), new Vector2(700f, 64f));
            sub.text = subtitle;

            buttons = new Button[3];
            labels = new Text[3];
            float buttonH = slim ? 148f : 200f;
            float[] y = slim ? new[] { -250f, -420f, -590f } : new[] { -380f, -620f, -860f };
            for (int i = 0; i < 3; i++)
            {
                buttons[i] = CreateButton(card.transform, "Choice" + i, font, out labels[i]);
                Place(buttons[i].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y[i]), new Vector2(700f, buttonH));
            }

            return panel;
        }

        static Button CreateButton(Transform parent, string name, Font font, out Text label)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = NeonVisuals.White;
            image.color = Color.white;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.07f, 0.11f, 0.18f, 1f);
            colors.highlightedColor = new Color(0.1f, 0.32f, 0.42f, 1f);
            colors.pressedColor = new Color(0.25f, 0.85f, 0.95f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            NeonArt.PaintButton(button);
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            label = CreateText("Label", go.transform, font, 26, FontStyle.Bold, new Color(0.75f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            bool slim = NeonArt.IsSlim(NeonArt.Button);
            labelRect.offsetMin = slim ? new Vector2(48f, 16f) : new Vector2(150f, 70f);
            labelRect.offsetMax = slim ? new Vector2(-48f, -16f) : new Vector2(-150f, -70f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.text = Loc.Get("upgrade.placeholder");
            return button;
        }

        static Sprite roundedBar;

        static Image CreateBar(Transform parent, string name, Vector2 position, Vector2 size, Color fillColor)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);

            float lipY = Mathf.Clamp(size.y * 0.22f, 10f, 16f);
            float lipX = Mathf.Max(28f, size.y * 0.46f);
            GameObject clip = new GameObject(name + "Clip", typeof(RectTransform));
            clip.transform.SetParent(root.transform, false);
            RectTransform clipRect = clip.GetComponent<RectTransform>();
            clipRect.anchorMin = Vector2.zero;
            clipRect.anchorMax = Vector2.one;
            clipRect.offsetMin = new Vector2(lipX, lipY);
            clipRect.offsetMax = new Vector2(-lipX, -lipY);

            Image track = clip.AddComponent<Image>();
            track.sprite = RoundedBar();
            track.type = Image.Type.Sliced;
            track.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.22f);
            track.raycastTarget = false;
            Mask mask = clip.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            GameObject fillGo = new GameObject(name + "Fill", typeof(RectTransform));
            fillGo.transform.SetParent(clip.transform, false);
            Image fill = fillGo.AddComponent<Image>();
            fill.sprite = NeonVisuals.White;
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject frameGo = new GameObject(name + "Frame", typeof(RectTransform));
            frameGo.transform.SetParent(root.transform, false);
            Image frame = frameGo.AddComponent<Image>();
            frame.raycastTarget = false;
            if (NeonArt.Bar != null)
                NeonArt.Paint(frame, NeonArt.Bar, true);
            else
            {
                frame.sprite = RoundedBar();
                frame.type = Image.Type.Sliced;
                frame.color = new Color(1f, 1f, 1f, 0.16f);
            }

            RectTransform frameRect = frame.rectTransform;
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            return fill;
        }

        static Sprite RoundedBar()
        {
            if (roundedBar != null)
                return roundedBar;

            const int size = 48;
            const float radius = 16f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float ix = Mathf.Clamp(px, radius, size - radius);
                    float iy = Mathf.Clamp(py, radius, size - radius);
                    float dist = Mathf.Sqrt((px - ix) * (px - ix) + (py - iy) * (py - iy));
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - dist + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            roundedBar = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius),
                false);
            roundedBar.name = "hud-bar-mask";
            return roundedBar;
        }

        static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        static Font BuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }
}
