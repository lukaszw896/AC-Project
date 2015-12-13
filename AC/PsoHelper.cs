using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC
{
    public class PsoHelper
    {
        public static Random random = new Random();


        /// <summary>
        /// Function checking whether two words are finishing computations in the same state
        /// </summary>
        /// <param name="automata"></param>
        /// <param name="word1"></param>
        /// <param name="word2"></param>
        /// <returns></returns>
       public static bool AreWordsRelated(Automat automata, List<int> word1, List<int> word2)
        {
            int numberOfStates = automata.getStatesNumber();
            List<int[]> transitionTableList = new List<int[]>();
            transitionTableList = automata.getTransitionTableList();
            int word1Length = word1.Count;
            int word2Length = word2.Count;

            int currentState1 = 0, currentState2 = 0;

            int[] transitionTable;
            for (int i = 0; i < word1Length; i++)
            {
                transitionTable = transitionTableList[word1[i]];
                for (int j = 0; j < numberOfStates; j++)
                {
                    if (transitionTable[currentState1] == j)
                    {
                        currentState1 = j;
                        break;
                    }
                }
            }
            for (int i = 0; i < word2Length; i++)
            {
                transitionTable = transitionTableList[word2[i]];
                for (int j = 0; j < numberOfStates; j++)
                {
                    if (transitionTable[currentState2] == j)
                    {
                        currentState2 = j;
                        break;
                    }
                }
            }

            return (currentState1 == currentState2);
        }


        /// <summary>
        /// Function calculating minimal number of states of automaton from given tool. We're using Myhill-Nerode 
        /// lemma about Equivalence classes and ending states after computation of words
        /// </summary>
        /// <param name="idealAutomat"></param>
        /// <param name="learningSetOfWords"></param>
        /// <returns></returns>
       public static int MinimumNumberOfStates(Automat idealAutomat, List<List<int>> learningSetOfWords)
       {
           int states = 0;
           int wordAmount = learningSetOfWords.Count;
           List<List<List<int>>> EQClasses = new List<List<List<int>>>();

           //dodaje jedna klasie bo zawsze jest jedna
           List<List<int>> temp = new List<List<int>>();
           temp.Add(learningSetOfWords[0]);
           EQClasses.Add(temp);

           for (int i = 1; i < wordAmount; i++)
           {
               bool flag = false;
               for (int j = 0; j < EQClasses.Count; j++)
               {
                   List<List<int>> currentClass = EQClasses[j];
                   for (int w = 0; w < currentClass.Count; w++)
                   {
                       if (currentClass.Count > 0)
                       {
                           if (PsoHelper.AreWordsRelated(idealAutomat, learningSetOfWords[i], currentClass[w]))
                           {
                               flag = true;
                               EQClasses[j].Add(learningSetOfWords[i]);
                           }
                           break;
                       }
                   }
                   if (flag == true)
                   {
                       break;
                   }
               }
               if (flag == false)
               {
                   List<List<int>> tmp = new List<List<int>>();
                   tmp.Add(learningSetOfWords[i]);
                   EQClasses.Add(tmp);
               }
           }

           states = EQClasses.Count;

           return states;
       }

        /// <summary>
        /// Function returning the state at which the computations will end after reading all word
        /// </summary>
        /// <param name="automata"></param>
        /// <param name="word"></param>
        /// <returns></returns>
       public static int WordComputationFinishingState(Automat automata, List<int> word)
       {
           int numberOfStates = automata.getStatesNumber();
           List<int[]> transitionTableList = new List<int[]>();
           transitionTableList = automata.getTransitionTableList();
           int word1Length = word.Count;


           int currentState1 = 0;

           int[] transitionTable;
           for (int i = 0; i < word1Length; i++)
           {
               transitionTable = transitionTableList[word[i]];
               for (int j = 0; j < numberOfStates; j++)
               {
                   if (transitionTable[currentState1] == j)
                   {
                       currentState1 = j;
                       break;
                   }
               }
           }

           return currentState1;
       }



        /// <summary>
        /// Function computing error value for a given automaton/particle.
        /// </summary>
        /// <param name="Words"></param>
        /// <param name="particle"></param>
        /// <param name="pairsOfRelation"></param>
        /// <returns></returns>
       public static double CalculateParticleError(List<List<int>> Words, Automat particle,int[][] pairsOfRelation)
       {
           double error = 0.0;

           int[][] currentParticlePairs;
           currentParticlePairs = new int[Words.Count][];

           for (int j = 0; j < Words.Count; j++)
           {
               currentParticlePairs[j] = new int[Words.Count];
           }

           int[] finishingStates = new int[Words.Count];


           //for (int i = 0; i < Words.Count; i++)
           Parallel.For(0, Words.Count, i =>
           {
               int tmp = PsoHelper.WordComputationFinishingState(particle, Words[i]);
               finishingStates[i] = tmp;
           }
           );

           var watch = Stopwatch.StartNew();
           //////
           double errorCounter = 0.0;

           for (int i = 0; i < Words.Count; i++)
           {
               for (int j = i + 1; j < Words.Count; j++)
               {
                   if (finishingStates[i] == finishingStates[j])
                   {
                       if (pairsOfRelation[i][j] == 0)
                       {
                           errorCounter = errorCounter + 1.0;
                       }
                   }
                   else
                   {
                       if (pairsOfRelation[i][j] == 1)
                       {
                           errorCounter = errorCounter + 1.0;
                       }
                   }

               }
           }

           watch.Stop();
           var elapsedMs = watch.ElapsedMilliseconds;
           //Console.WriteLine("Related Words checking execution time: " + elapsedMs);       

           error = (errorCounter / ((Words.Count * Words.Count) - Words.Count)) * 100.0;

           return error;
       }


        /// <summary>
        /// In this function we are changing values of automaton transition function to discrete values.
        /// For each letter in alphabet we want exactly on transtion from state to state.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="speed"></param>
        /// <param name="roundparam"></param>
        /// <param name="_statesNumber"></param>
        /// <param name="_alphabetLength"></param>
        /// <returns></returns>
       public static List<double> AutomatonDiscretisation(List<double> vector, List<double> speed, double roundparam, int _statesNumber, int _alphabetLength)
       {
           List<double> output = new List<double>();
           int index = -1;
           for (int i = 0; i < _alphabetLength; i++)
           {
               for (int j = 0; j < _statesNumber; j++)
               {
                   ArrayList onesIndex = new ArrayList();
                   int maxIndex = -1;
                   double maxSpeed = -1.0;
                   int maxMinIndex = -1;
                   double zeroMaxSpeed = -1.0;
                   for (int k = 0; k < _statesNumber; k++)
                   {
                       index++;
                       output.Add(0);
                       if ((double)vector[index] > roundparam)
                       {
                           if ((double)speed[index] > maxSpeed)
                           {
                               if (maxIndex != -1)
                               {
                                   vector[maxIndex] = roundparam - 0.3;
                               }
                               maxSpeed = (double)speed[index];
                               maxIndex = index;
                           }
                           else
                           {
                               vector[index] = roundparam - 0.3;
                           }
                       }
                       if (maxIndex == -1 && (double)speed[index] > zeroMaxSpeed)
                       {
                           zeroMaxSpeed = (double)speed[index];
                           maxMinIndex = index;
                       }
                   }
                   if (maxIndex != -1)
                   {
                       output[maxIndex] = 1;
                   }
                   else
                   {
                       output[maxMinIndex] = 1;
                   }
               }
           }

           return output;
       }


       /// <summary>
       /// aplikowane predkosci wedlug wzoru z dokumentacji
       /// nV = v + c1 *random * (local - position) + c2 * random*(global - position)
       /// random z (0;1)
       /// </summary>
       /// 
       public static List<double> CalculateVelocity(double maxSpeed, List<double> globalBest, List<double> localBest, List<double> currentVelocity, List<double> currentPosition, double c1, double c2)
       {
           List<double> newVelocity = new List<double>();

           //c1 *random * (local - position)
           double part1 = c1 * GetRandomNumber();
           double part2 = c2 * GetRandomNumber();

           for (int i = 0; i < localBest.Count; i++)
           {
               /*double tmp1 = ((double.Parse(localBest[i]+"") - double.Parse(""+currentPosition[i])) * part1);
               double tmp2 = ((double.Parse(""+globalBest[i]) - double.Parse(""+currentPosition[i])) * part2);*/

               double tmp1 = ((localBest[i] - currentPosition[i]) * part1);
               double tmp2 = ((globalBest[i] - currentPosition[i]) * part2);

               double speed = currentVelocity[i] + tmp1 + tmp2;

               if (speed > maxSpeed)
               {
                   speed = maxSpeed;
               }
               else if (speed < (0.0 - maxSpeed))
               {
                   speed = (0.0 - maxSpeed);
               }


               newVelocity.Add(speed);
           }

           return newVelocity;
       }


       public static double GetRandomNumber()
       {
           return random.NextDouble() * (1.0 - 0.0) + 0.0;
       }
    }
}
