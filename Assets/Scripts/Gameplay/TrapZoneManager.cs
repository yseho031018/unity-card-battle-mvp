using System.Collections.Generic;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class TrapZoneManager : MonoBehaviour
    {
        [SerializeField] private List<TrapSlot> trapSlots = new();

        public void Initialize()
        {
            foreach (var slot in trapSlots)
            {
                slot.ClearIfEmpty();
            }
        }

        public bool SetTrap(CardData cardData)
        {
            if (cardData == null || cardData.CardType != CardType.Trap)
            {
                return false;
            }

            foreach (var slot in trapSlots)
            {
                if (slot != null && !slot.IsOccupied)
                {
                    slot.SetTrap(cardData);
                    Debug.Log($"{cardData.CardName} set in Trap Zone. Activation is not implemented yet.");
                    return true;
                }
            }

            Debug.Log("Trap Zone is full.");
            return false;
        }
    }
}
