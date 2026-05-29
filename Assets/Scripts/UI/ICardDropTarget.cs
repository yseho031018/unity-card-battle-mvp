namespace CardBattle.UI
{
    public interface ICardDropTarget
    {
        bool TryDropCard(CardView cardView);
    }
}
