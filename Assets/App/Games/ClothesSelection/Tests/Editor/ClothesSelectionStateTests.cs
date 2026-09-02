using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.ClothesSelection;
using NUnit.Framework;
namespace Lbs.MiniGames.Tests
{
 public sealed class ClothesSelectionStateTests
 {
  [Test] public void Gloves_IsTheSemanticCorrectAnswer() => Assert.IsTrue(ClothesSelectionRule.IsCorrect("gloves"));
  [Test] public void FirstTouchLocksUntilResolution() { var state=new SelectionGameState(); Assert.IsFalse(state.Select("heel", ClothesSelectionRule.CorrectAnswer)); Assert.IsFalse(state.Select("gloves", ClothesSelectionRule.CorrectAnswer)); Assert.AreEqual(SelectionPhase.ResolvingIncorrect,state.Phase); }
  [Test] public void IncorrectAnswerUnlocksRetryAndUsesLowerTier() { var state=new SelectionGameState(); state.Select("shoes", ClothesSelectionRule.CorrectAnswer); state.FinishIncorrect(); Assert.AreEqual(SelectionPhase.Ready,state.Phase); Assert.AreEqual(4,state.Score); Assert.AreEqual(1,state.StarCount); }
   [Test] public void CorrectAnswerLocksAndUsesPerfectTier() { var state=new SelectionGameState(); Assert.IsTrue(state.Select("gloves", ClothesSelectionRule.CorrectAnswer)); Assert.AreEqual(SelectionPhase.Celebrating,state.Phase); Assert.AreEqual(8,state.Score); Assert.AreEqual(2,state.StarCount); }
   [Test] public void FinalInputRemainsLockedUntilResultEntranceCompletes() { var state=new SelectionGameState(); state.Select("gloves", ClothesSelectionRule.CorrectAnswer); state.FinishCelebration(); Assert.IsFalse(state.AcceptFinalInput()); state.EnableFinalInput(); Assert.IsTrue(state.AcceptFinalInput()); }
 }
}
