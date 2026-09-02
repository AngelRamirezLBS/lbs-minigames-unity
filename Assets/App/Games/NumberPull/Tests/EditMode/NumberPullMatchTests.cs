using System.Collections.Generic;
using Lbs.MiniGames.Games.NumberPull.Domain;
using NUnit.Framework;

namespace Lbs.MiniGames.Games.NumberPull.Tests
{
    public sealed class NumberPullMatchTests
    {
        [Test]
        public void CorrectAnswerMovesOneStepAndIncorrectAnswerDoesNotMove()
        {
            NumberPullMatch match = CreateMatch();

            SubmissionResult incorrect = match.Submit(match.LeftProblem.Answer + 1, null);
            Assert.That(incorrect.Left, Is.EqualTo(SubmissionFeedback.Incorrect));
            Assert.That(match.Balance, Is.Zero);
            Assert.That(match.LeftStats.Attempts, Is.EqualTo(1));
            Assert.That(match.LeftStats.Correct, Is.Zero);

            SubmissionResult correct = match.Submit(match.LeftProblem.Answer, null);
            Assert.That(correct.Left, Is.EqualTo(SubmissionFeedback.Correct));
            Assert.That(match.Balance, Is.EqualTo(-1));
            Assert.That(match.LeftStats.Attempts, Is.EqualTo(2));
            Assert.That(match.LeftStats.Correct, Is.EqualTo(1));
        }

        [Test]
        public void CorrectAnswersSubmittedTogetherAreAtomicAndNeutralizeMovement()
        {
            NumberPullMatch match = CreateMatch();

            SubmissionResult result = match.Submit(match.LeftProblem.Answer, match.RightProblem.Answer);

            Assert.That(result.Left, Is.EqualTo(SubmissionFeedback.Neutralized));
            Assert.That(result.Right, Is.EqualTo(SubmissionFeedback.Neutralized));
            Assert.That(result.BalanceChanged, Is.False);
            Assert.That(match.Balance, Is.Zero);
            Assert.That(match.LeftStats.Correct, Is.EqualTo(1));
            Assert.That(match.RightStats.Correct, Is.EqualTo(1));
        }

        [Test]
        public void MatchStopsExactlyAtConfiguredPullLimit()
        {
            NumberPullMatch match = CreateMatch(target: 2);

            match.Submit(null, match.RightProblem.Answer);
            match.Submit(null, match.RightProblem.Answer);
            SubmissionResult ignored = match.Submit(null, match.RightProblem.Answer);

            Assert.That(match.Balance, Is.EqualTo(2));
            Assert.That(match.Outcome, Is.EqualTo(MatchOutcome.RightWins));
            Assert.That(ignored.Right, Is.EqualTo(SubmissionFeedback.None));
            Assert.That(match.RightStats.Attempts, Is.EqualTo(2));
        }

        [Test]
        public void TimeoutUsesBalanceAndProducesDrawAtCenter()
        {
            NumberPullMatch rightLeading = CreateMatch(duration: 1f);
            rightLeading.Submit(null, rightLeading.RightProblem.Answer);
            rightLeading.Tick(1f);

            NumberPullMatch centered = CreateMatch(duration: 1f);
            centered.Tick(1.5f);

            Assert.That(rightLeading.Outcome, Is.EqualTo(MatchOutcome.RightWins));
            Assert.That(centered.Outcome, Is.EqualTo(MatchOutcome.Draw));
            Assert.That(centered.RemainingSeconds, Is.Zero);
        }

        [Test]
        public void CompletedResultCanOnlyBeConsumedOnce()
        {
            NumberPullMatch match = CreateMatch(target: 1);
            match.Submit(match.LeftProblem.Answer, null);

            bool first = match.TryConsumeResult(out NumberPullResult result);
            bool second = match.TryConsumeResult(out _);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(result.Outcome, Is.EqualTo(MatchOutcome.LeftWins));
            Assert.That(result.LeftStats.Correct, Is.EqualTo(1));
        }

        [Test]
        public void SafeAreaMappingRecalculatesOnlyWhenInputsChange()
        {
            SafeAreaLayoutState layout = new();

            bool initial = layout.TryUpdate(2000, 1000, new LayoutRect(100f, 50f, 1800f, 900f), out LayoutRect first);
            bool unchanged = layout.TryUpdate(2000, 1000, new LayoutRect(100f, 50f, 1800f, 900f), out LayoutRect second);
            bool rotated = layout.TryUpdate(1000, 2000, new LayoutRect(-20f, 100f, 1040f, 1800f), out LayoutRect third);

            Assert.That(initial, Is.True);
            Assert.That(unchanged, Is.False);
            Assert.That(first.X, Is.EqualTo(0.05f));
            Assert.That(first.Y, Is.EqualTo(0.05f));
            Assert.That(first.Width, Is.EqualTo(0.9f));
            Assert.That(first.Height, Is.EqualTo(0.9f));
            Assert.That(second.X, Is.EqualTo(first.X));
            Assert.That(third.X, Is.Zero);
            Assert.That(third.Y, Is.EqualTo(0.05f));
            Assert.That(third.Width, Is.EqualTo(1f));
            Assert.That(third.Height, Is.EqualTo(0.9f));
            Assert.That(rotated, Is.True);
        }

        [TestCase(1920, 1080, 1920f)]
        [TestCase(2000, 922, 2000f)]
        [TestCase(1440, 1080, 1360f)]
        public void CharacterBoundsStayOutsideReservedInputAreas(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            CharacterLayoutBounds bounds = NumberPullBoardLayout.CalculateCharacterBounds(
                screenWidth,
                screenHeight,
                safeAreaWidth);
            ReservedInputLayoutBounds inputs = NumberPullBoardLayout.CalculateReservedInputBounds(
                screenWidth,
                screenHeight,
                safeAreaWidth);

            Assert.That(bounds.Left.Minimum, Is.GreaterThanOrEqualTo(inputs.LeftMaximum));
            Assert.That(bounds.Right.Maximum, Is.LessThanOrEqualTo(inputs.RightMinimum));
        }

        [Test]
        public void RopeIsContainedInCentralGapWithMargins()
        {
            Assert.That(NumberPullBoardLayout.IsRopeContainedWithMargins(), Is.True, "Rope must stay inside central gap with margins.");

            HorizontalLayoutBounds rope = NumberPullBoardLayout.CalculateRopeBounds();
            Assert.That(rope.Minimum, Is.GreaterThan(NumberPullBoardLayout.LeftInputMaximum), "Rope left must not invade left input panel.");
            Assert.That(rope.Maximum, Is.LessThan(NumberPullBoardLayout.RightInputMinimum), "Rope right must not invade right input panel.");
            Assert.That(rope.Minimum, Is.GreaterThanOrEqualTo(NumberPullBoardLayout.CentralStageLeft));
            Assert.That(rope.Maximum, Is.LessThanOrEqualTo(NumberPullBoardLayout.CentralStageRight));
            Assert.That(NumberPullBoardLayout.RopeLeftAnchor, Is.LessThan(NumberPullBoardLayout.LeftCharacterAnchor));
            Assert.That(NumberPullBoardLayout.RopeRightAnchor, Is.GreaterThan(NumberPullBoardLayout.RightCharacterAnchor));
        }

        [Test]
        public void CharacterVerticalAnchorAlignsWithRopeCenter()
        {
            const float handAlignmentTolerance = 0.01f;
            float ropeCenter = (NumberPullBoardLayout.RopeBottomAnchor + NumberPullBoardLayout.RopeTopAnchor) * 0.5f;

            Assert.That(NumberPullBoardLayout.CharacterVerticalAnchor, Is.EqualTo(ropeCenter).Within(handAlignmentTolerance));
        }

        [TestCase(1920, 1080, 1920f)]
        [TestCase(2000, 922, 2000f)]
        [TestCase(1440, 1080, 1360f)]
        [TestCase(1280, 800, 1280f)]
        public void KnotAtMaximumPullStaysInsideRopeWithMargins(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            Assert.That(NumberPullBoardLayout.IsKnotContainedAtMaxPull(screenWidth, screenHeight, safeAreaWidth), Is.True);

            HorizontalLayoutBounds rope = NumberPullBoardLayout.CalculateRopeBounds();
            HorizontalLayoutBounds knot = NumberPullBoardLayout.CalculateKnotCenterBounds(screenWidth, screenHeight, safeAreaWidth);
            Assert.That(knot.Minimum, Is.GreaterThanOrEqualTo(rope.Minimum));
            Assert.That(knot.Maximum, Is.LessThanOrEqualTo(rope.Maximum));
        }

        [TestCase(1920, 1080, 1920f)]
        [TestCase(2000, 922, 2000f)]
        [TestCase(1440, 1080, 1360f)]
        public void CharactersAreSeparatedAtRestAndRemainContained(
            int screenWidth,
            int screenHeight,
            float safeAreaWidth)
        {
            CharacterLayoutBounds bounds = NumberPullBoardLayout.CalculateCharacterBounds(screenWidth, screenHeight, safeAreaWidth);
            float separation = NumberPullBoardLayout.CalculateCharacterSeparation(screenWidth, screenHeight, safeAreaWidth);

            Assert.That(bounds.Left.Maximum, Is.LessThan(bounds.Right.Minimum), "Characters must be on opposite ends of rope without overlapping at rest.");
            Assert.That(separation, Is.GreaterThan(0.04f), "Characters must keep visible separation between central gap ends.");
            Assert.That(bounds.Left.Minimum, Is.GreaterThanOrEqualTo(NumberPullBoardLayout.CentralStageLeft - 0.01f));
            Assert.That(bounds.Right.Maximum, Is.LessThanOrEqualTo(NumberPullBoardLayout.CentralStageRight + 0.01f));

            Assert.That(NumberPullBoardLayout.LeftCharacterAnchor, Is.LessThan(0.5f));
            Assert.That(NumberPullBoardLayout.RightCharacterAnchor, Is.GreaterThan(0.5f));
            Assert.That(NumberPullBoardLayout.RightCharacterAnchor - NumberPullBoardLayout.LeftCharacterAnchor, Is.GreaterThan(0.10f));
        }

        [Test]
        public void SeededGeneratorProducesValidNonNegativeProblemsDeterministically()
        {
            MathProblemGenerator first = new(4182);
            MathProblemGenerator second = new(4182);

            for (int index = 0; index < 250; index++)
            {
                MathProblem a = first.Next();
                MathProblem b = second.Next();

                Assert.That(a.LeftOperand, Is.EqualTo(b.LeftOperand));
                Assert.That(a.RightOperand, Is.EqualTo(b.RightOperand));
                Assert.That(a.Operation, Is.EqualTo(b.Operation));
                Assert.That(a.Answer, Is.InRange(0, MathProblemGenerator.MaximumAnswer));
                if (a.Operation == MathOperation.Subtraction)
                {
                    Assert.That(a.LeftOperand, Is.GreaterThanOrEqualTo(a.RightOperand));
                }
            }
        }

        [TestCase(NumberPullDifficultyTier.LowerPrimary, 4, 105f, 20)]
        [TestCase(NumberPullDifficultyTier.UpperPrimaryAndSecondary, 5, 90f, 100)]
        [TestCase(NumberPullDifficultyTier.PreparatoryHighSchool, 6, 75f, 144)]
        public void DifficultyConfigurationDefinesMatchPacingAndAnswerBounds(NumberPullDifficultyTier tier, int targetPulls, float duration, int maximumMagnitude)
        {
            NumberPullDifficulty difficulty = NumberPullDifficulty.For(tier);

            Assert.That(difficulty.TargetPulls, Is.EqualTo(targetPulls));
            Assert.That(difficulty.DurationSeconds, Is.EqualTo(duration));
            Assert.That(difficulty.MaximumAnswerMagnitude, Is.EqualTo(maximumMagnitude));
        }

        [Test]
        public void LowerPrimaryGeneratesOnlyNonNegativeAdditionAndSubtractionToTwenty()
        {
            MathProblemGenerator generator = new(1452, NumberPullDifficultyTier.LowerPrimary);

            for (int index = 0; index < 500; index++)
            {
                MathProblem problem = generator.Next();
                Assert.That(
                    problem.Operation == MathOperation.Addition || problem.Operation == MathOperation.Subtraction,
                    Is.True);
                Assert.That(problem.Answer, Is.InRange(0, 20));
                Assert.That(problem.LeftOperand, Is.GreaterThanOrEqualTo(0));
                Assert.That(problem.RightOperand, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public void UpperPrimaryAndSecondaryUsesWholeNumberOperationsWithinSupportedInputBounds()
        {
            MathProblemGenerator generator = new(2567, NumberPullDifficultyTier.UpperPrimaryAndSecondary);
            bool sawMultiplication = false;
            bool sawDivision = false;

            for (int index = 0; index < 1000; index++)
            {
                MathProblem problem = generator.Next();
                Assert.That(problem.Answer, Is.InRange(0, 100));
                Assert.That(problem.LeftOperand, Is.GreaterThanOrEqualTo(0));
                Assert.That(problem.RightOperand, Is.GreaterThanOrEqualTo(0));
                if (problem.Operation == MathOperation.Multiplication)
                {
                    sawMultiplication = true;
                    Assert.That(problem.LeftOperand, Is.InRange(2, 10));
                    Assert.That(problem.RightOperand, Is.InRange(2, 10));
                }
                else if (problem.Operation == MathOperation.Division)
                {
                    sawDivision = true;
                    Assert.That(problem.RightOperand, Is.InRange(2, 10));
                    Assert.That(problem.LeftOperand % problem.RightOperand, Is.Zero);
                }
            }

            Assert.That(sawMultiplication, Is.True);
            Assert.That(sawDivision, Is.True);
        }

        [Test]
        public void PreparatoryHighSchoolUsesSignedIntegersAndExactDivisionOnly()
        {
            MathProblemGenerator generator = new(3789, NumberPullDifficultyTier.PreparatoryHighSchool);
            bool sawNegativeAnswer = false;
            bool sawMultiplication = false;
            bool sawDivision = false;

            for (int index = 0; index < 1000; index++)
            {
                MathProblem problem = generator.Next();
                Assert.That(System.Math.Abs(problem.Answer), Is.LessThanOrEqualTo(144));
                sawNegativeAnswer |= problem.Answer < 0;
                if (problem.Operation == MathOperation.Multiplication)
                {
                    sawMultiplication = true;
                }
                else if (problem.Operation == MathOperation.Division)
                {
                    sawDivision = true;
                    Assert.That(problem.RightOperand, Is.Not.Zero);
                    Assert.That(problem.LeftOperand % problem.RightOperand, Is.Zero);
                }
            }

            Assert.That(sawNegativeAnswer, Is.True);
            Assert.That(sawMultiplication, Is.True);
            Assert.That(sawDivision, Is.True);
        }

        private static NumberPullMatch CreateMatch(int target = 5, float duration = 90f)
        {
            return new NumberPullMatch(
                new SequenceGenerator(new MathProblem(2, 3, MathOperation.Addition), new MathProblem(7, 4, MathOperation.Subtraction)),
                new SequenceGenerator(new MathProblem(4, 4, MathOperation.Addition), new MathProblem(9, 2, MathOperation.Subtraction)),
                target,
                duration);
        }

        private sealed class SequenceGenerator : IMathProblemGenerator
        {
            private readonly List<MathProblem> problems;
            private int index;

            public SequenceGenerator(params MathProblem[] values)
            {
                problems = new List<MathProblem>(values);
            }

            public MathProblem Next()
            {
                MathProblem problem = problems[index % problems.Count];
                index++;
                return problem;
            }
        }
    }
}
