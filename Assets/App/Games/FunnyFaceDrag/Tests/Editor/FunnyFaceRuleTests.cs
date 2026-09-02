using Lbs.MiniGames.Games.FunnyFaceDrag;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class FunnyFaceRuleTests
    {
        [Test] public void PurplePiece_MatchesTopSlot() => Assert.IsTrue(FunnyFaceRule.Matches(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot));
        [Test] public void GreenPiece_MatchesBottomSlot() => Assert.IsTrue(FunnyFaceRule.Matches(FunnyFaceRule.GreenPiece, FunnyFaceRule.BottomSlot));
        [Test] public void YellowPiece_MatchesNoSlot()
        {
            Assert.IsFalse(FunnyFaceRule.Matches(FunnyFaceRule.YellowPiece, FunnyFaceRule.TopSlot));
            Assert.IsFalse(FunnyFaceRule.Matches(FunnyFaceRule.YellowPiece, FunnyFaceRule.BottomSlot));
        }
        [Test] public void CrossMatches_AreFalse()
        {
            Assert.IsFalse(FunnyFaceRule.Matches(FunnyFaceRule.PurplePiece, FunnyFaceRule.BottomSlot));
            Assert.IsFalse(FunnyFaceRule.Matches(FunnyFaceRule.GreenPiece, FunnyFaceRule.TopSlot));
        }
        [Test] public void Evaluate_Outside_ReturnsOutside() => Assert.AreEqual(FunnyFaceDropOutcome.Outside, FunnyFaceRule.Evaluate(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot, false));
        [Test] public void Evaluate_Correct_ReturnsCorrect() => Assert.AreEqual(FunnyFaceDropOutcome.Correct, FunnyFaceRule.Evaluate(FunnyFaceRule.PurplePiece, FunnyFaceRule.TopSlot, true));
        [Test] public void Evaluate_Incorrect_ReturnsIncorrect() => Assert.AreEqual(FunnyFaceDropOutcome.Incorrect, FunnyFaceRule.Evaluate(FunnyFaceRule.YellowPiece, FunnyFaceRule.TopSlot, true));
        [Test] public void Constants_MatchSpec()
        {
            Assert.AreEqual("TopSlot", FunnyFaceRule.TopSlot);
            Assert.AreEqual("BottomSlot", FunnyFaceRule.BottomSlot);
            Assert.AreEqual("Drag3", FunnyFaceRule.PurplePiece);
            Assert.AreEqual("Drag2", FunnyFaceRule.GreenPiece);
            Assert.AreEqual("Drag1", FunnyFaceRule.YellowPiece);
        }
    }
}
