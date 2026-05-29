using CardBattle.Cards;
using UnityEngine;

namespace CardBattle.UI
{
    public class CardPreviewManager : MonoBehaviour
    {
        [SerializeField] private Transform previewRoot;
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField, Range(1f, 2f)] private float previewScale = 1.25f;

        private CardView previewCardView;

        private void Awake()
        {
            EnsurePreviewCard();
            Hide();
        }

        public void Show(CardData cardData)
        {
            if (cardData == null)
            {
                Hide();
                return;
            }

            if (previewRoot != null)
            {
                previewRoot.gameObject.SetActive(true);
            }

            EnsurePreviewCard();

            if (previewCardView == null)
            {
                return;
            }

            previewCardView.gameObject.SetActive(true);
            previewCardView.SetCard(cardData);
            previewCardView.SetSelectable(false);
            previewCardView.SetShowFullDescription(true);
            previewCardView.SetBaseScale(previewScale);
        }

        public void Hide()
        {
            if (previewCardView != null)
            {
                previewCardView.gameObject.SetActive(false);
            }

            if (previewRoot != null)
            {
                previewRoot.gameObject.SetActive(false);
            }
        }

        private void EnsurePreviewCard()
        {
            if (previewCardView != null || previewRoot == null || cardViewPrefab == null)
            {
                return;
            }

            previewCardView = Instantiate(cardViewPrefab, previewRoot);
            previewCardView.name = "카드 프리뷰";
            previewCardView.SetSelectable(false);
            previewCardView.SetShowFullDescription(true);

            var rectTransform = previewCardView.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            var canvasGroup = previewCardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = previewCardView.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
