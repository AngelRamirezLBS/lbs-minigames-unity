namespace Lbs.MiniGames.Games.ShapeAnalogy
{
    public enum ShapeAnalogyPhase { Idle, Dragging, Resolving, Celebrating, Final }

    public sealed class ShapeAnalogyState
    {
        public ShapeAnalogyPhase Phase { get; private set; }
        public int HongFrame { get; private set; }
        public bool FinalArmed { get; private set; }
        public bool HasMistake { get; private set; }
        public int Score => HasMistake ? 4 : 8;
        public int StarCount => HasMistake ? 1 : 2;
        public void StartDrag() { if (Phase == ShapeAnalogyPhase.Idle) Phase = ShapeAnalogyPhase.Dragging; }
        public ShapeAnalogyDropOutcome Drop(string answerId, bool overTarget)
        {
            if (Phase != ShapeAnalogyPhase.Dragging) return ShapeAnalogyDropOutcome.Outside;
            var result = ShapeAnalogyRule.Evaluate(answerId, overTarget);
            Phase = result == ShapeAnalogyDropOutcome.Correct ? ShapeAnalogyPhase.Celebrating : result == ShapeAnalogyDropOutcome.Incorrect ? ShapeAnalogyPhase.Resolving : ShapeAnalogyPhase.Idle;
            return result;
        }
        public void FinishResolve() { if (Phase == ShapeAnalogyPhase.Resolving) Phase = ShapeAnalogyPhase.Idle; }
        public void RecordMistake() { HasMistake = true; }
        public void FinishCelebration() { Phase = ShapeAnalogyPhase.Final; FinalArmed = false; }
        public bool AcceptFinalTap() => Phase == ShapeAnalogyPhase.Final && FinalArmed;
        public void ArmFinal() { if (Phase == ShapeAnalogyPhase.Final) FinalArmed = true; }
        public void SetHongFrame(int frame) { HongFrame = frame < 1 || frame > 3 ? 0 : frame; }
        public void Reset() { Phase = ShapeAnalogyPhase.Idle; HongFrame = 0; FinalArmed = false; HasMistake = false; }
    }
}
