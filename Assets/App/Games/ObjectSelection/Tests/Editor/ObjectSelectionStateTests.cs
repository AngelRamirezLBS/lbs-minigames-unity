using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.ObjectSelection;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class ObjectSelectionStateTests
    {
        [Test] public void Tenis_IsTheSemanticCorrectAnswer() => Assert.IsTrue(ObjectSelectionRule.IsCorrect("tenis"));
        [Test] public void HatSelectionLocksUntilResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("sombrero", ObjectSelectionRule.CorrectAnswer));
            Assert.IsFalse(state.Select("tenis", ObjectSelectionRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }
    }
}
