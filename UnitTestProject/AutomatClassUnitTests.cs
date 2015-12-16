using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AC;

namespace UnitTestProject
{
    [TestClass]
    public class AutomatClassUnitTests
    {
        [TestMethod]
        public void ToVectorUnitTest()
        {
            //Arrange
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);

            Automat automaton = new Automat(2, 2, transition);

            List<double> answer = new List<double>();
            answer.Add(1);
            answer.Add(0);
            answer.Add(0);
            answer.Add(1);
            answer.Add(0);
            answer.Add(1);
            answer.Add(0);
            answer.Add(1);
            //Act

            List<double> testVector = automaton.toVector();
       
            //Assert

            CollectionAssert.AreEqual(answer, testVector);
        }

        [TestMethod]
        public void FromStringUnitTest()
        {
            //Arrange
            Automat automaton = new Automat();

            string automatonString = "2,2,1,2,2,2";

            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);

            //Act
            automaton.fromString(automatonString);

            //Assert

            Assert.AreEqual(2, automaton.AlphabetLength);
            Assert.AreEqual(2, automaton.AlphabetLength);
            CollectionAssert.AreEqual(transition[1], automaton.TransitiontableList[1]);
            CollectionAssert.AreEqual(transition[0], automaton.TransitiontableList[0]);
        }


        [TestMethod]
        public void FromVectorUnitTest()
        {
            List<int[]> transition = new List<int[]>();
            int[] a = { 0, 1 };
            int[] b = { 1, 1 };
            transition.Add(a);
            transition.Add(b);
            //Arrange
            string vector = "10010101";

            //Act

            Automat calculated = Automat.fromVector(vector, 2, 2);

            //Assert

            Assert.AreEqual(2, calculated.StatesNumber);
            Assert.AreEqual(2, calculated.StatesNumber);
            CollectionAssert.AreEqual(transition[1], calculated.TransitiontableList[1]);
            CollectionAssert.AreEqual(transition[0], calculated.TransitiontableList[0]);
        }
    }
}
