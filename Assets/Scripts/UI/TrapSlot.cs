using CardBattle.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class TrapSlot : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        private CardData trapCard;

        public bool IsOccupied => trapCard != null;

        public void SetTrap(CardData cardData)
        {
            trapCard = cardData;

            if (labelText != null)
            {
                labelText.text = $"SET TRAP\n{cardData.CardName}";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.33f, 0.17f, 0.4f, 0.95f);
            }
        }

        public void ClearIfEmpty()
        {
            if (trapCard != null)
            {
                return;
            }

            if (labelText != null)
            {
                labelText.text = "TRAP ZONE";
            }
        }
    }
}
