using System.Text;
using CardBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class GraveyardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject listPanel;
        [SerializeField] private TMP_Text listText;
        [SerializeField] private Text titleLegacyText;
        [SerializeField] private Text countLegacyText;
        [SerializeField] private Text listLegacyText;

        private readonly StringBuilder listBuilder = new();
        private DeckManager deckManager;

        private void Awake()
        {
            SetListVisible(false);
            RefreshView();
        }

        public void Initialize(DeckManager deckManager)
        {
            this.deckManager = deckManager;
            SetListVisible(false);
            RefreshView();
        }

        public void RefreshView()
        {
            var graveyard = deckManager != null ? deckManager.Graveyard : null;
            var count = graveyard?.Count ?? 0;

            if (titleText != null)
            {
                titleText.text = "묘지";
            }
            if (titleLegacyText != null)
            {
                titleLegacyText.text = "묘지";
            }

            if (countText != null)
            {
                countText.text = count.ToString();
            }
            if (countLegacyText != null)
            {
                countLegacyText.text = count.ToString();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = count > 0
                    ? new Color(0.27f, 0.22f, 0.28f, 0.96f)
                    : new Color(0.1f, 0.08f, 0.12f, 0.9f);
            }

            if (listText == null && listLegacyText == null)
            {
                return;
            }

            if (count == 0)
            {
                SetListText("카드: 0장\n비어 있음");
                return;
            }

            listBuilder.Clear();
            listBuilder.Append("카드: ");
            listBuilder.AppendLine(count.ToString());

            for (var i = 0; i < count; i++)
            {
                listBuilder.Append(i + 1);
                listBuilder.Append(". ");
                listBuilder.AppendLine(graveyard[i].CardName);
            }

            SetListText(listBuilder.ToString());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RefreshView();
            SetListVisible(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetListVisible(false);
        }

        private void SetListVisible(bool isVisible)
        {
            if (listPanel != null)
            {
                listPanel.SetActive(isVisible);
            }
        }

        private void SetListText(string value)
        {
            if (listText != null)
            {
                listText.text = value;
            }

            if (listLegacyText != null)
            {
                listLegacyText.text = value;
            }
        }
    }
}
