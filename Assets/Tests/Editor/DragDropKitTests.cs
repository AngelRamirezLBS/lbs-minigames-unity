using NUnit.Framework;
using Lbs.MiniGames.GameKits.DragDrop;

namespace Lbs.MiniGames.Tests
{
    public sealed class DragDropKitTests
    {
        [Test]
        public void Rule_Evaluate_CorrectOutsideIncorrect()
        {
            var rule = new TestRule("correct-id");
            Assert.AreEqual(DragDropOutcome.Outside, rule.Evaluate("any", false));
            Assert.AreEqual(DragDropOutcome.Incorrect, rule.Evaluate("wrong", true));
            Assert.AreEqual(DragDropOutcome.Correct, rule.Evaluate("correct-id", true));
        }

        [Test]
        public void State_Transitions_Match_ShapeAnalogy()
        {
            var state = new DragDropLevelState("correct-id");
            Assert.AreEqual(DragDropPhase.Idle, state.Phase);
            state.StartDrag();
            Assert.AreEqual(DragDropPhase.Dragging, state.Phase);
            Assert.AreEqual(DragDropOutcome.Outside, state.Drop("wrong", false));
            Assert.AreEqual(DragDropPhase.Idle, state.Phase);
            state.StartDrag();
            Assert.AreEqual(DragDropOutcome.Incorrect, state.Drop("wrong", true));
            Assert.AreEqual(DragDropPhase.Resolving, state.Phase);
            state.FinishResolve();
            Assert.AreEqual(DragDropPhase.Idle, state.Phase);
            state.StartDrag();
            Assert.AreEqual(DragDropOutcome.Correct, state.Drop("correct-id", true));
            Assert.AreEqual(DragDropPhase.Celebrating, state.Phase);
            state.FinishCelebration();
            Assert.AreEqual(DragDropPhase.Final, state.Phase);
            Assert.IsFalse(state.AcceptFinalTap());
            state.ArmFinal();
            Assert.IsTrue(state.AcceptFinalTap());
        }

        [Test]
        public void Proximity_Mapping_Matches_Current_Interpolation()
        {
            // Distance 0 => t=1, visible true, alpha 1, outline 3, scale 1.02
            var near = ProximityHighlighter.Compute(0f);
            Assert.IsTrue(near.visible);
            Assert.AreEqual(1f, near.t, 0.001f);
            Assert.AreEqual(1f, near.alpha, 0.001f);
            Assert.AreEqual(1f, near.colorAlpha, 0.001f);
            Assert.AreEqual(3f, near.outlineThickness, 0.001f);
            Assert.AreEqual(1.02f, near.scale, 0.001f);

            // Far beyond maxDist => t ~0, not visible
            var far = ProximityHighlighter.Compute(400f);
            Assert.IsFalse(far.visible);

            // Mid distance 175 => t=0.5, lerp checks
            var mid = ProximityHighlighter.Compute(175f);
            Assert.IsTrue(mid.visible);
            Assert.AreEqual(0.5f, mid.t, 0.001f);
            Assert.AreEqual(0.675f, mid.alpha, 0.001f); // lerp 0.35->1 at 0.5 =0.675
            Assert.AreEqual(0.725f, mid.colorAlpha, 0.001f); // 0.45->1 at 0.5
            Assert.AreEqual(5.5f, mid.outlineThickness, 0.001f);
            Assert.AreEqual(1.01f, mid.scale, 0.001f);
        }

        [Test]
        public void OnePointer_Default_BlocksSecondPointer_And_Cleanup()
        {
            // Pure logic test via state: second StartDrag while already dragging should not reset
            var state = new DragDropLevelState("a");
            state.StartDrag();
            state.StartDrag(); // second call while already dragging does nothing (still Dragging)
            Assert.AreEqual(DragDropPhase.Dragging, state.Phase);
            // Symmetric cleanup via Reset
            state.Reset();
            Assert.AreEqual(DragDropPhase.Idle, state.Phase);
            Assert.IsFalse(state.AcceptFinalTap());
        }

        private sealed class TestRule : IDragDropRule
        {
            private readonly string correct;
            public TestRule(string c) { correct = c; }
            public string CorrectTokenId => correct;
            public DragDropOutcome Evaluate(string tokenId, bool overTarget)
            {
                if (!overTarget) return DragDropOutcome.Outside;
                return tokenId == correct ? DragDropOutcome.Correct : DragDropOutcome.Incorrect;
            }
        }
    }
}
