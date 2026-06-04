using CardBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class DeckView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject hoverPanel;
        [SerializeField] private TMP_Text hoverText;
        [SerializeField] private Text titleLegacyText;
        [SerializeField] private Text countLegacyText;
        [SerializeField] private Text hoverLegacyText;
        [SerializeField] private TurnManager turnManager;
        [SerializeField, Min(1)] private int cardsDrawnPerClick = 1;

        private static readonly Color NormalColor = new(0.07f, 0.11f, 0.2f, 0.98f);
        private static readonly Color HoverColor = new(0.09f, 0.16f, 0.3f, 0.98f);
        private static readonly Color EmptyColor = new(0.08f, 0.1f, 0.13f, 0.9f);
        private const float CardBackMargin = 10f;

        private DeckManager deckManager;
        private bool isHovering;

        private void Awake()
        {
            EnsureCardBackVisuals();
            SetHoverVisible(false);
            RefreshView();
        }

        public void Initialize(DeckManager deckManager)
        {
            this.deckManager = deckManager;
            SetHoverVisible(false);
            RefreshView();
        }

        public void RefreshView()
        {
            var remainingCards = deckManager != null ? deckManager.DrawPile.Count : 0;

            var showEmptyDeckLabels = remainingCards <= 0;
            SetPileLabel(titleText, "덱", showEmptyDeckLabels);
            SetPileLabel(countText, remainingCards.ToString(), showEmptyDeckLabels);
            SetPileLabel(titleLegacyText, "덱", showEmptyDeckLabels);
            SetPileLabel(countLegacyText, remainingCards.ToString(), showEmptyDeckLabels);

            if (hoverText != null)
            {
                hoverText.text = $"남은 카드\n{remainingCards}";
            }
            if (hoverLegacyText != null)
            {
                hoverLegacyText.text = $"남은 카드\n{remainingCards}";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = remainingCards > 0
                    ? isHovering ? HoverColor : NormalColor
                    : EmptyColor;
            }

            CardBackVisual.SetVisible(transform, remainingCards > 0);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            RefreshView();
            SetHoverVisible(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            RefreshView();
            SetHoverVisible(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || deckManager == null)
            {
                return;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null)
            {
                GameLogManager.Log("카드는 드로우 페이즈에 자동으로 드로우됩니다.");
                return;
            }

            var drawnCards = deckManager.DrawCards(cardsDrawnPerClick);
            if (drawnCards.Count == 0)
            {
                GameLogManager.Log("덱에 남은 카드가 없습니다.");
                RefreshView();
                return;
            }

            GameLogManager.Log($"덱에서 {drawnCards.Count}장을 드로우했습니다.");
        }

        private void SetHoverVisible(bool isVisible)
        {
            if (hoverPanel != null)
            {
                hoverPanel.SetActive(isVisible);
            }
        }

        private void EnsureCardBackVisuals()
        {
            CardBackVisual.Ensure(transform, CardBackMargin);
        }

        private TurnManager GetTurnManager()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindAnyObjectByType<TurnManager>();
            }

            return turnManager;
        }

        private static void SetPileLabel(TMP_Text text, string value, bool isVisible)
        {
            if (text == null)
            {
                return;
            }

            text.text = value;
            text.gameObject.SetActive(isVisible);
        }

        private static void SetPileLabel(Text text, string value, bool isVisible)
        {
            if (text == null)
            {
                return;
            }

            text.text = value;
            text.gameObject.SetActive(isVisible);
        }
    }
}
