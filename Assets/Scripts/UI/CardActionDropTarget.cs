using CardBattle.Cards;
using CardBattle.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardBattle.UI
{
    public class CardActionDropTarget : MonoBehaviour, IDropHandler, ICardDropTarget
    {
        public enum DropAction
        {
            UseSpell,
            SetTrap
        }

        [SerializeField] private CardActionManager cardActionManager;
        [SerializeField] private DropAction dropAction;

        public void Initialize(CardActionManager cardActionManager, DropAction dropAction)
        {
            this.cardActionManager = cardActionManager;
            this.dropAction = dropAction;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cardView = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<CardView>()
                : null;

            TryDropCard(cardView);
        }

        public bool TryDropCard(CardView cardView)
        {
            if (cardActionManager == null || cardView == null || cardView.CardData == null)
            {
                return false;
            }

            switch (dropAction)
            {
                case DropAction.UseSpell:
                    if (cardView.CardData.CardType != CardType.Spell)
                    {
                        return false;
                    }

                    return cardActionManager.TryUseCard(cardView);
                case DropAction.SetTrap:
                    if (cardView.CardData.CardType != CardType.Trap)
                    {
                        return false;
                    }

                    return cardActionManager.TrySetTrap(cardView);
                default:
                    return false;
            }
        }
    }
}
