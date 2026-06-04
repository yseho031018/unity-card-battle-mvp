using System;
using System.Collections.Generic;
using CardBattle.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class HandManager : MonoBehaviour
    {
        [SerializeField] private Transform handRoot;
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private CardPreviewManager cardPreviewManager;
        [SerializeField, Min(1)] private int maxVisibleCards = 10;
        [SerializeField, Min(0f)] private float cardWidth = 210f;
        [SerializeField, Min(0f)] private float cardHeight = 300f;
        [SerializeField, Min(0f)] private float defaultSpacing = 12f;
        [SerializeField, Range(0.5f, 1f)] private float minimumCardScale = 0.86f;

        private CardView selectedCardView;
        private readonly List<CardView> visibleCards = new();
        private HorizontalLayoutGroup handLayout;
        private RectTransform handRectTransform;

        public event Action<CardView> CardSelected;
        public CardView SelectedCardView => selectedCardView;

        private void Awake()
        {
            CacheLayoutReferences();
        }

        public void ShowCards(IEnumerable<CardData> cards)
        {
            ClearHand();
            visibleCards.Clear();

            foreach (var card in cards)
            {
                var cardView = Instantiate(cardViewPrefab, handRoot);
                cardView.SetCard(card);
                cardView.SetSelectable(true);
                cardView.Clicked += SelectCard;
                cardView.DragStarted += SelectCardForDrag;
                cardView.PointerEntered += ShowPreview;
                cardView.PointerExited += HidePreview;
                visibleCards.Add(cardView);
            }

            ApplyHandLayout();
        }

        public void ClearSelection()
        {
            if (selectedCardView != null)
            {
                selectedCardView.SetSelected(false);
                selectedCardView = null;
            }

            CardSelected?.Invoke(null);
        }

        public void ClearHand()
        {
            if (handRoot == null)
            {
                return;
            }

            selectedCardView = null;
            var queuedForDestroy = new HashSet<GameObject>();

            for (var i = visibleCards.Count - 1; i >= 0; i--)
            {
                var cardView = visibleCards[i];
                HideAndDestroy(cardView != null ? cardView.gameObject : null, queuedForDestroy);
            }

            visibleCards.Clear();

            for (var i = handRoot.childCount - 1; i >= 0; i--)
            {
                HideAndDestroy(handRoot.GetChild(i).gameObject, queuedForDestroy);
            }

            CardSelected?.Invoke(null);
        }

        private static void HideAndDestroy(GameObject target, HashSet<GameObject> queuedForDestroy)
        {
            if (target == null || !queuedForDestroy.Add(target))
            {
                return;
            }

            target.SetActive(false);
            Destroy(target);
        }

        private void SelectCard(CardView cardView)
        {
            if (selectedCardView == cardView)
            {
                ClearSelection();
                return;
            }

            if (selectedCardView != null)
            {
                selectedCardView.SetSelected(false);
            }

            selectedCardView = cardView;
            selectedCardView.SetSelected(true);
            CardSelected?.Invoke(selectedCardView);
        }

        private void SelectCardForDrag(CardView cardView)
        {
            if (selectedCardView == cardView)
            {
                return;
            }

            if (selectedCardView != null)
            {
                selectedCardView.SetSelected(false);
            }

            selectedCardView = cardView;
            selectedCardView.SetSelected(true);
            CardSelected?.Invoke(selectedCardView);
        }

        private void ShowPreview(CardView cardView)
        {
            if (cardView != null && cardPreviewManager != null)
            {
                cardPreviewManager.Show(cardView.CardData);
            }
        }

        private void HidePreview(CardView cardView)
        {
            if (cardPreviewManager != null)
            {
                cardPreviewManager.Hide();
            }
        }

        private void ApplyHandLayout()
        {
            CacheLayoutReferences();

            if (handRectTransform == null || visibleCards.Count == 0)
            {
                return;
            }

            var cardCount = Mathf.Min(visibleCards.Count, maxVisibleCards);
            var availableWidth = handRectTransform.rect.width;
            if (availableWidth <= 0f)
            {
                availableWidth = handRectTransform.sizeDelta.x;
            }

            var availableHeight = handRectTransform.rect.height;
            if (availableHeight <= 0f)
            {
                availableHeight = handRectTransform.sizeDelta.y;
            }

            var spacing = defaultSpacing;
            var scale = 1f;
            var fullWidth = cardCount * cardWidth + Mathf.Max(0, cardCount - 1) * spacing;

            if (fullWidth > availableWidth && cardCount > 1)
            {
                spacing = Mathf.Max(-cardWidth * 0.45f, (availableWidth - cardCount * cardWidth) / (cardCount - 1));
                fullWidth = cardCount * cardWidth + Mathf.Max(0, cardCount - 1) * spacing;
            }

            if (fullWidth > availableWidth)
            {
                scale = Mathf.Clamp(availableWidth / fullWidth, minimumCardScale, 1f);
            }

            if (availableHeight > 0f && cardHeight > 0f)
            {
                scale = Mathf.Min(scale, Mathf.Clamp(availableHeight / cardHeight, minimumCardScale, 1f));
            }

            if (handLayout != null)
            {
                handLayout.spacing = spacing;
            }

            foreach (var cardView in visibleCards)
            {
                cardView.SetBaseScale(scale);
            }
        }

        private void CacheLayoutReferences()
        {
            if (handRoot == null)
            {
                return;
            }

            if (handLayout == null)
            {
                handLayout = handRoot.GetComponent<HorizontalLayoutGroup>();
            }

            if (handRectTransform == null)
            {
                handRectTransform = handRoot.GetComponent<RectTransform>();
            }
        }
    }
}
