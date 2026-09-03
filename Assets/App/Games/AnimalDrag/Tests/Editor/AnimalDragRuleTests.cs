using Lbs.MiniGames.Games.AnimalDrag;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class AnimalDragRuleTests
    {
        [Test]
        public void Cat_Matches_YellowHouse_Only()
        {
            Assert.IsTrue(AnimalDragRule.Matches(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot));
            Assert.IsFalse(AnimalDragRule.Matches(AnimalDragRule.CatPiece, AnimalDragRule.GreenHouseSlot));
        }

        [Test]
        public void Pig_Matches_GreenHouse_Only()
        {
            Assert.IsTrue(AnimalDragRule.Matches(AnimalDragRule.PigPiece, AnimalDragRule.GreenHouseSlot));
            Assert.IsFalse(AnimalDragRule.Matches(AnimalDragRule.PigPiece, AnimalDragRule.YellowHouseSlot));
        }

        [Test]
        public void Evaluate_ReturnsOutside_WhenNotOverlapping()
        {
            Assert.AreEqual(AnimalDragDropOutcome.Outside, AnimalDragRule.Evaluate(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, false));
        }

        [Test]
        public void Evaluate_ReturnsCorrect_Or_Incorrect_BasedOnMatch()
        {
            Assert.AreEqual(AnimalDragDropOutcome.Correct, AnimalDragRule.Evaluate(AnimalDragRule.CatPiece, AnimalDragRule.YellowHouseSlot, true));
            Assert.AreEqual(AnimalDragDropOutcome.Incorrect, AnimalDragRule.Evaluate(AnimalDragRule.CatPiece, AnimalDragRule.GreenHouseSlot, true));
            Assert.AreEqual(AnimalDragDropOutcome.Correct, AnimalDragRule.Evaluate(AnimalDragRule.PigPiece, AnimalDragRule.GreenHouseSlot, true));
            Assert.AreEqual(AnimalDragDropOutcome.Incorrect, AnimalDragRule.Evaluate(AnimalDragRule.PigPiece, AnimalDragRule.YellowHouseSlot, true));
        }
    }
}
