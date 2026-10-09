using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSurvivor
{
    public class MainMenuController : MonoBehaviour
    {
        class MapRow
        {
            public Button Button;
            public Text Index;
            public Text Name;
            public Text Blurb;
            public Text Status;
            public Image Accent;
        }

        class AchRow
        {
            public Image Plate;
            public Text Title;
            public Text Detail;
            public Text Status;
            public Image Fill;
        }

        static readonly Color[] MapColors =
        {
            new Color(0.35f, 0.95f, 1f, 1f),
            new Color(0.72f, 0.45f, 1f, 1f),
            new Color(1f, 0.35f, 0.72f, 1f)
        };

        GameObject homePanel;
        GameObject mapPanel;
        GameObject achievementPanel;
        MapRow[] maps;
        AchRow[] achievements;

        void Awake()
        {
            if (!Application.isPlaying)
                return;

            Loc.Ensure();
            EnsureCamera();
            StartCoroutine(Boot());
        }

        IEnumerator Boot()
        {
            GameObject loading = null;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                loading = BuildLoading();

            yield return NeonArt.Preload();
            if (loading != null)
                Destroy(loading);
            if (transform.Find("UI") == null)
                Build();
            ShowHome();
        }

        GameObject BuildLoading()
        {
            Font font = BuiltinFont();
            GameObject root = new GameObject("Loading", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2100f);
            scaler.matchWidthOrHeight = 0f;
            Text label = CreateText("Label", root.transform, font, 42, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 120f));
            label.text = Loc.Get("menu.loading");
            return root;
        }

        void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }

            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();

            cam.orthographic = true;
            cam.orthographicSize = 21f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NeonVisuals.Background;
            cam.allowHDR = true;
            NeonFactory.PlaceWorldBackdrop(cam, Color.black);
        }

        void ShowHome()
        {
            homePanel.SetActive(true);
            mapPanel.SetActive(false);
            achievementPanel.SetActive(false);
        }

        void ShowMaps()
        {
            RefreshMaps();
            homePanel.SetActive(false);
            mapPanel.SetActive(true);
            achievementPanel.SetActive(false);
        }

        void ShowAchievements()
        {
            RefreshAchievements();
            homePanel.SetActive(false);
            mapPanel.SetActive(false);
            achievementPanel.SetActive(true);
        }

        void RefreshMaps()
        {
            for (int i = 0; i < MapCatalog.Count; i++)
            {
                MapDefinition map = MapCatalog.Maps[i];
                bool open = MapCatalog.IsUnlocked(i);
                MapRow row = maps[i];
                row.Index.text = (i + 1).ToString("00");
                row.Name.text = Loc.Get(map.NameKey);
                row.Blurb.text = Loc.Get(map.BlurbKey);
                if (open)
                    row.Status.text = Loc.Format("menu.best", SaveProfile.BestScore(i));
                else
                {
                    string previous = Loc.Get(MapCatalog.Maps[i - 1].NameKey);
                    row.Status.text = Loc.Get("menu.locked") + "\n" + Loc.Format("menu.need", map.RequiredScore, previous);
                }

                Color accent = open ? MapColors[i] : new Color(0.35f, 0.38f, 0.45f, 1f);
                row.Accent.color = accent;
                row.Index.color = accent;
                row.Name.color = open ? Color.white : new Color(0.75f, 0.78f, 0.84f, 1f);
                row.Status.color = open ? new Color(0.55f, 1f, 0.82f, 1f) : new Color(1f, 0.72f, 0.42f, 1f);
                row.Button.interactable = open;
            }
        }

        void RefreshAchievements()
        {
            AchievementProgress progress = AchievementCatalog.Capture(null);
            for (int i = 0; i < AchievementCatalog.Ids.Length; i++)
            {
                string id = AchievementCatalog.Ids[i];
                int goal = AchievementCatalog.Goal(id);
                int current = Mathf.Clamp(AchievementCatalog.ProgressValue(id, progress), 0, goal);
                bool done = SaveProfile.IsUnlocked(id) || AchievementCatalog.IsMet(id, progress);
                AchRow row = achievements[i];
                row.Title.text = Loc.Get("ach." + id + ".title");
                row.Detail.text = Loc.Format("ach." + id + ".desc", goal);
                row.Status.text = done ? Loc.Get("ach.done") : Loc.Format("ach.progress", current, goal);
                row.Title.color = done ? new Color(0.6f, 1f, 0.84f, 1f) : Color.white;
                row.Status.color = done ? new Color(0.55f, 1f, 0.78f, 1f) : new Color(0.7f, 0.9f, 1f, 1f);
                row.Fill.color = done ? new Color(0.35f, 0.95f, 0.7f, 1f) : new Color(0.3f, 0.85f, 1f, 1f);
                row.Fill.fillAmount = goal <= 0 ? 0f : current / (float)goal;
                row.Plate.color = NeonArt.Panel != null
                    ? Color.white
                    : done
                        ? new Color(0.05f, 0.12f, 0.1f, 0.94f)
                        : new Color(0.05f, 0.07f, 0.12f, 0.94f);
            }
        }

        void Build()
        {
            Font font = BuiltinFont();
            GameObject root = new GameObject("UI", typeof(RectTransform));
            root.transform.SetParent(transform, false);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2100f);
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            homePanel = CreatePanel(root.transform, "Home");
            if (NeonArt.Panel != null)
            {
                Image homePlate = CreateImage("Plate", homePanel.transform, Color.white);
                NeonArt.Paint(homePlate, NeonArt.Panel, true);
                Place(homePlate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(960f, 1120f));
            }

            Text title = CreateText("Title", homePanel.transform, font, 72, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 360f), new Vector2(960f, 160f));
            title.text = "NEON SURVIVOR";
            Outline outline = title.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.05f, 0.35f, 0.55f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            Text hint = CreateText("Hint", homePanel.transform, font, 28, FontStyle.Normal, new Color(0.75f, 0.92f, 1f, 0.9f), TextAnchor.MiddleCenter);
            Place(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(900f, 80f));
            hint.text = Loc.Get("menu.hint");

            Text playLabel;
            Button play = CreateButton(homePanel.transform, "Play", font, 38, out playLabel);
            Place(play.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(860f, 220f));
            playLabel.text = Loc.Get("menu.play");
            play.onClick.AddListener(ShowMaps);

            Text achievementsLabel;
            Button achievementsButton = CreateButton(homePanel.transform, "Achievements", font, 38, out achievementsLabel);
            Place(achievementsButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(860f, 220f));
            achievementsLabel.text = Loc.Get("menu.achievements");
            achievementsButton.onClick.AddListener(ShowAchievements);

            mapPanel = CreatePanel(root.transform, "Maps");
            Text mapTitle = CreateText("Title", mapPanel.transform, font, 52, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(mapTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(960f, 90f));
            mapTitle.text = Loc.Get("menu.maps");
            BuildMapList(mapPanel.transform, font);

            Text mapBackLabel;
            Button mapBack = CreateButton(mapPanel.transform, "Back", font, 32, out mapBackLabel);
            Place(mapBack.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(720f, 250f));
            mapBackLabel.text = Loc.Get("menu.back");
            mapBack.onClick.AddListener(ShowHome);

            achievementPanel = CreatePanel(root.transform, "Achievements");
            Text achievementTitle = CreateText("Title", achievementPanel.transform, font, 52, FontStyle.Bold, new Color(0.6f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            Place(achievementTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(960f, 90f));
            achievementTitle.text = Loc.Get("ach.title");
            BuildAchievementList(achievementPanel.transform, font);

            Text achievementBackLabel;
            Button achievementBack = CreateButton(achievementPanel.transform, "Back", font, 32, out achievementBackLabel);
            Place(achievementBack.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(720f, 250f));
            achievementBackLabel.text = Loc.Get("menu.back");
            achievementBack.onClick.AddListener(ShowHome);
        }

        void BuildMapList(Transform parent, Font font)
        {
            maps = new MapRow[MapCatalog.Count];
            float start = -190f;
            float height = 360f;
            float gap = 32f;
            for (int i = 0; i < MapCatalog.Count; i++)
            {
                int index = i;
                MapRow row = new MapRow();
                Button button = CreateButton(parent, "Map" + i, font, 28, out row.Name);
                RectTransform rect = button.GetComponent<RectTransform>();
                Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, start - i * (height + gap)), new Vector2(960f, height));
                row.Name.gameObject.SetActive(false);

                RectTransform well = new GameObject("Well", typeof(RectTransform)).GetComponent<RectTransform>();
                well.SetParent(button.transform, false);
                well.anchorMin = Vector2.zero;
                well.anchorMax = Vector2.one;
                well.offsetMin = new Vector2(188f, 96f);
                well.offsetMax = new Vector2(-176f, -104f);

                Image accent = CreateImage("Accent", well, MapColors[i]);
                RectTransform accentRect = accent.rectTransform;
                accentRect.anchorMin = new Vector2(0f, 0.12f);
                accentRect.anchorMax = new Vector2(0f, 0.88f);
                accentRect.pivot = new Vector2(0f, 0.5f);
                accentRect.anchoredPosition = new Vector2(0f, 0f);
                accentRect.sizeDelta = new Vector2(8f, 0f);
                row.Accent = accent;

                row.Index = CreateText("Index", well, font, 40, FontStyle.Bold, MapColors[i], TextAnchor.MiddleCenter);
                Place(row.Index.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(62f, 0f), new Vector2(88f, 80f));

                row.Name = CreateText("Name", well, font, 34, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
                Place(row.Name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -6f), new Vector2(430f, 42f));

                row.Blurb = CreateText("Blurb", well, font, 22, FontStyle.Normal, new Color(0.72f, 0.86f, 0.95f, 0.95f), TextAnchor.MiddleLeft);
                Place(row.Blurb.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -50f), new Vector2(430f, 34f));

                row.Status = CreateText("Status", well, font, 22, FontStyle.Bold, new Color(0.55f, 1f, 0.82f, 1f), TextAnchor.UpperLeft);
                Place(row.Status.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(118f, -88f), new Vector2(430f, 70f));
                row.Status.horizontalOverflow = HorizontalWrapMode.Wrap;

                button.onClick.AddListener(delegate { GameSession.Play(index); });
                row.Button = button;
                maps[i] = row;
            }
        }

        void BuildAchievementList(Transform parent, Font font)
        {
            GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(parent, false);
            RectTransform viewport = viewportGo.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(36f, 210f);
            viewport.offsetMax = new Vector2(-36f, -210f);
            viewportGo.AddComponent<RectMask2D>();
            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.sprite = NeonVisuals.White;
            viewportImage.color = new Color(0.01f, 0.02f, 0.04f, 0.35f);

            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            RectTransform content = contentGo.GetComponent<RectTransform>();
            int count = AchievementCatalog.Ids.Length;
            float rowHeight = 340f;
            float gap = 12f;
            float step = rowHeight + gap;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, count * step);

            ScrollRect scroll = viewportGo.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            achievements = new AchRow[count];
            for (int i = 0; i < count; i++)
            {
                AchRow row = new AchRow();
                Image plate = CreateImage("Row" + i, content, new Color(0.05f, 0.07f, 0.12f, 0.94f));
                RectTransform plateRect = plate.rectTransform;
                plateRect.anchorMin = new Vector2(0f, 1f);
                plateRect.anchorMax = new Vector2(1f, 1f);
                plateRect.pivot = new Vector2(0.5f, 1f);
                plateRect.anchoredPosition = new Vector2(0f, -i * step);
                plateRect.sizeDelta = new Vector2(-8f, rowHeight);
                if (NeonArt.Panel != null)
                    NeonArt.Paint(plate, NeonArt.Panel, true);
                row.Plate = plate;

                RectTransform well = new GameObject("Well", typeof(RectTransform)).GetComponent<RectTransform>();
                well.SetParent(plate.transform, false);
                well.anchorMin = Vector2.zero;
                well.anchorMax = Vector2.one;
                well.offsetMin = new Vector2(120f, 108f);
                well.offsetMax = new Vector2(-120f, -112f);

                row.Title = CreateText("Title", well, font, 30, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
                Place(row.Title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -2f), new Vector2(460f, 40f));

                row.Detail = CreateText("Detail", well, font, 22, FontStyle.Normal, new Color(0.75f, 0.88f, 0.96f, 0.95f), TextAnchor.UpperLeft);
                Place(row.Detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -46f), new Vector2(700f, 52f));
                row.Detail.horizontalOverflow = HorizontalWrapMode.Wrap;
                row.Detail.verticalOverflow = VerticalWrapMode.Truncate;

                row.Status = CreateText("Status", well, font, 22, FontStyle.Bold, new Color(0.7f, 0.9f, 1f, 1f), TextAnchor.MiddleRight);
                Place(row.Status.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -2f), new Vector2(210f, 40f));
                row.Status.horizontalOverflow = HorizontalWrapMode.Wrap;

                Image track = CreateImage("Track", well, new Color(1f, 1f, 1f, 0.12f));
                RectTransform trackRect = track.rectTransform;
                trackRect.anchorMin = new Vector2(0f, 0f);
                trackRect.anchorMax = new Vector2(1f, 0f);
                trackRect.pivot = new Vector2(0.5f, 0f);
                trackRect.anchoredPosition = new Vector2(0f, 4f);
                trackRect.sizeDelta = new Vector2(-16f, 16f);

                Image fill = CreateImage("Fill", track.transform, new Color(0.3f, 0.85f, 1f, 1f));
                RectTransform fillRect = fill.rectTransform;
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = (int)Image.OriginHorizontal.Left;
                fill.fillAmount = 0f;
                row.Fill = fill;
                achievements[i] = row;
            }
        }

        static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = NeonVisuals.White;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static Button CreateButton(Transform parent, string name, Font font, int fontSize, out Text label)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = NeonVisuals.White;
            image.color = Color.white;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.06f, 0.09f, 0.15f, 0.96f);
            colors.highlightedColor = new Color(0.1f, 0.28f, 0.38f, 1f);
            colors.pressedColor = new Color(0.22f, 0.75f, 0.88f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.04f, 0.05f, 0.07f, 0.9f);
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            NeonArt.PaintButton(button);
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            label = CreateText("Label", go.transform, font, fontSize, FontStyle.Bold, new Color(0.8f, 1f, 1f, 1f), TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(160f, 78f);
            labelRect.offsetMax = new Vector2(-160f, -78f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            return button;
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
