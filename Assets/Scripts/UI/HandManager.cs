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
        [SerializeField, Min(1)] private int maxVisibleCards = 10;
        [SerializeField, Min(0f)] private float cardWidth = 210f;
        [SerializeField, Min(0f)] private float defaultSpacing = 12f;
        [SerializeField, Range(0.5f, 1f)] private float minimumCardScale = 0.72f;

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
            visibleCards.Clear();

            for (var i = handRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(handRoot.GetChild(i).gameObject);
            }
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
