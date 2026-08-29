using NUnit.Framework;
using Lbs.MiniGames.Games.ShapeAnalogy;

namespace Lbs.MiniGames.Tests
{
    public sealed class ShapeAnalogyStateTests
    {
        [Test] public void RuleDistinguishesOutsideIncorrectAndCorrect() { Assert.AreEqual(ShapeAnalogyDropOutcome.Outside, ShapeAnalogyRule.Evaluate("filled-star", false)); Assert.AreEqual(ShapeAnalogyDropOutcome.Incorrect, ShapeAnalogyRule.Evaluate("filled-star", true)); Assert.AreEqual(ShapeAnalogyDropOutcome.Correct, ShapeAnalogyRule.Evaluate("outlined-heart", true)); }
        [Test] public void CorrectDropLocksUntilFinalTap() { var state = new ShapeAnalogyState(); state.StartDrag(); Assert.AreEqual(ShapeAnalogyDropOutcome.Correct, state.Drop("outlined-heart", true)); Assert.IsFalse(state.AcceptFinalTap()); state.FinishCelebration(); Assert.IsFalse(state.AcceptFinalTap()); state.ArmFinal(); Assert.IsTrue(state.AcceptFinalTap()); }
        [Test] public void OutsideDropRestoresIdleAndWrongDropRequiresResolve() { var state = new ShapeAnalogyState(); state.StartDrag(); Assert.AreEqual(ShapeAnalogyDropOutcome.Outside, state.Drop("filled-star", false)); state.StartDrag(); Assert.AreEqual(ShapeAnalogyDropOutcome.Incorrect, state.Drop("filled-star", true)); state.FinishResolve(); Assert.AreEqual(ShapeAnalogyPhase.Idle, state.Phase); }
        [Test] public void HongFrameAndResetAreDeterministic() { var state = new ShapeAnalogyState(); state.SetHongFrame(2); Assert.AreEqual(2, state.HongFrame); state.SetHongFrame(8); Assert.AreEqual(0, state.HongFrame); state.Reset(); Assert.AreEqual(ShapeAnalogyPhase.Idle, state.Phase); }
    }
}
