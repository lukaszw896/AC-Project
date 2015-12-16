using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AC;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class PsoHelperUnitTests
    {
        [TestMethod]
        public void MinimumNumberOfStatesUnitTests()
        {
            //Arrange
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            Automat automaton = new Automat(2, 2, transition);

            List<List<int>> learningSetOfWords = new List<List<int>>();

            List<int> word1 = new List<int>();
            List<int> word2 = new List<int>();

            word1.Add(0);
            word1.Add(0);
            word1.Add(0);

            word2.Add(0);
            word2.Add(1);
            word2.Add(0);
            word2.Add(1);

            learningSetOfWords.Add(word1);
            learningSetOfWords.Add(word2);


            //Act

            int minNumOfStatesComputed = PsoHelper.MinimumNumberOfStates(automaton, learningSetOfWords);
            
            //Assert

            Assert.AreEqual(2, minNumOfStatesComputed);
        }

        [TestMethod]
        public void AreWordsRelatedUnitTests()
        {
            //Arrange
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            Automat automaton = new Automat(2, 2, transition);

            List<int> word1 = new List<int>();
            List<int> word2 = new List<int>();
            List<int> word3 = new List<int>();
            word1.Add(0);
            word1.Add(0);
            word1.Add(0);

            word2.Add(0);
            word2.Add(1);
            word2.Add(0);
            word2.Add(1);

            word3.Add(0);
            word3.Add(1);
            word3.Add(1);

            //Act

            bool word1AndWord2Rel = PsoHelper.AreWordsRelated(automaton, word1, word2);

            bool word2AndWord3Rel = PsoHelper.AreWordsRelated(automaton, word2, word3);

            //Assert

            Assert.AreEqual(false, word1AndWord2Rel);
            Assert.AreEqual(true, word2AndWord3Rel);

        }

        [TestMethod]
        public void WordComputationFinishingStateUnitTests()
        {
            //Arrange
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            Automat automaton = new Automat(2, 2, transition);

            List<int> word1 = new List<int>();
            List<int> word2 = new List<int>();
            List<int> word3 = new List<int>();
            word1.Add(0);
            word1.Add(0);
            word1.Add(0);

            word2.Add(0);
            word2.Add(1);
            word2.Add(0);
            word2.Add(1);

            word3.Add(0);
            word3.Add(1);
            word3.Add(1);

            //Act

            int word1FinishingState = PsoHelper.WordComputationFinishingState(automaton, word1);
            int word2FinishingState = PsoHelper.WordComputationFinishingState(automaton, word2);
            int word3FinishingState = PsoHelper.WordComputationFinishingState(automaton, word3);

            //Assert

            Assert.AreEqual(0, word1FinishingState);
            Assert.AreEqual(1, word2FinishingState);
            Assert.AreEqual(1, word3FinishingState);
        }

        [TestMethod]
        public void CalculateParticleErrorUnitTests()
        {
            //Arrange
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            Automat automaton = new Automat(2, 2, transition);

            List<List<int>> words = new List<List<int>>();

            List<int> word1 = new List<int>();
            List<int> word2 = new List<int>();

            word1.Add(0);
            word1.Add(0);
            word1.Add(0);

            word2.Add(0);
            word2.Add(1);
            word2.Add(0);
            word2.Add(1);

            words.Add(word1);
            words.Add(word2);

            int[][] pairsOfRelation = new int[2][];
            pairsOfRelation[0] = new int[2];
            pairsOfRelation[0][0] = 1;
            pairsOfRelation[0][1] = 1;
            pairsOfRelation[1] = new int[2];
            pairsOfRelation[1][0] = 1;
            pairsOfRelation[1][1] = 1;

            //Act

            double calculatedParticleError = PsoHelper.CalculateParticleError(words, automaton, pairsOfRelation);

            //Assert

            Assert.AreEqual(100.0, calculatedParticleError);
        }

        [TestMethod]
        public void GetRandomNumberUnitTests()
        {
            //Act
            double randomNumber = PsoHelper.GetRandomNumber();
            
            //Assert

            Assert.IsTrue(randomNumber <= 1.0, "Random number is grater than 1.0");
            Assert.IsTrue(randomNumber >= 0.0, "Random number is smaller than 0.0");
        }

        [TestMethod]
        public void FindrelationpairtsUnitTests()
        {
            //Arrange

            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            Automat automaton = new Automat(2, 2, transition);

            List<List<int>> words = new List<List<int>>();

            List<int> word1 = new List<int>();
            List<int> word2 = new List<int>();
            List<int> word3 = new List<int>();
            word1.Add(0);
            word1.Add(0);
            word1.Add(0);

            word2.Add(0);
            word2.Add(1);
            word2.Add(0);
            word2.Add(1);

            word3.Add(0);
            word3.Add(1);
            word3.Add(1);

            words.Add(word1);
            words.Add(word2);
            words.Add(word3);

            int[][] expectedPairsOfRelation = new int[3][];
            expectedPairsOfRelation[0] = new int[3];
            expectedPairsOfRelation[1] = new int[3];
            expectedPairsOfRelation[2] = new int[3];
            expectedPairsOfRelation[0][0] = 1;
            expectedPairsOfRelation[0][1] = 0;
            expectedPairsOfRelation[0][2] = 0;
            expectedPairsOfRelation[1][0] = 0;
            expectedPairsOfRelation[1][1] = 1;
            expectedPairsOfRelation[1][2] = 1;
            expectedPairsOfRelation[2][0] = 0;
            expectedPairsOfRelation[2][1] = 1;
            expectedPairsOfRelation[2][2] = 1;


            //Act

            int[][] calculatedPairsOfRelation = PsoHelper.FindRelationPairs(words, automaton);
            //Assert

            CollectionAssert.AreEqual(expectedPairsOfRelation[0], calculatedPairsOfRelation[0]);
            CollectionAssert.AreEqual(expectedPairsOfRelation[1], calculatedPairsOfRelation[1]);
            CollectionAssert.AreEqual(expectedPairsOfRelation[2], calculatedPairsOfRelation[2]);

        }
    }
}
