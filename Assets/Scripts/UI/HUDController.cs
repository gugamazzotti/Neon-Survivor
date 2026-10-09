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
            if (NeonArt.Panel != null)
                NeonArt.Paint(plate, NeonArt.Panel, true);
            else
            {
                plate.sprite = NeonVisuals.White;
                plate.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            }

            Place(plate.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -16f), new Vector2(1032f, 530f));

            RectTransform well = new GameObject("Well", typeof(RectTransform)).GetComponent<RectTransform>();
            well.SetParent(plateGo.transform, false);
            well.anchorMin = Vector2.zero;
            well.anchorMax = Vector2.one;
            well.offsetMin = new Vector2(118f, 108f);
            well.offsetMax = new Vector2(-118f, -116f);

            Text title = CreateText("Title", well, font, 28, FontStyle.Bold, new Color(0.55f, 1f, 1f, 0.9f), TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -2f), new Vector2(760f, 36f));
            title.text = "NEON SURVIVOR";

            levelText = CreateText("Level", well, font, 30, FontStyle.Bold, Color.white, TextAnchor.UpperLeft);
            Place(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -40f), new Vector2(760f, 36f));

            healthFill = CreateBar(well, "Health", new Vector2(8f, -86f), new Vector2(760f, 52f), new Color(1f, 0.28f, 0.48f, 1f));
            xpFill = CreateBar(well, "XP", new Vector2(8f, -148f), new Vector2(760f, 44f), new Color(0.25f, 0.95f, 1f, 1f));

            xpText = CreateText("XpLabel", well, font, 20, FontStyle.Normal, new Color(0.75f, 0.95f, 1f, 0.9f), TextAnchor.UpperLeft);
            Place(xpText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -198f), new Vector2(760f, 28f));

            scoreText = CreateText("Score", well, font, 22, FontStyle.Bold, new Color(0.95f, 0.85f, 0.45f, 0.95f), TextAnchor.UpperLeft);
            Place(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -228f), new Vector2(760f, 32f));

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
            restartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -620f);
            menuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -860f);
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
            if (NeonArt.Panel != null)
                NeonArt.Paint(cardImage, NeonArt.Panel, true);
            else
            {
                cardImage.sprite = NeonVisuals.White;
                cardImage.color = new Color(0.03f, 0.05f, 0.1f, 0.96f);
            }
            Place(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 1120f));

            Text title = CreateText("Heading", card.transform, font, 48, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(700f, 72f));
            title.text = heading;

            Text sub = CreateText("Subtitle", card.transform, font, 24, FontStyle.Normal, new Color(0.8f, 0.9f, 1f, 0.85f), TextAnchor.MiddleCenter);
            Place(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(700f, 64f));
            sub.text = subtitle;

            buttons = new Button[3];
            labels = new Text[3];
            float[] y = { -380f, -620f, -860f };
            for (int i = 0; i < 3; i++)
            {
                buttons[i] = CreateButton(card.transform, "Choice" + i, font, out labels[i]);
                Place(buttons[i].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y[i]), new Vector2(700f, 200f));
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
            labelRect.offsetMin = new Vector2(150f, 70f);
            labelRect.offsetMax = new Vector2(-150f, -70f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.text = Loc.Get("upgrade.placeholder");
            return button;
        }

        static Image CreateBar(Transform parent, string name, Vector2 position, Vector2 size, Color fillColor)
        {
            GameObject background = new GameObject(name + "Bg", typeof(RectTransform));
            background.transform.SetParent(parent, false);
            Image bg = background.AddComponent<Image>();
            bg.sprite = NeonVisuals.White;
            bg.color = new Color(1f, 1f, 1f, 0.12f);
            bg.raycastTarget = false;
            if (NeonArt.Bar != null)
                NeonArt.Paint(bg, NeonArt.Bar, true);

            Place(bg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);

            GameObject fillGo = new GameObject(name + "Fill", typeof(RectTransform));
            fillGo.transform.SetParent(background.transform, false);
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
            fillRect.offsetMin = NeonArt.Bar != null ? new Vector2(36f, 16f) : Vector2.zero;
            fillRect.offsetMax = NeonArt.Bar != null ? new Vector2(-36f, -16f) : Vector2.zero;
            return fill;
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
