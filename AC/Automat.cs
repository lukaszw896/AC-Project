﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

namespace AC
{
    /// <summary>
    /// Klasa Automat z wszystkimi polami i metodami
    /// </summary>
    public class Automat
    {
        private int statesNumber;
        private int alphabetLength;
        private List<int[]> transitionTableList;


        /// <summary>
        /// empty constructor
        /// </summary>
        public Automat ()
        {
            statesNumber = 0;
            alphabetLength = 0;
            transitionTableList = new List<int[]>();
        }


        /// <summary>
        /// Automat constructor with 3 arguments
        /// </summary>
        /// <param name="_states"></param>
        /// <param name="_alphabet"></param>
        /// /// <param name="_transition"></param>
        public Automat(int _states, int _alphabet, List<int[]> _transition)
        {
            statesNumber = _states;
            alphabetLength = _alphabet;
            transitionTableList = new List<int[]>();
            transitionTableList = _transition;
        }

        /// <summary>
        /// Automat constructor from txt string
        /// </summary>
        /// <param name="input"></param>

       /* public Automat(String input)
        {
            string[] inputs = input.Split(',');
            statesNumber = int.Parse(inputs[0]);
            alphabetLength = int.Parse(inputs[1]);

            transitionTableList = new List<int[][]>();
            for (int i = 0; i < alphabetLength; i++)
            {
                //init transition table
                int[][] transitionTable = new int[statesNumber][];

                for (int j = 0; j < statesNumber; j++)
                {
                    transitionTable[j] = new int[statesNumber];
                }

                for (int j = 0; j < statesNumber; j++)
                {
                    for (int k = 0; k < statesNumber; k++)
                    {
                        int index = 2 + (i * statesNumber) + j;
                        int tmp = int.Parse(inputs[index]);
                        if (tmp == k)
                        {
                            transitionTable[j][k] = 1;
                        }
                        else
                        {
                            transitionTable[j][k] = 0;
                        }

                    }
                }
                transitionTableList.Add(transitionTable);

            }
        }*/

        /// <summary>
        /// Method returns a List which represents automat as vector
        /// </summary>
        public List<double> toVector()
        {
            List<double> vector = new List<double>();

            for (int i = 0; i < alphabetLength; i++)
            {
                int[] transitionTable = transitionTableList[i];

                for (int j = 0; j < statesNumber; j++)
                {
                    for (int k = 0; k < statesNumber; k++)
                    {
                        if(transitionTable[j] == k)
                        {
                            vector.Add(1);
                        }
                        else
                        {
                            vector.Add(0);
                        }
                    }
                }
            }
            return vector;
        }



        /// <summary>
        /// Method returns instance of Automat from input string
        /// </summary>
        /// <param name="input"></param>
        public void fromString(String input)
        {
            string[] inputs = input.Split(',');
            statesNumber = int.Parse(inputs[0]);
            alphabetLength = int.Parse(inputs[1]);

            transitionTableList = new List<int[]>();
            for (int i = 0; i < alphabetLength; i++)
            {
                //init transition table
                List<int> symbolsForletter = new List<int>();
                int[] transitionTable = new int[statesNumber];

                for (int j = 0; j < statesNumber; j++)
                {
                    symbolsForletter.Add(int.Parse(inputs[(j * alphabetLength) + 2 + i]));
                }

                for (int j = 0; j < statesNumber; j++)
                {
                    for (int k = 0; k < statesNumber; k++)
                    {
                        //int index = 2 + (i * statesNumber) + j;
                        //int index = 2 + (i * statesNumber) + j;
                        int tmp = symbolsForletter[j] - 1;
                        if (tmp == k)
                        {
                            transitionTable[j] = k;
                        }
                    }
                }
                transitionTableList.Add(transitionTable);

            }

           // Automat returned = new Automat(statesNumber, alphabetLength, transitionTableList);
            //return returned;
        }

        /// <summary>
        /// method to create automata from PSO vector
        /// </summary>

        public static Automat fromVector(String vector, int _statesNumber, int _alphabetLength)
        {
            char[] inputs = vector.ToCharArray();

            List<int> data = new List<int>();

            for (int i = 0; i < inputs.Length; i++)
            {
                /*char symbol = inputs[i];
                data.Add(int.Parse("" + symbol));*/

                char symbol = inputs[i];
                data.Add(int.Parse(inputs[i].ToString()));
            }

            //statesNumber = _statesNumber;
            //alphabetLength = _alphabetLength;


            List<int[]> _transitionTableList = new List<int[]>();
            for (int i = 0; i < _alphabetLength; i++)
            {
                //init transition table
                int[] transitionTable = new int[_statesNumber];

                for (int j = 0; j < _statesNumber; j++)
                {
                    for (int k = 0; k < _statesNumber; k++)
                    {
                        int index = (i * _statesNumber * _statesNumber) + (j * _statesNumber) + k;
                        //int tmp = int.Parse(inputs[index]);
                        if ((int)data[index] == 1)
                        {
                            transitionTable[j] = k; ;
                        }
                    }
                }
                _transitionTableList.Add(transitionTable);

            }

            Automat returned = new Automat(_statesNumber, _alphabetLength, _transitionTableList);
            return returned;
        }

        public int getStatesNumber()
        {
            return statesNumber;
        }
        public int getAlphabetLength()
        {
            return alphabetLength;
        }
        public List<int[]> getTransitionTableList()
        {
            return transitionTableList;
        }


        public void setStatesNumber(int _states)
        {
            statesNumber = _states;
        }
        public void setAlphabetLength(int _alphabet)
        {
            alphabetLength = _alphabet;
        }
        public void setTransitionTableList(List<int[]> _transition)
        {
            transitionTableList.Clear();
            transitionTableList = new List<int[]>();
            transitionTableList = _transition;
        }
    }
}
