using System;

namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public enum MatchSide
    {
        Left,
        Right
    }

    public enum SubmissionFeedback
    {
        None,
        Correct,
        Incorrect,
        Neutralized
    }

    public enum MatchOutcome
    {
        InProgress,
        LeftWins,
        RightWins,
        Draw
    }

    public readonly struct PlayerStats
    {
        public PlayerStats(int correct, int attempts)
        {
            Correct = correct;
            Attempts = attempts;
        }

        public int Correct { get; }
        public int Attempts { get; }
    }

    public readonly struct SubmissionResult
    {
        public SubmissionResult(SubmissionFeedback left, SubmissionFeedback right, bool balanceChanged)
        {
            Left = left;
            Right = right;
            BalanceChanged = balanceChanged;
        }

        public SubmissionFeedback Left { get; }
        public SubmissionFeedback Right { get; }
        public bool BalanceChanged { get; }
    }

    public readonly struct NumberPullResult
    {
        public NumberPullResult(
            MatchOutcome outcome,
            int balance,
            float elapsedSeconds,
            PlayerStats leftStats,
            PlayerStats rightStats)
        {
            Outcome = outcome;
            Balance = balance;
            ElapsedSeconds = elapsedSeconds;
            LeftStats = leftStats;
            RightStats = rightStats;
        }

        public MatchOutcome Outcome { get; }
        public int Balance { get; }
        public float ElapsedSeconds { get; }
        public PlayerStats LeftStats { get; }
        public PlayerStats RightStats { get; }
    }

    public sealed class NumberPullMatch
    {
        public const int DefaultTargetPulls = 5;
        public const float DefaultDurationSeconds = 90f;

        private readonly int targetPulls;
        private readonly float durationSeconds;
        private readonly IMathProblemGenerator leftGenerator;
        private readonly IMathProblemGenerator rightGenerator;

        private int leftCorrect;
        private int leftAttempts;
        private int rightCorrect;
        private int rightAttempts;
        private bool resultConsumed;

        public NumberPullMatch(
            IMathProblemGenerator leftGenerator,
            IMathProblemGenerator rightGenerator,
            int targetPulls = DefaultTargetPulls,
            float durationSeconds = DefaultDurationSeconds)
        {
            this.leftGenerator = leftGenerator ?? throw new ArgumentNullException(nameof(leftGenerator));
            this.rightGenerator = rightGenerator ?? throw new ArgumentNullException(nameof(rightGenerator));
            if (targetPulls <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPulls));
            }

            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            this.targetPulls = targetPulls;
            this.durationSeconds = durationSeconds;
            LeftProblem = leftGenerator.Next();
            RightProblem = rightGenerator.Next();
            Outcome = MatchOutcome.InProgress;
        }

        public MathProblem LeftProblem { get; private set; }
        public MathProblem RightProblem { get; private set; }
        public int Balance { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float RemainingSeconds => Math.Max(0f, durationSeconds - ElapsedSeconds);
        public MatchOutcome Outcome { get; private set; }
        public bool IsComplete => Outcome != MatchOutcome.InProgress;
        public PlayerStats LeftStats => new(leftCorrect, leftAttempts);
        public PlayerStats RightStats => new(rightCorrect, rightAttempts);

        public SubmissionResult Submit(int? leftAnswer, int? rightAnswer)
        {
            if (IsComplete)
            {
                return new SubmissionResult(SubmissionFeedback.None, SubmissionFeedback.None, false);
            }

            bool leftSubmitted = leftAnswer.HasValue;
            bool rightSubmitted = rightAnswer.HasValue;
            bool leftIsCorrect = leftSubmitted && leftAnswer.Value == LeftProblem.Answer;
            bool rightIsCorrect = rightSubmitted && rightAnswer.Value == RightProblem.Answer;

            if (leftSubmitted)
            {
                leftAttempts++;
                if (leftIsCorrect)
                {
                    leftCorrect++;
                    LeftProblem = leftGenerator.Next();
                }
            }

            if (rightSubmitted)
            {
                rightAttempts++;
                if (rightIsCorrect)
                {
                    rightCorrect++;
                    RightProblem = rightGenerator.Next();
                }
            }

            SubmissionFeedback leftFeedback = FeedbackFor(leftSubmitted, leftIsCorrect);
            SubmissionFeedback rightFeedback = FeedbackFor(rightSubmitted, rightIsCorrect);
            bool changed = false;

            if (leftIsCorrect && rightIsCorrect)
            {
                leftFeedback = SubmissionFeedback.Neutralized;
                rightFeedback = SubmissionFeedback.Neutralized;
            }
            else if (leftIsCorrect)
            {
                Balance = Math.Max(-targetPulls, Balance - 1);
                changed = true;
            }
            else if (rightIsCorrect)
            {
                Balance = Math.Min(targetPulls, Balance + 1);
                changed = true;
            }

            if (Balance <= -targetPulls)
            {
                Outcome = MatchOutcome.LeftWins;
            }
            else if (Balance >= targetPulls)
            {
                Outcome = MatchOutcome.RightWins;
            }

            return new SubmissionResult(leftFeedback, rightFeedback, changed);
        }

        public void Tick(float deltaSeconds)
        {
            if (IsComplete || deltaSeconds <= 0f)
            {
                return;
            }

            ElapsedSeconds = Math.Min(durationSeconds, ElapsedSeconds + deltaSeconds);
            if (ElapsedSeconds < durationSeconds)
            {
                return;
            }

            Outcome = Balance < 0
                ? MatchOutcome.LeftWins
                : Balance > 0
                    ? MatchOutcome.RightWins
                    : MatchOutcome.Draw;
        }

        public bool TryConsumeResult(out NumberPullResult result)
        {
            if (!IsComplete || resultConsumed)
            {
                result = default;
                return false;
            }

            resultConsumed = true;
            result = new NumberPullResult(Outcome, Balance, ElapsedSeconds, LeftStats, RightStats);
            return true;
        }

        private static SubmissionFeedback FeedbackFor(bool submitted, bool correct)
        {
            if (!submitted)
            {
                return SubmissionFeedback.None;
            }

            return correct ? SubmissionFeedback.Correct : SubmissionFeedback.Incorrect;
        }
    }
}
