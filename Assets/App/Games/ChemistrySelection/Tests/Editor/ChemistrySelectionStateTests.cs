using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.ChemistrySelection;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class ChemistrySelectionStateTests
    {
        [Test] public void Option1_IsTheSemanticCorrectAnswer() => Assert.IsTrue(ChemistrySelectionRule.IsCorrect("option1"));

        [Test]
        public void Option2_LocksSelectionUntilIncorrectResolution()
        {
            var state = new SelectionGameState();
            Assert.IsFalse(state.Select("option2", ChemistrySelectionRule.CorrectAnswer));
            Assert.IsFalse(state.Select("option1", ChemistrySelectionRule.CorrectAnswer));
            Assert.AreEqual(SelectionPhase.ResolvingIncorrect, state.Phase);
        }

        [Test] public void Option3_IsIncorrect() => Assert.IsFalse(ChemistrySelectionRule.IsCorrect("option3"));

        [Test]
        public void FinalInputRemainsLockedUntilResultEntranceCompletes()
        {
            var state = new SelectionGameState();
            state.Select("option1", ChemistrySelectionRule.CorrectAnswer);
            state.FinishCelebration();
            Assert.IsFalse(state.AcceptFinalInput());
            state.EnableFinalInput();
            Assert.IsTrue(state.AcceptFinalInput());
        }
    }
}
