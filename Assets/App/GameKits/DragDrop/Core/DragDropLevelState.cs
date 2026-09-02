namespace Lbs.MiniGames.GameKits.DragDrop
{
    public enum DragDropPhase { Idle, Dragging, Resolving, Celebrating, Final }

    /// <summary>
    /// Pure reusable drag-drop level state. No Unity dependencies, explicit owner, no asset mutation.
    /// </summary>
    public sealed class DragDropLevelState
    {
        private readonly IDragDropRule rule;

        public DragDropLevelState(IDragDropRule rule)
        {
            this.rule = rule;
        }

        // Convenience for ShapeAnalogy-style correct answer without full rule instance.
        public DragDropLevelState(string correctAnswerId) : this(new SingleCorrectRule(correctAnswerId)) { }

        public DragDropPhase Phase { get; private set; } = DragDropPhase.Idle;
        public bool FinalArmed { get; private set; }
        public int HongFrame { get; private set; }

        public void StartDrag()
        {
            if (Phase == DragDropPhase.Idle) Phase = DragDropPhase.Dragging;
        }

        public DragDropOutcome Drop(string tokenId, bool overTarget)
        {
            if (Phase != DragDropPhase.Dragging) return DragDropOutcome.Outside;
            DragDropOutcome outcome = rule != null ? rule.Evaluate(tokenId, overTarget) : DragDropOutcome.Outside;
            Phase = outcome == DragDropOutcome.Correct ? DragDropPhase.Celebrating
                : outcome == DragDropOutcome.Incorrect ? DragDropPhase.Resolving
                : DragDropPhase.Idle;
            return outcome;
        }

        public void FinishResolve()
        {
            if (Phase == DragDropPhase.Resolving) Phase = DragDropPhase.Idle;
        }

        public void FinishCelebration()
        {
            Phase = DragDropPhase.Final;
            FinalArmed = false;
        }

        public bool AcceptFinalTap() => Phase == DragDropPhase.Final && FinalArmed;
        public void ArmFinal() { if (Phase == DragDropPhase.Final) FinalArmed = true; }
        public void SetHongFrame(int frame) { HongFrame = frame < 1 || frame > 3 ? 0 : frame; }
        public void Reset() { Phase = DragDropPhase.Idle; HongFrame = 0; FinalArmed = false; }

        private sealed class SingleCorrectRule : IDragDropRule
        {
            private readonly string correctId;
            public SingleCorrectRule(string id) { correctId = id; }
            public string CorrectTokenId => correctId;
            public DragDropOutcome Evaluate(string tokenId, bool overTarget)
            {
                if (!overTarget) return DragDropOutcome.Outside;
                return tokenId == correctId ? DragDropOutcome.Correct : DragDropOutcome.Incorrect;
            }
        }
    }
}
