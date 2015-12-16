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
    }
}
