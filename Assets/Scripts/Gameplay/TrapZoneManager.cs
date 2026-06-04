using System.Collections.Generic;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class TrapZoneManager : MonoBehaviour
    {
        [SerializeField] private List<TrapSlot> trapSlots = new();

        public void Initialize(CardActionManager cardActionManager)
        {
            foreach (var slot in trapSlots)
            {
                slot.Initialize(cardActionManager);
                slot.ClearIfEmpty();
            }
        }

        public bool SetTrap(CardData cardData, TrapSlot preferredSlot = null)
        {
            if (cardData == null || cardData.CardType != CardType.Trap)
            {
                return false;
            }

            if (preferredSlot != null)
            {
                return TrySetTrapInSlot(cardData, preferredSlot);
            }

            foreach (var slot in trapSlots)
            {
                if (TrySetTrapInSlot(cardData, slot))
                {
                    return true;
                }
            }

            Debug.Log("함정 존이 가득 찼습니다.");
            return false;
        }

        public void ClearTraps()
        {
            foreach (var slot in trapSlots)
            {
                slot?.Clear();
            }
        }

        private static bool TrySetTrapInSlot(CardData cardData, TrapSlot slot)
        {
            if (slot == null || slot.IsOccupied)
            {
                return false;
            }

            slot.SetTrap(cardData);
            GameLogManager.Log($"{cardData.CardName}을(를) 함정 존에 세트했습니다.");
            return true;
        }
    }
}
