using CardBattle.Gameplay;
using CardBattle.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CardBattle.Editor
{
    public static class DefaultCardPrefabCreator
    {
        private const string PrefabPath = "Assets/Prefabs/UI/CardView.prefab";

        [MenuItem("Tools/Create Default Card Prefab")]
        public static void CreateDefaultCardPrefab()
        {
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "UI");

            var cardPrefab = CreateCardPrefab();
            var canvas = EnsureCanvas();
            EnsureEventSystem();
            var handRoot = EnsureHandRoot(canvas.transform);
            var fieldRoot = EnsureFieldRoot(canvas.transform);
            var trapZoneRoot = EnsureTrapZoneRoot(canvas.transform);
            var fieldManager = EnsureFieldManager(canvas.transform, fieldRoot, cardPrefab);
            var trapZoneManager = EnsureTrapZoneManager(canvas.transform, trapZoneRoot);
            var handManager = EnsureHandManager(canvas.transform, handRoot, cardPrefab);
            var actionPanel = EnsureActionPanel(canvas.transform);
            var cardActionManager = EnsureCardActionManager(canvas.transform, actionPanel, handManager, trapZoneManager);
            var deckManager = EnsureDeckManager(handManager, fieldManager, trapZoneManager, cardActionManager);
            EnsureTurnManager(canvas.transform, deckManager);

            Selection.activeObject = cardPrefab;
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

            Debug.Log(
                $"Created default card prefab at {PrefabPath} and connected it to {handManager.name}.",
                cardPrefab);
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

            var nameText = CreateText("NameText", header.transform, "Card Name", 18, FontStyles.Bold, TextAlignmentOptions.Left);
            nameText.color = Color.white;
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1f;

            var costOrLevelText = CreateText("CostOrLevelText", header.transform, "LV 1", 15, FontStyles.Bold, TextAlignmentOptions.Center);
            costOrLevelText.color = Color.white;
            var costLayout = costOrLevelText.gameObject.AddComponent<LayoutElement>();
            costLayout.preferredWidth = 48f;

            var typeBanner = CreatePanel("TypeBanner", card.transform, new Color(0.86f, 0.51f, 0.28f));
            SetPreferredHeight(typeBanner, 24f);
            var typeText = CreateText("TypeText", typeBanner.transform, "Monster", 14, FontStyles.Bold, TextAlignmentOptions.Center);
            StretchToParent(typeText.rectTransform, new RectOffset(6, 6, 2, 2));
            typeText.color = Color.white;

            var artFrame = CreatePanel("ArtFrame", card.transform, new Color(0.82f, 0.78f, 0.65f));
            SetPreferredHeight(artFrame, 92f);
            var artLabel = CreateText("ArtPlaceholderText", artFrame.transform, "ART", 24, FontStyles.Bold, TextAlignmentOptions.Center);
            StretchToParent(artLabel.rectTransform, new RectOffset(0, 0, 0, 0));
            artLabel.color = new Color(0.35f, 0.31f, 0.25f);

            var descriptionBox = CreatePanel("DescriptionBox", card.transform, new Color(1f, 0.98f, 0.9f));
            SetPreferredHeight(descriptionBox, 76f);
            var descriptionText = CreateText("DescriptionText", descriptionBox.transform, "Card description.", 13, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            StretchToParent(descriptionText.rectTransform, new RectOffset(7, 7, 6, 6));

            var statsRow = CreateUiObject("StatsRow", Vector2.zero);
            statsRow.transform.SetParent(card.transform, false);
            AddHorizontalLayout(statsRow, new RectOffset(0, 0, 0, 0), 8f, TextAnchor.MiddleCenter);
            SetPreferredHeight(statsRow, 24f);

            var attackText = CreateText("AttackText", statsRow.transform, "ATK 1000", 14, FontStyles.Bold, TextAlignmentOptions.Left);
            var defenseText = CreateText("DefenseText", statsRow.transform, "DEF 1000", 14, FontStyles.Bold, TextAlignmentOptions.Right);
            attackText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            defenseText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            AssignCardViewFields(cardView, background, typeBanner.GetComponent<Image>(), outline, sortingCanvas, nameText, typeText, costOrLevelText, attackText, defenseText, descriptionText);

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
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

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
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
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
            Undo.RegisterCreatedObjectUndo(handRoot, "Create HandRoot");
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
            Undo.RegisterCreatedObjectUndo(fieldRoot, "Create Monster Field Root");
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
            rectTransform.anchoredPosition = new Vector2(0f, 270f);
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
            Undo.RegisterCreatedObjectUndo(slot, "Create Monster Slot");
            slot.transform.SetParent(parent, false);
            ConfigureFieldSlot(slot, slotNumber);
            return slot;
        }

        private static void ConfigureFieldSlot(GameObject slot, int slotNumber)
        {
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
            Undo.RegisterCreatedObjectUndo(trapZoneRoot, "Create Trap Zone Root");
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
            rectTransform.anchoredPosition = new Vector2(0f, -45f);
            rectTransform.sizeDelta = new Vector2(1160f, 230f);

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
            Undo.RegisterCreatedObjectUndo(slot, "Create Trap Slot");
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
                label = CreateText("SlotLabel", slot.transform, $"TRAP ZONE {slotNumber}", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            }

            StretchToParent(label.rectTransform, new RectOffset(12, 12, 12, 12));
            label.color = new Color(0.92f, 0.86f, 0.96f);

            var serializedSlot = new SerializedObject(trapSlot);
            serializedSlot.FindProperty("backgroundImage").objectReferenceValue = image;
            serializedSlot.FindProperty("labelText").objectReferenceValue = label;
            serializedSlot.ApplyModifiedProperties();
        }

        private static HandManager EnsureHandManager(Transform canvasTransform, Transform handRoot, GameObject cardPrefab)
        {
            var handManager = Object.FindAnyObjectByType<HandManager>();
            if (handManager == null)
            {
                var handManagerObject = new GameObject("HandManager");
                Undo.RegisterCreatedObjectUndo(handManagerObject, "Create HandManager");
                handManagerObject.transform.SetParent(canvasTransform, false);
                handManager = handManagerObject.AddComponent<HandManager>();
            }

            var serializedObject = new SerializedObject(handManager);
            serializedObject.FindProperty("handRoot").objectReferenceValue = handRoot;
            serializedObject.FindProperty("cardViewPrefab").objectReferenceValue = cardPrefab.GetComponent<CardView>();
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
                Undo.RegisterCreatedObjectUndo(fieldManagerObject, "Create FieldManager");
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
                Undo.RegisterCreatedObjectUndo(trapZoneManagerObject, "Create TrapZoneManager");
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
                Undo.RegisterCreatedObjectUndo(cardActionObject, "Create CardActionManager");
                cardActionObject.transform.SetParent(canvasTransform, false);
                cardActionManager = cardActionObject.AddComponent<CardActionManager>();
            }

            var useButton = actionPanel.Find("UseCardButton")?.GetComponent<Button>();
            var setTrapButton = actionPanel.Find("SetTrapButton")?.GetComponent<Button>();

            var serializedObject = new SerializedObject(cardActionManager);
            serializedObject.FindProperty("useCardButton").objectReferenceValue = useButton;
            serializedObject.FindProperty("setTrapButton").objectReferenceValue = setTrapButton;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(cardActionManager);
            return cardActionManager;
        }

        private static DeckManager EnsureDeckManager(
            HandManager handManager,
            FieldManager fieldManager,
            TrapZoneManager trapZoneManager,
            CardActionManager cardActionManager)
        {
            var deckManager = Object.FindAnyObjectByType<DeckManager>();
            if (deckManager == null)
            {
                var deckManagerObject = new GameObject("DeckManager");
                Undo.RegisterCreatedObjectUndo(deckManagerObject, "Create DeckManager");
                deckManager = deckManagerObject.AddComponent<DeckManager>();
            }

            var serializedObject = new SerializedObject(deckManager);
            serializedObject.FindProperty("handManager").objectReferenceValue = handManager;
            serializedObject.FindProperty("fieldManager").objectReferenceValue = fieldManager;
            serializedObject.FindProperty("trapZoneManager").objectReferenceValue = trapZoneManager;
            serializedObject.FindProperty("cardActionManager").objectReferenceValue = cardActionManager;
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
                Undo.RegisterCreatedObjectUndo(turnManagerObject, "Create TurnManager");
                turnManagerObject.transform.SetParent(canvasTransform, false);
                turnManager = turnManagerObject.AddComponent<TurnManager>();
            }

            var nextTurnButton = turnPanel.Find("NextTurnButton")?.GetComponent<Button>();
            var turnText = turnPanel.Find("TurnText")?.GetComponent<TMP_Text>();
            var deckCountText = turnPanel.Find("DeckCountText")?.GetComponent<TMP_Text>();

            var serializedObject = new SerializedObject(turnManager);
            serializedObject.FindProperty("deckManager").objectReferenceValue = deckManager;
            serializedObject.FindProperty("nextTurnButton").objectReferenceValue = nextTurnButton;
            serializedObject.FindProperty("turnText").objectReferenceValue = turnText;
            serializedObject.FindProperty("deckCountText").objectReferenceValue = deckCountText;
            serializedObject.ApplyModifiedProperties();

            if (nextTurnButton != null)
            {
                EditorUtility.SetDirty(nextTurnButton);
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
            Undo.RegisterCreatedObjectUndo(turnPanel, "Create Turn Panel");
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
            Undo.RegisterCreatedObjectUndo(actionPanel, "Create Action Panel");
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
            rectTransform.anchoredPosition = new Vector2(-28f, -168f);
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

            SetOrUpdatePreferredHeight(useCardButton.gameObject, 40f);

            var setTrapButton = actionPanel.Find("SetTrapButton")?.GetComponent<Button>();
            if (setTrapButton == null)
            {
                setTrapButton = CreateButton("SetTrapButton", actionPanel, "Set Trap");
            }

            SetOrUpdatePreferredHeight(setTrapButton.gameObject, 40f);
        }

        private static void ConfigureTurnPanel(GameObject turnPanel)
        {
            var rectTransform = turnPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-28f, -28f);
            rectTransform.sizeDelta = new Vector2(230f, 128f);

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

            turnText.color = Color.white;
            SetOrUpdatePreferredHeight(turnText.gameObject, 28f);

            var deckCountText = turnPanel.Find("DeckCountText")?.GetComponent<TextMeshProUGUI>();
            if (deckCountText == null)
            {
                deckCountText = CreateText("DeckCountText", turnPanel, "Deck 0", 15, FontStyles.Normal, TextAlignmentOptions.Center);
            }

            deckCountText.color = new Color(0.78f, 0.84f, 0.9f);
            SetOrUpdatePreferredHeight(deckCountText.gameObject, 24f);

            var nextTurnButton = turnPanel.Find("NextTurnButton")?.GetComponent<Button>();
            if (nextTurnButton == null)
            {
                nextTurnButton = CreateButton("NextTurnButton", turnPanel, "Next Turn / Draw");
            }

            SetOrUpdatePreferredHeight(nextTurnButton.gameObject, 42f);
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
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.overflowMode = TextOverflowModes.Truncate;

            return textComponent;
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
            Outline outline,
            Canvas sortingCanvas,
            TMP_Text nameText,
            TMP_Text typeText,
            TMP_Text costOrLevelText,
            TMP_Text attackText,
            TMP_Text defenseText,
            TMP_Text descriptionText)
        {
            var serializedObject = new SerializedObject(cardView);
            serializedObject.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serializedObject.FindProperty("typeBannerImage").objectReferenceValue = typeBannerImage;
            serializedObject.FindProperty("outline").objectReferenceValue = outline;
            serializedObject.FindProperty("sortingCanvas").objectReferenceValue = sortingCanvas;
            serializedObject.FindProperty("nameText").objectReferenceValue = nameText;
            serializedObject.FindProperty("typeText").objectReferenceValue = typeText;
            serializedObject.FindProperty("costOrLevelText").objectReferenceValue = costOrLevelText;
            serializedObject.FindProperty("attackText").objectReferenceValue = attackText;
            serializedObject.FindProperty("defenseText").objectReferenceValue = defenseText;
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
