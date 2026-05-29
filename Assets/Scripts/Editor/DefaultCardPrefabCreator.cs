using System.IO;
using CardBattle.Gameplay;
using CardBattle.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace CardBattle.Editor
{
    public static class DefaultCardPrefabCreator
    {
        private const string PrefabPath = "Assets/Prefabs/UI/CardView.prefab";
        private const string DefaultFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string KoreanFontAssetPath = "Assets/Fonts/MalgunGothic SDF.asset";
        private const string KoreanSystemFontPath = "C:/Windows/Fonts/malgun.ttf";

        private static TMP_FontAsset cachedDefaultFontAsset;
        private static TMP_FontAsset cachedKoreanFontAsset;

        [MenuItem("Tools/기본 카드 프리팹 생성")]
        public static void CreateDefaultCardPrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Play Mode 중에는 기본 카드 프리팹을 생성할 수 없습니다. Play Mode를 종료한 뒤 다시 실행해주세요.");
                return;
            }

            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "UI");

            var cardPrefab = CreateCardPrefab();
            var canvas = EnsureCanvas();
            EnsureEventSystem();
            var handRoot = EnsureHandRoot(canvas.transform);
            var fieldRoot = EnsureFieldRoot(canvas.transform);
            var trapZoneRoot = EnsureTrapZoneRoot(canvas.transform);
            var previewRoot = EnsurePreviewRoot(canvas.transform);
            var fieldManager = EnsureFieldManager(canvas.transform, fieldRoot, cardPrefab);
            var trapZoneManager = EnsureTrapZoneManager(canvas.transform, trapZoneRoot);
            var previewManager = EnsureCardPreviewManager(canvas.transform, previewRoot, cardPrefab);
            var handManager = EnsureHandManager(canvas.transform, handRoot, cardPrefab, previewManager);
            var actionPanel = EnsureActionPanel(canvas.transform);
            var cardActionManager = EnsureCardActionManager(canvas.transform, actionPanel, handManager, trapZoneManager);
            var deckPilePanel = EnsureDeckPilePanel(canvas.transform);
            var deckView = EnsureDeckView(deckPilePanel);
            var graveyardPanel = EnsureGraveyardPanel(canvas.transform);
            var graveyardView = EnsureGraveyardView(graveyardPanel);
            var gameLogPanel = EnsureGameLogPanel(canvas.transform);
            EnsureGameLogManager(canvas.transform, gameLogPanel);
            var deckManager = EnsureDeckManager(handManager, fieldManager, trapZoneManager, cardActionManager, deckView, graveyardView);
            EnsureTurnManager(canvas.transform, deckManager);

            Selection.activeObject = cardPrefab;
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

            Debug.Log(
                $"기본 카드 프리팹을 생성하고 {handManager.name}에 연결했습니다: {PrefabPath}",
                cardPrefab);
        }

        [MenuItem("Tools/기본 카드 프리팹 생성", true)]
        private static bool CanCreateDefaultCardPrefab()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        [MenuItem("Tools/한글 TMP 폰트 생성")]
        public static void CreateKoreanTmpFontAsset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Play Mode 중에는 한글 TMP 폰트를 생성할 수 없습니다. Play Mode를 종료한 뒤 다시 실행해주세요.");
                return;
            }

            var fontAsset = EnsureKoreanFontAsset();
            if (fontAsset != null)
            {
                Selection.activeObject = fontAsset;
                Debug.Log($"한글 TMP 폰트를 준비했습니다: {KoreanFontAssetPath}", fontAsset);
            }
        }

        [MenuItem("Tools/한글 TMP 폰트 생성", true)]
        private static bool CanCreateKoreanTmpFontAsset()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static GameObject CreateCardPrefab()
        {
            var card = CreateUiObject("CardView", new Vector2(210f, 300f));
            var cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.preferredWidth = 210f;
            cardLayout.preferredHeight = 300f;

            var sortingCanvas = card.AddComponent<Canvas>();
            sortingCanvas.overrideSorting = false;
            sortingCanvas.sortingOrder = 0;
            card.AddComponent<GraphicRaycaster>();

            var background = card.AddComponent<Image>();
            background.color = new Color(0.96f, 0.91f, 0.82f);

            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var cardView = card.AddComponent<CardView>();

            var header = CreatePanel("Header", card.transform, new Color(0.18f, 0.12f, 0.09f, 0.92f));
            AddHorizontalLayout(header, new RectOffset(7, 7, 4, 4), 6f, TextAnchor.MiddleCenter);
            SetPreferredHeight(header, 38f);

            var nameText = CreateText("NameText", header.transform, "Card Name", 16, FontStyles.Bold, TextAlignmentOptions.Left);
            nameText.color = Color.white;
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1f;

            var costOrLevelText = CreateText("CostOrLevelText", header.transform, "LV 1", 13, FontStyles.Bold, TextAlignmentOptions.Center);
            costOrLevelText.color = Color.white;
            var costLayout = costOrLevelText.gameObject.AddComponent<LayoutElement>();
            costLayout.preferredWidth = 36f;

            var typeBanner = CreatePanel("TypeBanner", card.transform, new Color(0.86f, 0.51f, 0.28f));
            SetPreferredHeight(typeBanner, 24f);
            var typeText = CreateText("TypeText", typeBanner.transform, "Monster", 14, FontStyles.Bold, TextAlignmentOptions.Center);
            StretchToParent(typeText.rectTransform, new RectOffset(6, 6, 2, 2));
            typeText.color = Color.white;

            var artFrame = CreatePanel("ArtFrame", card.transform, new Color(0.82f, 0.78f, 0.65f));
            SetPreferredHeight(artFrame, 92f);
            var artworkImage = CreateArtworkImage(artFrame.transform);
            var artLabel = CreateText("ArtPlaceholderText", artFrame.transform, "ART", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            StretchToParent(artLabel.rectTransform, new RectOffset(0, 0, 0, 0));
            artLabel.color = new Color(0.35f, 0.31f, 0.25f);

            var descriptionBox = CreatePanel("DescriptionBox", card.transform, new Color(1f, 0.98f, 0.9f));
            SetPreferredHeight(descriptionBox, 76f);
            var descriptionText = CreateText("DescriptionText", descriptionBox.transform, "Card description.", 10, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            StretchToParent(descriptionText.rectTransform, new RectOffset(7, 7, 6, 6));

            var statsRow = CreateUiObject("StatsRow", Vector2.zero);
            statsRow.transform.SetParent(card.transform, false);
            AddHorizontalLayout(statsRow, new RectOffset(0, 0, 0, 0), 4f, TextAnchor.MiddleCenter);
            SetPreferredHeight(statsRow, 24f);

            var attackText = CreateText("AttackText", statsRow.transform, "ATK 1000", 11, FontStyles.Bold, TextAlignmentOptions.Left);
            var defenseText = CreateText("DefenseText", statsRow.transform, "DEF 1000", 11, FontStyles.Bold, TextAlignmentOptions.Right);
            var attackLayout = attackText.gameObject.AddComponent<LayoutElement>();
            attackLayout.preferredWidth = 90f;
            attackLayout.flexibleWidth = 1f;
            var defenseLayout = defenseText.gameObject.AddComponent<LayoutElement>();
            defenseLayout.preferredWidth = 90f;
            defenseLayout.flexibleWidth = 1f;

            AssignCardViewFields(cardView, background, typeBanner.GetComponent<Image>(), artworkImage, outline, sortingCanvas, nameText, typeText, costOrLevelText, attackText, defenseText, artLabel, descriptionText);

            var prefab = PrefabUtility.SaveAsPrefabAsset(card, PrefabPath);
            Object.DestroyImmediate(card);
            return prefab;
        }

        private static Canvas EnsureCanvas()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }

            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Canvas 생성");

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "EventSystem 생성");
        }

        private static Transform EnsureHandRoot(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("HandRoot");
            if (existing != null)
            {
                ConfigureHandRoot(existing.gameObject);
                return existing;
            }

            var handRoot = CreateUiObject("HandRoot", Vector2.zero);
            Undo.RegisterCreatedObjectUndo(handRoot, "HandRoot 생성");
            handRoot.transform.SetParent(canvasTransform, false);

            ConfigureHandRoot(handRoot);
            return handRoot.transform;
        }

        private static void ConfigureHandRoot(GameObject handRoot)
        {
            var rectTransform = handRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 8f);
            rectTransform.sizeDelta = new Vector2(1160f, 260f);

            var layout = handRoot.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = handRoot.AddComponent<HorizontalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 12f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static Transform EnsureFieldRoot(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("MonsterFieldRoot");
            if (existing != null)
            {
                ConfigureFieldRoot(existing.gameObject);
                EnsureFieldSlots(existing);
                return existing;
            }

            var fieldRoot = CreateUiObject("MonsterFieldRoot", Vector2.zero);
            Undo.RegisterCreatedObjectUndo(fieldRoot, "몬스터 필드 루트 생성");
            fieldRoot.transform.SetParent(canvasTransform, false);

            ConfigureFieldRoot(fieldRoot);
            EnsureFieldSlots(fieldRoot.transform);
            return fieldRoot.transform;
        }

        private static void ConfigureFieldRoot(GameObject fieldRoot)
        {
            var rectTransform = fieldRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-114f, 270f);
            rectTransform.sizeDelta = new Vector2(1160f, 330f);

            var layout = fieldRoot.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = fieldRoot.AddComponent<HorizontalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 18f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static void EnsureFieldSlots(Transform fieldRoot)
        {
            for (var i = 0; i < 5; i++)
            {
                var slotName = $"MonsterSlot {i + 1}";
                var slotTransform = fieldRoot.Find(slotName);
                var slotObject = slotTransform != null
                    ? slotTransform.gameObject
                    : CreateFieldSlot(slotName, fieldRoot, i + 1);

                ConfigureFieldSlot(slotObject, i + 1);
            }
        }

        private static GameObject CreateFieldSlot(string slotName, Transform parent, int slotNumber)
        {
            var slot = CreateUiObject(slotName, new Vector2(210f, 300f));
            Undo.RegisterCreatedObjectUndo(slot, "몬스터 슬롯 생성");
            slot.transform.SetParent(parent, false);
            ConfigureFieldSlot(slot, slotNumber);
            return slot;
        }

        private static void ConfigureFieldSlot(GameObject slot, int slotNumber)
        {
            var rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(210f, 300f);

            var image = slot.GetComponent<Image>();
            if (image == null)
            {
                image = slot.AddComponent<Image>();
            }

            image.color = new Color(0.08f, 0.12f, 0.16f, 0.58f);

            var outline = slot.GetComponent<Outline>();
            if (outline == null)
            {
                outline = slot.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.72f, 0.78f, 0.82f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);

            var layoutElement = slot.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = slot.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = 210f;
            layoutElement.preferredHeight = 300f;

            var fieldSlot = slot.GetComponent<FieldSlot>();
            if (fieldSlot == null)
            {
                fieldSlot = slot.AddComponent<FieldSlot>();
            }

            var label = slot.transform.Find("SlotLabel")?.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = CreateText("SlotLabel", slot.transform, $"MONSTER {slotNumber}", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            }

            label.text = $"MONSTER {slotNumber}";
            ApplyUiFont(label);

            StretchToParent(label.rectTransform, new RectOffset(8, 8, 8, 8));
            label.color = new Color(0.78f, 0.84f, 0.9f, 0.9f);

            var serializedSlot = new SerializedObject(fieldSlot);
            serializedSlot.FindProperty("backgroundImage").objectReferenceValue = image;
            serializedSlot.FindProperty("labelText").objectReferenceValue = label;
            serializedSlot.ApplyModifiedProperties();
        }

        private static Transform EnsureTrapZoneRoot(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("TrapZoneRoot");
            if (existing != null)
            {
                ConfigureTrapZoneRoot(existing.gameObject);
                EnsureTrapSlots(existing);
                return existing;
            }

            var trapZoneRoot = CreateUiObject("TrapZoneRoot", Vector2.zero);
            Undo.RegisterCreatedObjectUndo(trapZoneRoot, "함정 존 루트 생성");
            trapZoneRoot.transform.SetParent(canvasTransform, false);

            ConfigureTrapZoneRoot(trapZoneRoot);
            EnsureTrapSlots(trapZoneRoot.transform);
            return trapZoneRoot.transform;
        }

        private static void ConfigureTrapZoneRoot(GameObject trapZoneRoot)
        {
            var rectTransform = trapZoneRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-114f, -45f);
            rectTransform.sizeDelta = new Vector2(1160f, 330f);

            var layout = trapZoneRoot.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = trapZoneRoot.AddComponent<HorizontalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 18f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static void EnsureTrapSlots(Transform trapZoneRoot)
        {
            for (var i = 0; i < 5; i++)
            {
                var slotName = $"TrapSlot {i + 1}";
                var slotTransform = trapZoneRoot.Find(slotName);
                var slotObject = slotTransform != null
                    ? slotTransform.gameObject
                    : CreateTrapSlot(slotName, trapZoneRoot, i + 1);

                ConfigureTrapSlot(slotObject, i + 1);
            }
        }

        private static GameObject CreateTrapSlot(string slotName, Transform parent, int slotNumber)
        {
            var slot = CreateUiObject(slotName, new Vector2(210f, 300f));
            Undo.RegisterCreatedObjectUndo(slot, "함정 슬롯 생성");
            slot.transform.SetParent(parent, false);
            ConfigureTrapSlot(slot, slotNumber);
            return slot;
        }

        private static void ConfigureTrapSlot(GameObject slot, int slotNumber)
        {
            var rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(210f, 300f);

            var image = slot.GetComponent<Image>();
            if (image == null)
            {
                image = slot.AddComponent<Image>();
            }

            image.color = new Color(0.1f, 0.08f, 0.16f, 0.72f);

            var outline = slot.GetComponent<Outline>();
            if (outline == null)
            {
                outline = slot.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.78f, 0.56f, 0.82f, 0.78f);
            outline.effectDistance = new Vector2(2f, -2f);

            var layoutElement = slot.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = slot.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = 210f;
            layoutElement.preferredHeight = 300f;

            var trapSlot = slot.GetComponent<TrapSlot>();
            if (trapSlot == null)
            {
                trapSlot = slot.AddComponent<TrapSlot>();
            }

            var label = slot.transform.Find("SlotLabel")?.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = CreateText("SlotLabel", slot.transform, $"TRAP {slotNumber}", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            }

            label.text = "TRAP ZONE";
            ApplyUiFont(label);

            StretchToParent(label.rectTransform, new RectOffset(12, 12, 12, 12));
            label.color = new Color(0.92f, 0.86f, 0.96f);
            label.gameObject.SetActive(true);

            var cardBackRoot = CardBackVisual.Ensure(slot.transform, 10f);
            cardBackRoot.SetActive(false);

            var serializedSlot = new SerializedObject(trapSlot);
            serializedSlot.FindProperty("backgroundImage").objectReferenceValue = image;
            serializedSlot.FindProperty("labelText").objectReferenceValue = label;
            serializedSlot.ApplyModifiedProperties();
        }

        private static Transform EnsurePreviewRoot(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("CardPreviewRoot");
            if (existing != null)
            {
                ConfigurePreviewRoot(existing.gameObject);
                return existing;
            }

            var previewRoot = CreatePanel("CardPreviewRoot", canvasTransform, new Color(0.04f, 0.05f, 0.07f, 0.72f));
            Undo.RegisterCreatedObjectUndo(previewRoot, "카드 프리뷰 루트 생성");
            ConfigurePreviewRoot(previewRoot);
            return previewRoot.transform;
        }

        private static void ConfigurePreviewRoot(GameObject previewRoot)
        {
            var rectTransform = previewRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-28f, -40f);
            rectTransform.sizeDelta = new Vector2(292f, 410f);

            var image = previewRoot.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }

        private static CardPreviewManager EnsureCardPreviewManager(Transform canvasTransform, Transform previewRoot, GameObject cardPrefab)
        {
            var previewManager = Object.FindAnyObjectByType<CardPreviewManager>();
            if (previewManager == null)
            {
                var previewManagerObject = new GameObject("CardPreviewManager");
                Undo.RegisterCreatedObjectUndo(previewManagerObject, "CardPreviewManager 생성");
                previewManagerObject.transform.SetParent(canvasTransform, false);
                previewManager = previewManagerObject.AddComponent<CardPreviewManager>();
            }

            var serializedObject = new SerializedObject(previewManager);
            serializedObject.FindProperty("previewRoot").objectReferenceValue = previewRoot;
            serializedObject.FindProperty("cardViewPrefab").objectReferenceValue = cardPrefab.GetComponent<CardView>();
            serializedObject.FindProperty("previewScale").floatValue = 1.25f;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(previewManager);
            return previewManager;
        }

        private static HandManager EnsureHandManager(
            Transform canvasTransform,
            Transform handRoot,
            GameObject cardPrefab,
            CardPreviewManager previewManager)
        {
            var handManager = Object.FindAnyObjectByType<HandManager>();
            if (handManager == null)
            {
                var handManagerObject = new GameObject("HandManager");
                Undo.RegisterCreatedObjectUndo(handManagerObject, "HandManager 생성");
                handManagerObject.transform.SetParent(canvasTransform, false);
                handManager = handManagerObject.AddComponent<HandManager>();
            }

            var serializedObject = new SerializedObject(handManager);
            serializedObject.FindProperty("handRoot").objectReferenceValue = handRoot;
            serializedObject.FindProperty("cardViewPrefab").objectReferenceValue = cardPrefab.GetComponent<CardView>();
            serializedObject.FindProperty("cardPreviewManager").objectReferenceValue = previewManager;
            serializedObject.FindProperty("maxVisibleCards").intValue = 10;
            serializedObject.FindProperty("cardWidth").floatValue = 210f;
            serializedObject.FindProperty("defaultSpacing").floatValue = 12f;
            serializedObject.FindProperty("minimumCardScale").floatValue = 0.72f;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(handManager);
            return handManager;
        }

        private static FieldManager EnsureFieldManager(Transform canvasTransform, Transform fieldRoot, GameObject cardPrefab)
        {
            var fieldManager = Object.FindAnyObjectByType<FieldManager>();
            if (fieldManager == null)
            {
                var fieldManagerObject = new GameObject("FieldManager");
                Undo.RegisterCreatedObjectUndo(fieldManagerObject, "FieldManager 생성");
                fieldManagerObject.transform.SetParent(canvasTransform, false);
                fieldManager = fieldManagerObject.AddComponent<FieldManager>();
            }

            var serializedObject = new SerializedObject(fieldManager);
            serializedObject.FindProperty("cardViewPrefab").objectReferenceValue = cardPrefab.GetComponent<CardView>();

            var slotsProperty = serializedObject.FindProperty("fieldSlots");
            slotsProperty.arraySize = 5;
            for (var i = 0; i < 5; i++)
            {
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                    fieldRoot.Find($"MonsterSlot {i + 1}")?.GetComponent<FieldSlot>();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(fieldManager);
            return fieldManager;
        }

        private static TrapZoneManager EnsureTrapZoneManager(Transform canvasTransform, Transform trapZoneRoot)
        {
            var trapZoneManager = Object.FindAnyObjectByType<TrapZoneManager>();
            if (trapZoneManager == null)
            {
                var trapZoneManagerObject = new GameObject("TrapZoneManager");
                Undo.RegisterCreatedObjectUndo(trapZoneManagerObject, "TrapZoneManager 생성");
                trapZoneManagerObject.transform.SetParent(canvasTransform, false);
                trapZoneManager = trapZoneManagerObject.AddComponent<TrapZoneManager>();
            }

            var serializedObject = new SerializedObject(trapZoneManager);
            var slotsProperty = serializedObject.FindProperty("trapSlots");
            slotsProperty.arraySize = 5;
            for (var i = 0; i < 5; i++)
            {
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                    trapZoneRoot.Find($"TrapSlot {i + 1}")?.GetComponent<TrapSlot>();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(trapZoneManager);
            return trapZoneManager;
        }

        private static CardActionManager EnsureCardActionManager(
            Transform canvasTransform,
            Transform actionPanel,
            HandManager handManager,
            TrapZoneManager trapZoneManager)
        {
            var cardActionManager = Object.FindAnyObjectByType<CardActionManager>();
            if (cardActionManager == null)
            {
                var cardActionObject = new GameObject("CardActionManager");
                Undo.RegisterCreatedObjectUndo(cardActionObject, "CardActionManager 생성");
                cardActionObject.transform.SetParent(canvasTransform, false);
                cardActionManager = cardActionObject.AddComponent<CardActionManager>();
            }

            var useButton = actionPanel.Find("UseCardButton")?.GetComponent<Button>();
            var setTrapButton = actionPanel.Find("SetTrapButton")?.GetComponent<Button>();

            var serializedObject = new SerializedObject(cardActionManager);
            serializedObject.FindProperty("useCardButton").objectReferenceValue = useButton;
            serializedObject.FindProperty("setTrapButton").objectReferenceValue = setTrapButton;
            serializedObject.ApplyModifiedProperties();

            ConfigureCardActionDropTarget(useButton, cardActionManager, CardActionDropTarget.DropAction.UseSpell);
            ConfigureCardActionDropTarget(setTrapButton, cardActionManager, CardActionDropTarget.DropAction.SetTrap);

            EditorUtility.SetDirty(cardActionManager);
            return cardActionManager;
        }

        private static void ConfigureCardActionDropTarget(
            Button button,
            CardActionManager cardActionManager,
            CardActionDropTarget.DropAction dropAction)
        {
            if (button == null)
            {
                return;
            }

            var dropTarget = button.GetComponent<CardActionDropTarget>();
            if (dropTarget == null)
            {
                dropTarget = button.gameObject.AddComponent<CardActionDropTarget>();
            }

            var serializedObject = new SerializedObject(dropTarget);
            serializedObject.FindProperty("cardActionManager").objectReferenceValue = cardActionManager;
            serializedObject.FindProperty("dropAction").enumValueIndex = (int)dropAction;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(dropTarget);
        }

        private static DeckManager EnsureDeckManager(
            HandManager handManager,
            FieldManager fieldManager,
            TrapZoneManager trapZoneManager,
            CardActionManager cardActionManager,
            DeckView deckView,
            GraveyardView graveyardView)
        {
            var deckManager = Object.FindAnyObjectByType<DeckManager>();
            if (deckManager == null)
            {
                var deckManagerObject = new GameObject("DeckManager");
                Undo.RegisterCreatedObjectUndo(deckManagerObject, "DeckManager 생성");
                deckManager = deckManagerObject.AddComponent<DeckManager>();
            }

            var serializedObject = new SerializedObject(deckManager);
            serializedObject.FindProperty("handManager").objectReferenceValue = handManager;
            serializedObject.FindProperty("fieldManager").objectReferenceValue = fieldManager;
            serializedObject.FindProperty("trapZoneManager").objectReferenceValue = trapZoneManager;
            serializedObject.FindProperty("cardActionManager").objectReferenceValue = cardActionManager;
            serializedObject.FindProperty("deckView").objectReferenceValue = deckView;
            serializedObject.FindProperty("graveyardView").objectReferenceValue = graveyardView;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(deckManager);
            return deckManager;
        }

        private static TurnManager EnsureTurnManager(Transform canvasTransform, DeckManager deckManager)
        {
            var turnPanel = EnsureTurnPanel(canvasTransform);
            var turnManager = Object.FindAnyObjectByType<TurnManager>();
            if (turnManager == null)
            {
                var turnManagerObject = new GameObject("TurnManager");
                Undo.RegisterCreatedObjectUndo(turnManagerObject, "TurnManager 생성");
                turnManagerObject.transform.SetParent(canvasTransform, false);
                turnManager = turnManagerObject.AddComponent<TurnManager>();
            }

            var nextTurnButton = turnPanel.Find("NextTurnButton")?.GetComponent<Button>();
            var startRestartButton = turnPanel.Find("StartRestartButton")?.GetComponent<Button>();
            var turnText = turnPanel.Find("TurnText")?.GetComponent<TMP_Text>();
            var phaseText = turnPanel.Find("PhaseText")?.GetComponent<TMP_Text>();
            var deckCountText = turnPanel.Find("DeckCountText")?.GetComponent<TMP_Text>();

            var serializedObject = new SerializedObject(turnManager);
            serializedObject.FindProperty("deckManager").objectReferenceValue = deckManager;
            serializedObject.FindProperty("nextTurnButton").objectReferenceValue = nextTurnButton;
            serializedObject.FindProperty("startRestartButton").objectReferenceValue = startRestartButton;
            serializedObject.FindProperty("turnText").objectReferenceValue = turnText;
            serializedObject.FindProperty("phaseText").objectReferenceValue = phaseText;
            serializedObject.FindProperty("deckCountText").objectReferenceValue = deckCountText;
            serializedObject.ApplyModifiedProperties();

            if (nextTurnButton != null)
            {
                EditorUtility.SetDirty(nextTurnButton);
            }

            if (startRestartButton != null)
            {
                EditorUtility.SetDirty(startRestartButton);
            }

            EditorUtility.SetDirty(turnManager);
            return turnManager;
        }

        private static Transform EnsureTurnPanel(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("TurnPanel");
            if (existing != null)
            {
                ConfigureTurnPanel(existing.gameObject);
                EnsureTurnPanelChildren(existing);
                return existing;
            }

            var turnPanel = CreatePanel("TurnPanel", canvasTransform, new Color(0.07f, 0.09f, 0.12f, 0.82f));
            Undo.RegisterCreatedObjectUndo(turnPanel, "턴 패널 생성");
            ConfigureTurnPanel(turnPanel);
            EnsureTurnPanelChildren(turnPanel.transform);
            return turnPanel.transform;
        }

        private static Transform EnsureActionPanel(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("ActionPanel");
            if (existing != null)
            {
                ConfigureActionPanel(existing.gameObject);
                EnsureActionPanelChildren(existing);
                return existing;
            }

            var actionPanel = CreatePanel("ActionPanel", canvasTransform, new Color(0.07f, 0.09f, 0.12f, 0.82f));
            Undo.RegisterCreatedObjectUndo(actionPanel, "액션 패널 생성");
            ConfigureActionPanel(actionPanel);
            EnsureActionPanelChildren(actionPanel.transform);
            return actionPanel.transform;
        }

        private static void ConfigureActionPanel(GameObject actionPanel)
        {
            var rectTransform = actionPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-28f, -258f);
            rectTransform.sizeDelta = new Vector2(230f, 112f);

            var layout = actionPanel.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = actionPanel.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void EnsureActionPanelChildren(Transform actionPanel)
        {
            var useCardButton = actionPanel.Find("UseCardButton")?.GetComponent<Button>();
            if (useCardButton == null)
            {
                useCardButton = CreateButton("UseCardButton", actionPanel, "Use Card");
            }

            SetButtonLabel(useCardButton, "Use Card");

            SetOrUpdatePreferredHeight(useCardButton.gameObject, 40f);

            var setTrapButton = actionPanel.Find("SetTrapButton")?.GetComponent<Button>();
            if (setTrapButton == null)
            {
                setTrapButton = CreateButton("SetTrapButton", actionPanel, "Set Trap");
            }

            SetButtonLabel(setTrapButton, "Set Trap");

            SetOrUpdatePreferredHeight(setTrapButton.gameObject, 40f);
        }

        private static Transform EnsureDeckPilePanel(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("DeckPilePanel");
            if (existing != null)
            {
                ConfigurePileCardRoot(
                    existing.gameObject,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(570f, -45f),
                    new Color(0.16f, 0.22f, 0.31f, 0.96f));
                EnsureDeckPilePanelChildren(existing);
                return existing;
            }

            var deckPilePanel = CreatePanel("DeckPilePanel", canvasTransform, new Color(0.16f, 0.22f, 0.31f, 0.96f));
            Undo.RegisterCreatedObjectUndo(deckPilePanel, "Deck Pile Panel 생성");
            ConfigurePileCardRoot(
                deckPilePanel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(570f, -45f),
                new Color(0.16f, 0.22f, 0.31f, 0.96f));
            EnsureDeckPilePanelChildren(deckPilePanel.transform);
            return deckPilePanel.transform;
        }

        private static void EnsureDeckPilePanelChildren(Transform deckPilePanel)
        {
            EnsureDeckCardBackArt(deckPilePanel);

            var titleText = EnsurePileText(deckPilePanel, "TitleText", "DECK", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigurePileTitle(titleText);
            titleText.gameObject.SetActive(false);

            var countText = EnsurePileText(deckPilePanel, "CountText", "0", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigureDeckPileCount(countText);
            countText.gameObject.SetActive(false);

            var hoverPanel = EnsureHoverPanel(deckPilePanel, "DeckHoverPanel", new Vector2(250f, 96f), false);
            var hoverText = EnsurePileText(hoverPanel.transform, "HoverText", "Remaining Cards\n0", 16, FontStyles.Bold, TextAlignmentOptions.Center);
            hoverText.color = Color.white;
            hoverText.raycastTarget = false;
            hoverText.textWrappingMode = TextWrappingModes.Normal;
            StretchToParent(hoverText.rectTransform, new RectOffset(10, 10, 8, 8));
            hoverPanel.SetActive(false);
        }

        private static void EnsureDeckCardBackArt(Transform deckPilePanel)
        {
            CardBackVisual.Ensure(deckPilePanel, 10f);
        }

        private static DeckView EnsureDeckView(Transform deckPilePanel)
        {
            var deckView = deckPilePanel.GetComponent<DeckView>();
            if (deckView == null)
            {
                deckView = deckPilePanel.gameObject.AddComponent<DeckView>();
            }

            var hoverPanel = deckPilePanel.Find("DeckHoverPanel")?.gameObject;
            var hoverText = hoverPanel != null ? hoverPanel.transform.Find("HoverText")?.GetComponent<TMP_Text>() : null;

            var serializedObject = new SerializedObject(deckView);
            serializedObject.FindProperty("backgroundImage").objectReferenceValue = deckPilePanel.GetComponent<Image>();
            serializedObject.FindProperty("titleText").objectReferenceValue = deckPilePanel.Find("TitleText")?.GetComponent<TMP_Text>();
            serializedObject.FindProperty("countText").objectReferenceValue = deckPilePanel.Find("CountText")?.GetComponent<TMP_Text>();
            serializedObject.FindProperty("hoverPanel").objectReferenceValue = hoverPanel;
            serializedObject.FindProperty("hoverText").objectReferenceValue = hoverText;
            serializedObject.FindProperty("cardsDrawnPerClick").intValue = 1;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(deckView);
            return deckView;
        }

        private static Transform EnsureGraveyardPanel(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("GraveyardPanel");
            if (existing != null)
            {
                ConfigureGraveyardPanel(existing.gameObject);
                EnsureGraveyardPanelChildren(existing);
                return existing;
            }

            var graveyardPanel = CreatePanel("GraveyardPanel", canvasTransform, new Color(0.07f, 0.09f, 0.12f, 0.82f));
            Undo.RegisterCreatedObjectUndo(graveyardPanel, "Graveyard Panel 생성");
            ConfigureGraveyardPanel(graveyardPanel);
            EnsureGraveyardPanelChildren(graveyardPanel.transform);
            return graveyardPanel.transform;
        }

        private static void ConfigureGraveyardPanel(GameObject graveyardPanel)
        {
            ConfigurePileCardRoot(
                graveyardPanel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(570f, 270f),
                new Color(0.27f, 0.22f, 0.28f, 0.96f));
        }

        private static void EnsureGraveyardPanelChildren(Transform graveyardPanel)
        {
            var graveyardButton = graveyardPanel.Find("GraveyardButton")?.GetComponent<Button>();
            if (graveyardButton != null)
            {
                graveyardButton.gameObject.SetActive(false);
            }

            var titleText = EnsurePileText(graveyardPanel, "TitleText", "GRAVEYARD", 17, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigurePileTitle(titleText);

            var countText = EnsurePileText(graveyardPanel, "CountText", "0", 58, FontStyles.Bold, TextAlignmentOptions.Center);
            ConfigurePileCount(countText);

            var listPanel = graveyardPanel.Find("GraveyardListPanel")?.gameObject;
            if (listPanel == null)
            {
                listPanel = CreatePanel("GraveyardListPanel", graveyardPanel, new Color(0.03f, 0.04f, 0.06f, 0.94f));
            }

            ConfigureHoverPanel(listPanel, new Vector2(300f, 260f), true);

            var listText = listPanel.transform.Find("ListText")?.GetComponent<TextMeshProUGUI>();
            if (listText == null)
            {
                listText = CreateText("ListText", listPanel.transform, "Cards: 0\nEmpty", 13, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            }

            listText.text = "Cards: 0\nEmpty";
            ApplyUiFont(listText);
            listText.textWrappingMode = TextWrappingModes.Normal;
            listText.color = Color.white;
            listText.raycastTarget = false;
            StretchToParent(listText.rectTransform, new RectOffset(8, 8, 6, 6));
            listPanel.SetActive(false);
        }

        private static GraveyardView EnsureGraveyardView(Transform graveyardPanel)
        {
            var graveyardView = graveyardPanel.GetComponent<GraveyardView>();
            if (graveyardView == null)
            {
                graveyardView = graveyardPanel.gameObject.AddComponent<GraveyardView>();
            }

            var listPanel = graveyardPanel.Find("GraveyardListPanel")?.gameObject;
            var listText = listPanel != null ? listPanel.transform.Find("ListText")?.GetComponent<TMP_Text>() : null;

            var serializedObject = new SerializedObject(graveyardView);
            serializedObject.FindProperty("backgroundImage").objectReferenceValue = graveyardPanel.GetComponent<Image>();
            serializedObject.FindProperty("titleText").objectReferenceValue = graveyardPanel.Find("TitleText")?.GetComponent<TMP_Text>();
            serializedObject.FindProperty("countText").objectReferenceValue = graveyardPanel.Find("CountText")?.GetComponent<TMP_Text>();
            serializedObject.FindProperty("listPanel").objectReferenceValue = listPanel;
            serializedObject.FindProperty("listText").objectReferenceValue = listText;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(graveyardView);
            return graveyardView;
        }

        private static Transform EnsureGameLogPanel(Transform canvasTransform)
        {
            var existing = canvasTransform.Find("GameLogPanel");
            if (existing != null)
            {
                ConfigureGameLogPanel(existing.gameObject);
                EnsureGameLogPanelChildren(existing);
                return existing;
            }

            var logPanel = CreatePanel("GameLogPanel", canvasTransform, new Color(0.04f, 0.05f, 0.07f, 0.82f));
            Undo.RegisterCreatedObjectUndo(logPanel, "게임 로그 패널 생성");
            ConfigureGameLogPanel(logPanel);
            EnsureGameLogPanelChildren(logPanel.transform);
            return logPanel.transform;
        }

        private static void ConfigureGameLogPanel(GameObject logPanel)
        {
            var rectTransform = logPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = new Vector2(28f, 28f);
            rectTransform.sizeDelta = new Vector2(340f, 156f);

            var image = logPanel.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.04f, 0.05f, 0.07f, 0.82f);
                image.raycastTarget = false;
            }

            var outline = logPanel.GetComponent<Outline>();
            if (outline == null)
            {
                outline = logPanel.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.76f, 0.8f, 0.86f, 0.55f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void EnsureGameLogPanelChildren(Transform logPanel)
        {
            var titleText = logPanel.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
            {
                titleText = CreateText("TitleText", logPanel, "LOG", 14, FontStyles.Bold, TextAlignmentOptions.Left);
            }

            titleText.text = "LOG";
            ApplyUiFont(titleText);
            titleText.color = new Color(0.96f, 0.82f, 0.54f);
            titleText.raycastTarget = false;

            var titleRectTransform = titleText.rectTransform;
            titleRectTransform.anchorMin = new Vector2(0f, 1f);
            titleRectTransform.anchorMax = new Vector2(1f, 1f);
            titleRectTransform.pivot = new Vector2(0.5f, 1f);
            titleRectTransform.anchoredPosition = new Vector2(0f, -8f);
            titleRectTransform.sizeDelta = new Vector2(-20f, 22f);

            var logText = logPanel.Find("LogText")?.GetComponent<TextMeshProUGUI>();
            if (logText == null)
            {
                logText = CreateText("LogText", logPanel, "Log", 12, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            }

            logText.text = "Log";
            ApplyUiFont(logText);
            logText.color = new Color(0.9f, 0.94f, 1f);
            logText.textWrappingMode = TextWrappingModes.Normal;
            logText.overflowMode = TextOverflowModes.Truncate;
            logText.raycastTarget = false;
            StretchToParent(logText.rectTransform, new RectOffset(10, 10, 34, 8));
        }

        private static GameLogManager EnsureGameLogManager(Transform canvasTransform, Transform logPanel)
        {
            var gameLogManager = Object.FindAnyObjectByType<GameLogManager>();
            if (gameLogManager == null)
            {
                var logManagerObject = new GameObject("GameLogManager");
                Undo.RegisterCreatedObjectUndo(logManagerObject, "GameLogManager 생성");
                logManagerObject.transform.SetParent(canvasTransform, false);
                gameLogManager = logManagerObject.AddComponent<GameLogManager>();
            }

            var serializedObject = new SerializedObject(gameLogManager);
            serializedObject.FindProperty("logText").objectReferenceValue =
                logPanel != null ? logPanel.Find("LogText")?.GetComponent<TMP_Text>() : null;
            serializedObject.FindProperty("maxEntries").intValue = 8;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(gameLogManager);
            return gameLogManager;
        }

        private static void ConfigureTurnPanel(GameObject turnPanel)
        {
            var rectTransform = turnPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-28f, -28f);
            rectTransform.sizeDelta = new Vector2(230f, 210f);

            var layout = turnPanel.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = turnPanel.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void EnsureTurnPanelChildren(Transform turnPanel)
        {
            var turnText = turnPanel.Find("TurnText")?.GetComponent<TextMeshProUGUI>();
            if (turnText == null)
            {
                turnText = CreateText("TurnText", turnPanel, "Turn 1", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            }

            turnText.text = "Turn 1";
            ApplyUiFont(turnText);
            turnText.color = Color.white;
            SetOrUpdatePreferredHeight(turnText.gameObject, 28f);

            var phaseText = turnPanel.Find("PhaseText")?.GetComponent<TextMeshProUGUI>();
            if (phaseText == null)
            {
                phaseText = CreateText("PhaseText", turnPanel, "Main Phase", 16, FontStyles.Bold, TextAlignmentOptions.Center);
            }

            phaseText.text = "Main Phase";
            ApplyUiFont(phaseText);
            phaseText.color = new Color(0.96f, 0.82f, 0.54f);
            SetOrUpdatePreferredHeight(phaseText.gameObject, 24f);

            var deckCountText = turnPanel.Find("DeckCountText")?.GetComponent<TextMeshProUGUI>();
            if (deckCountText == null)
            {
                deckCountText = CreateText("DeckCountText", turnPanel, "Deck 0", 15, FontStyles.Normal, TextAlignmentOptions.Center);
            }

            deckCountText.text = "Deck 0";
            ApplyUiFont(deckCountText);
            deckCountText.color = new Color(0.78f, 0.84f, 0.9f);
            SetOrUpdatePreferredHeight(deckCountText.gameObject, 24f);

            var startRestartButton = turnPanel.Find("StartRestartButton")?.GetComponent<Button>();
            if (startRestartButton == null)
            {
                startRestartButton = CreateButton("StartRestartButton", turnPanel, "Start / Restart");
            }

            SetButtonLabel(startRestartButton, "Start / Restart");
            SetOrUpdatePreferredHeight(startRestartButton.gameObject, 38f);

            var nextTurnButton = turnPanel.Find("NextTurnButton")?.GetComponent<Button>();
            if (nextTurnButton == null)
            {
                nextTurnButton = CreateButton("NextTurnButton", turnPanel, "Next Phase");
            }

            SetButtonLabel(nextTurnButton, "Next Phase");
            SetOrUpdatePreferredHeight(nextTurnButton.gameObject, 38f);
        }

        private static void ConfigurePileCardRoot(
            GameObject pilePanel,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Color color)
        {
            var rectTransform = pilePanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(210f, 300f);

            RemoveLayoutGroups(pilePanel);

            var image = pilePanel.GetComponent<Image>();
            if (image == null)
            {
                image = pilePanel.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = true;

            var outline = pilePanel.GetComponent<Outline>();
            if (outline == null)
            {
                outline = pilePanel.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.78f, 0.84f, 0.9f, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static void RemoveLayoutGroups(GameObject target)
        {
            var layoutGroups = target.GetComponents<LayoutGroup>();
            foreach (var layoutGroup in layoutGroups)
            {
                Object.DestroyImmediate(layoutGroup);
            }
        }

        private static TextMeshProUGUI EnsurePileText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            var textComponent = parent.Find(name)?.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = CreateText(name, parent, text, fontSize, fontStyle, alignment);
            }

            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.overflowMode = TextOverflowModes.Truncate;
            textComponent.raycastTarget = false;
            ApplyUiFont(textComponent);
            return textComponent;
        }

        private static void ConfigurePileTitle(TextMeshProUGUI titleText)
        {
            titleText.color = Color.white;
            var rectTransform = titleText.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -20f);
            rectTransform.sizeDelta = new Vector2(-24f, 42f);
        }

        private static void ConfigurePileCount(TextMeshProUGUI countText)
        {
            countText.color = new Color(0.9f, 0.94f, 1f);
            var rectTransform = countText.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, -4f);
            rectTransform.sizeDelta = new Vector2(160f, 94f);
        }

        private static void ConfigureDeckPileCount(TextMeshProUGUI countText)
        {
            countText.color = new Color(0.96f, 0.88f, 0.64f);
            var rectTransform = countText.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 20f);
            rectTransform.sizeDelta = new Vector2(120f, 48f);
        }

        private static GameObject EnsureHoverPanel(Transform parent, string name, Vector2 size, bool opensRight)
        {
            var hoverPanel = parent.Find(name)?.gameObject;
            if (hoverPanel == null)
            {
                hoverPanel = CreatePanel(name, parent, new Color(0.03f, 0.04f, 0.06f, 0.94f));
            }

            ConfigureHoverPanel(hoverPanel, size, opensRight);
            return hoverPanel;
        }

        private static void ConfigureHoverPanel(GameObject hoverPanel, Vector2 size, bool opensRight)
        {
            RemoveLayoutGroups(hoverPanel);

            var rectTransform = hoverPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = opensRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.pivot = opensRight ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rectTransform.anchoredPosition = opensRight ? new Vector2(14f, 0f) : new Vector2(-14f, 0f);
            rectTransform.sizeDelta = size;

            var image = hoverPanel.GetComponent<Image>();
            if (image == null)
            {
                image = hoverPanel.AddComponent<Image>();
            }

            image.color = new Color(0.03f, 0.04f, 0.06f, 0.94f);
            image.raycastTarget = false;

            var outline = hoverPanel.GetComponent<Outline>();
            if (outline == null)
            {
                outline = hoverPanel.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.78f, 0.84f, 0.9f, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            var labelText = button != null ? button.transform.Find("Label")?.GetComponent<TMP_Text>() : null;
            if (labelText != null)
            {
                ApplyUiFont(labelText);
                labelText.text = label;
            }
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var buttonObject = CreatePanel(name, parent, new Color(0.9f, 0.58f, 0.25f));
            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.9f, 0.58f, 0.25f);
            colors.highlightedColor = new Color(1f, 0.7f, 0.36f);
            colors.pressedColor = new Color(0.72f, 0.42f, 0.16f);
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f);
            button.colors = colors;

            var labelText = CreateText("Label", buttonObject.transform, label, 15, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.color = Color.white;
            StretchToParent(labelText.rectTransform, new RectOffset(8, 8, 4, 4));
            return button;
        }

        private static GameObject CreateUiObject(string name, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return gameObject;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = CreateUiObject(name, Vector2.zero);
            panel.transform.SetParent(parent, false);

            var image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        private static Image CreateArtworkImage(Transform parent)
        {
            var artworkObject = CreateUiObject("ArtworkImage", Vector2.zero);
            artworkObject.transform.SetParent(parent, false);

            var image = artworkObject.AddComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.enabled = false;
            StretchToParent(image.rectTransform, new RectOffset(0, 0, 0, 0));

            return image;
        }

        private static void AddHorizontalLayout(GameObject target, RectOffset padding, float spacing, TextAnchor alignment)
        {
            var layout = target.AddComponent<HorizontalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string text,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            var textObject = CreateUiObject(name, Vector2.zero);
            textObject.transform.SetParent(parent, false);

            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = alignment;
            textComponent.color = Color.black;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.overflowMode = TextOverflowModes.Truncate;
            textComponent.enableAutoSizing = false;
            ApplyUiFont(textComponent);

            return textComponent;
        }

        private static TMP_FontAsset EnsureDefaultFontAsset()
        {
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            cachedDefaultFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetPath);
            if (cachedDefaultFontAsset == null)
            {
                Debug.LogWarning($"기본 TMP 폰트를 찾지 못했습니다: {DefaultFontAssetPath}");
            }

            return cachedDefaultFontAsset;
        }

        private static TMP_FontAsset EnsureKoreanFontAsset()
        {
            if (cachedKoreanFontAsset != null)
            {
                return cachedKoreanFontAsset;
            }

            var existingFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontAssetPath);
            if (existingFontAsset != null)
            {
                cachedKoreanFontAsset = existingFontAsset;
                return cachedKoreanFontAsset;
            }

            EnsureFolder("Assets", "Fonts");

            var fontAsset = File.Exists(KoreanSystemFontPath)
                ? TMP_FontAsset.CreateFontAsset(KoreanSystemFontPath, 0, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048)
                : TMP_FontAsset.CreateFontAsset("Malgun Gothic", "Regular", 90);

            if (fontAsset == null)
            {
                Debug.LogWarning("한글 TMP 폰트를 만들 OS 폰트를 찾지 못했습니다. Windows의 맑은 고딕 폰트를 확인해주세요.");
                return null;
            }

            fontAsset.name = "MalgunGothic SDF";
            AssetDatabase.CreateAsset(fontAsset, KoreanFontAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            cachedKoreanFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontAssetPath);
            return cachedKoreanFontAsset;
        }

        private static void ApplyKoreanFont(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            var koreanFontAsset = EnsureKoreanFontAsset();
            if (koreanFontAsset == null)
            {
                return;
            }

            text.font = koreanFontAsset;
            text.fontSharedMaterial = koreanFontAsset.material;
            EditorUtility.SetDirty(text);
        }

        private static void ApplyUiFont(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            var defaultFontAsset = EnsureDefaultFontAsset();
            if (defaultFontAsset == null)
            {
                return;
            }

            text.font = defaultFontAsset;
            text.fontSharedMaterial = defaultFontAsset.material;
            EditorUtility.SetDirty(text);
        }

        private static void SetPreferredHeight(Component component, float preferredHeight)
        {
            SetPreferredHeight(component.gameObject, preferredHeight);
        }

        private static void SetPreferredHeight(GameObject gameObject, float preferredHeight)
        {
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0f;
        }

        private static void SetOrUpdatePreferredHeight(GameObject gameObject, float preferredHeight)
        {
            var layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0f;
        }

        private static void StretchToParent(RectTransform rectTransform, RectOffset padding)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(padding.left, padding.bottom);
            rectTransform.offsetMax = new Vector2(-padding.right, -padding.top);
        }

        private static void AssignCardViewFields(
            CardView cardView,
            Image backgroundImage,
            Image typeBannerImage,
            Image artworkImage,
            Outline outline,
            Canvas sortingCanvas,
            TMP_Text nameText,
            TMP_Text typeText,
            TMP_Text costOrLevelText,
            TMP_Text attackText,
            TMP_Text defenseText,
            TMP_Text artworkPlaceholderText,
            TMP_Text descriptionText)
        {
            var serializedObject = new SerializedObject(cardView);
            serializedObject.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serializedObject.FindProperty("typeBannerImage").objectReferenceValue = typeBannerImage;
            serializedObject.FindProperty("artworkImage").objectReferenceValue = artworkImage;
            serializedObject.FindProperty("outline").objectReferenceValue = outline;
            serializedObject.FindProperty("sortingCanvas").objectReferenceValue = sortingCanvas;
            serializedObject.FindProperty("nameText").objectReferenceValue = nameText;
            serializedObject.FindProperty("typeText").objectReferenceValue = typeText;
            serializedObject.FindProperty("costOrLevelText").objectReferenceValue = costOrLevelText;
            serializedObject.FindProperty("attackText").objectReferenceValue = attackText;
            serializedObject.FindProperty("defenseText").objectReferenceValue = defenseText;
            serializedObject.FindProperty("artworkPlaceholderText").objectReferenceValue = artworkPlaceholderText;
            serializedObject.FindProperty("descriptionText").objectReferenceValue = descriptionText;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
