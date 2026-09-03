namespace Lbs.MiniGames.GameKits.DragDrop
{
    /// <summary>
    /// Pure rule contract independent of Unity. Evaluates token/target relationship.
    /// </summary>
    public interface IDragDropRule
    {
        DragDropOutcome Evaluate(string tokenId, bool overTarget);
        string CorrectTokenId { get; }
    }
}
