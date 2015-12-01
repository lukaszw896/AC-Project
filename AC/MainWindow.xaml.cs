﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Automat idealAutomat;

        List<List<int>> setOfWords;
        List<List<int>> LearningSetOfWords;
        List<List<int>> TrainingSetOfWords;

        int[][] pairsOfRelation;
        static Random random = new Random();
        double speedLowerBound = -0.2;
        double speedUpperBound = 0.2;
        public MainWindow()
        {
            InitializeComponent();

            setOfWords = new List<List<int>>();
          
            idealAutomat = new Automat();
        }

        /// <summary>
        /// ladujemy plik z automatem
        /// </summary>
        private void LoadAutomata_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                LoadAutomata(dlg.FileName);
                MessageBox.Show("Automata Loaded!");
            }
            else
            {
                MessageBox.Show("Loading Aborted!");
                return;
            }
        }


        /// <summary>
        /// funkcja wyciaga z pliku z automatem dane, i tworzy instancje automatu
        /// </summary>
        void LoadAutomata(String path)
        {
            String input = File.ReadAllText(path);

            Automat automata = new Automat();
            automata.fromString(input);
            Console.WriteLine("zaladowano automat");
            //gotowy automat : mamy funkcje stany i alfabet        
            idealAutomat = automata;

            //ArrayList test = automata.toVector();
        }
        /// <summary>
        /// Otwieramy okno w ktorym generujemy plik slow
        /// </summary>
        private void CreateSet_Click(object sender, RoutedEventArgs e)
        {
            GeneratorWindow generator = new GeneratorWindow();

            Nullable<bool> result =  generator.ShowDialog();
            
            if (result == true)
            {
                setOfWords.Clear();
                setOfWords = generator.getWords();
                splitWordsToSets();
                Console.WriteLine("Words Generated");
            }

        }

        /// <summary>
        /// ladujemy plik z slowami
        /// </summary>
        private void LoadSet_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                LoadWordSet(dlg.FileName);
                MessageBox.Show("Word Set Loaded!");
            }
            else
            {
                MessageBox.Show("Loading Aborted!");
                return;
            }
        }

        void LoadWordSet(String path)
        {
            String input = File.ReadAllText(path);

            List<string> words = new List<string>(input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            words.RemoveAt(words.Capacity - 1);
            setOfWords.Clear();
            for (int i = 0; i < words.Count; i++)
            {
                List<int> word = new List<int>();

                foreach (char a in words[i])
                {
                    word.Add(int.Parse(a.ToString()));
                }
                setOfWords.Add(word);
            }

            Console.WriteLine("zaladowano slowa ( " + setOfWords.Count + " )");
            splitWordsToSets();
        }

        void splitWordsToSets()
        {
            LearningSetOfWords = new List<List<int>>();
            TrainingSetOfWords = new List<List<int>>();

            LearningSetOfWords.Add(setOfWords[0]);
            TrainingSetOfWords.Add(setOfWords[0]);
            if (setOfWords.Count >= 1)
            {
                for (int i = 1; i < setOfWords.Count; i+=2)
                {
                    LearningSetOfWords.Add(setOfWords[i]);
                    TrainingSetOfWords.Add(setOfWords[i+1]);
                }
            }
        }

        /// <summary>
        /// tutaj oba slowa puszczamy przez automat i sprawdzamy czy sie koncza w tym samym stanie
        /// </summary>
        bool areWordsRelated(Automat automata, List<int> word1, List<int> word2)
        {
            int numberOfStates = automata.getStatesNumber();
            List<int[][]> transitionTableList = new List<int[][]>();
            transitionTableList = automata.getTransitionTableList();
            int word1Length = word1.Count;
            int word2Length = word2.Count;

            int currentState1 = 0, currentState2 = 0;

            int[][] transitionTable;
            for (int i = 0; i < word1Length; i++)
            {
                transitionTable = transitionTableList[word1[i]];
                for (int j = 0; j < numberOfStates; j++)
                {
                    if (transitionTable[currentState1][j] == 1)
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
                    if (transitionTable[currentState2][j] == 1)
                    {
                        currentState2 = j;
                        break;
                    }
                }
            }

            return (currentState1 == currentState2);
        }

        /// <summary>
        /// wyliczamy najmniejsza liczbe klas abstrakcji
        /// </summary>
        int minValueOfStates()
        {
            /*
             * FOR every word W in set DO:
                Set FLAG to false;
                 FOR every equivalence class EQ in set of classes DO:
                     IF W is in relation with any word in EQ DO:
                      add W to EQ;
                      set FLAG to true;
                      break;
                    END IF
                END FOR
             IF FLAG is false DO:
             add new class EQ to set of classes;
             add W to new class EQ;
            END OF
            END FOR
             */
            int states = 0;
            int wordAmount = setOfWords.Count;
            List<List<List<int>>> EQClasses = new List<List<List<int>>>();

            //dodaje jedna klasie bo zawsze jest jedna
            List<List<int>> temp = new List<List<int>>();
            temp.Add(setOfWords[0]);
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
                            if (areWordsRelated(idealAutomat, setOfWords[i], currentClass[w]))
                            {
                                flag = true;
                                EQClasses[j].Add(setOfWords[i]);
                                break;
                            }
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
                    tmp.Add(setOfWords[i]);
                    EQClasses.Add(tmp);
                }
            }

            states = EQClasses.Count;

            return states;
        }

        /// <summary>
        /// uzypelniamy tutaj tabele parami slow ktore sa w relacji
        /// </summary>
        /// 
        void findRelationPairs(List<List<int>> words)
        {
            //pairsOfRelation
            pairsOfRelation = new int[words.Count][];

            for (int j = 0; j < words.Count; j++)
            {
                pairsOfRelation[j] = new int[words.Count];
            }

            for (int i = 0; i < words.Count; i++)
            {
                for (int j = 0; j < words.Count; j++)
                {
                    if(i != j)
                    {
                        if (areWordsRelated(idealAutomat, words[i], words[j]) == true)
                        {
                            pairsOfRelation[i][j] = 1;
                            pairsOfRelation[j][i] = 1;
                        }
                        else
                        {
                            pairsOfRelation[i][j] = 0;
                            pairsOfRelation[j][i] = 0;
                        }
                    }
                    else
                    {
                        pairsOfRelation[i][j] = 1;
                        pairsOfRelation[j][i] = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Error liczy sie jako stosunek par w relacji z particle automatu
        /// do automatu idealnego. Wynik jest procentem bledu
        /// </summary>
        /// 
        double ErrorCalculation(List<List<int>> Words , Automat particle)
        {
            double error = 0.0;

            int[][] currentParticlePairs;
            currentParticlePairs = new int[Words.Count][];

            for (int j = 0; j < Words.Count; j++)
            {
                currentParticlePairs[j] = new int[Words.Count];
            }

            var watch = Stopwatch.StartNew();
            //////
            for (int i = 0; i < Words.Count; i++)
            {
                for (int j = i + 1; j < Words.Count; j++)
                {
                    if (areWordsRelated(particle, Words[i], Words[j]) == true)
                    {
                        currentParticlePairs[i][j] = 1;
                        currentParticlePairs[j][i] = 1;
                    }
                    else
                    {
                        currentParticlePairs[i][j] = 0;
                        currentParticlePairs[j][i] = 0;
                    }

                }
            }
            //////
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
         //   Console.WriteLine("Related Words checking execution time: " + elapsedMs);

            //relacjie idealnego automatu mam w globalnej zmiennej a teraz
            //policzono relacje dla particla . Teraz mamy dwie tabele i je porownujemy
            //i liczymy roznice
            

            watch = Stopwatch.StartNew();
            //////
            double errorCounter = 0.0;
            for (int i = 0; i < Words.Count; i++)
            {
                for (int j = 0; j < Words.Count; j++)
                {
                    if (i != j && pairsOfRelation[i][j] != currentParticlePairs[i][j])
                    {
                        errorCounter = errorCounter + 1.0;
                    }
                }
            }
            //////
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            //Console.WriteLine("ErrorChecking execution time: " + elapsedMs);



            errorCounter = errorCounter / 2.0 ;

            error = (errorCounter / ((Words.Count * Words.Count) - Words.Count)) * 100.0;

            return error;
        }

        /// <summary>
        /// PSO ^^
        /// </summary>
        /// 
        void PSO()
        {

            Console.WriteLine("Learning set size : "+ LearningSetOfWords.Count + ", Training set size: " + TrainingSetOfWords.Count+ " out of total words : "+ setOfWords.Count);
            List<List<int>> slowa = new List<List<int>>();
            slowa = LearningSetOfWords;

            speedLowerBound = double.Parse(speedLowerBoundTxt.Text, CultureInfo.InvariantCulture);
            speedUpperBound = double.Parse(speedUpperBoundTxt.Text, CultureInfo.InvariantCulture);

            double maxSpeed = double.Parse(maxSpeedTxt.Text, CultureInfo.InvariantCulture);

            int iterationToRandomise = int.Parse(errorSameRepetition.Text);

            List<Automat> BestAutomatForStates = new List<Automat>();
            List<double> BestErrorsForAutomats = new List<double>();

            List<double> BestErrorsForAutomats2Set = new List<double>();

            findRelationPairs(slowa);
            double roundAt = double.Parse(RountAtTxt.Text, CultureInfo.InvariantCulture);

            int particlesNumber = int.Parse(ParticleAmountTxt.Text);
            int maxIteration = int.Parse(IterationTxt.Text);
            double c2 = double.Parse(c1Txt.Text, CultureInfo.InvariantCulture);
            double c1 = double.Parse(c2Txt.Text, CultureInfo.InvariantCulture);
            int numberOfNeighbors =int.Parse(neighboursTxt.Text);

            int maxStopError = int.Parse(maxErrIterationTxt.Text);
            int particleRandomNumber = int.Parse(particlerandomnumber.Text);


            List<List<double>> particlesPos = new List<List<double>>();
            List<List<double>> particlesVel = new List<List<double>>();
            List<List<double>> particlesBestPos = new List<List<double>>();
            List<double> particleBest = new List<double>();
            List<int> particlesStopNumber = new List<int>();

            
            List<int> particlesLocBest = new List<int>();
            int particlesGlobBest = 0;
            List<double> particleError = new List<double>();
            int dimensions = 0;
            int currentStateNumber = minValueOfStates() - 1;

            double errTolerance = double.Parse(ToleranceTxt.Text, CultureInfo.InvariantCulture);

            int maxStateNumber = int.Parse(MaxstatesTXT.Text);

            bool continuePSO = true;
            bool addStates = true;
            int counter = 0;

            double lastErrorValue = 0;
            double errorRepeatCounter = 0;

            bool freez = freezGlobal.IsChecked.Value;

            while (continuePSO == true)
            {

                if (addStates == true)
                {
                    counter = 0;
                    currentStateNumber++;

                    if (currentStateNumber >= maxStateNumber)
                    {
                        addStates = false;
                        continuePSO = false;
                        break;
                    }



                    dimensions = (int)Math.Pow(currentStateNumber, 2.0) * (int)idealAutomat.getAlphabetLength();

                    particlesPos.Clear();
                    particlesVel.Clear();
                    particlesLocBest.Clear();
                    particlesGlobBest = 0;
                    particleError.Clear();
                    particleBest.Clear();
                    particlesStopNumber.Clear();
                    particlesBestPos.Clear();


                    //losujemy pozycje i predkosc particles
                    for (int i = 0; i < particlesNumber; i++)
                    {
                        List<double> singleVector = new List<double>();
                        List<double> singleSpeedVector = new List<double>();

                        for (int j = 0; j < dimensions; j++)
                        {
                            double randomVal = GetRandomNumber();
                            double randomVal2 = GetRandomParticleSpeed(speedLowerBound,speedUpperBound);
                            singleVector.Add(randomVal);
                            singleSpeedVector.Add(randomVal2);
                        }
                        particlesPos.Add(singleVector);
                        particlesVel.Add(singleSpeedVector);
                        particlesLocBest.Add(0);
                        particleError.Add(0.0);
                        particleBest.Add(100.0);
                        particlesStopNumber.Add(0);
                        particlesBestPos.Add(singleVector);
                    }

                    addStates = false;
                }

                var watch = Stopwatch.StartNew();
                //////
                //ustalanie errorow
                particleError.Clear();
                for (int i = 0; i < particlesNumber; i++)
                {

                    List<double> discretePosition = new List<double>();
                   
                    discretePosition = makeVectorDiscrete(particlesPos[i], particlesVel[i], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                    double error = 0.0;

                   // watch = Stopwatch.StartNew();
                    //////
                    Automat currentParticle = new Automat();
                    currentParticle = Automat.fromVector(zListyNaStringa(discretePosition), currentStateNumber, (int)idealAutomat.getAlphabetLength());
                    //////
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //Console.WriteLine("fromVector execution time: " + elapsedMs);
                    //watch = Stopwatch.StartNew();
                    //////
                    error = ErrorCalculation(slowa, currentParticle);
                    //////
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //Console.WriteLine("ErrorCalculation execution time: " + elapsedMs);
                    
                    //particleError[i] = error;
                    particleError.Add(error);

              

                    if (particleBest[i] != error)
                    {
                        particlesStopNumber[i] = 0;
                    }

                     if (particleBest[i] > error)
                    {
                        particleBest[i] = error;
                        particlesBestPos[i]= particlesPos[i];
                        
                    }

                     if (particleBest[i] == error)
                    {
                        particlesStopNumber[i]= particlesStopNumber[i] + 1;
                    }
                    


                    if ((double)particleError[i] <= errTolerance)
                    {
                        continuePSO = false;
                        Console.WriteLine("One of particle is very similar ! ");

                        //List<double> digitAutomattmp = makeVectorDiscrete(particlesPos[bestFinalAutomatIndextmp], particlesVel[bestFinalAutomatIndextmp], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                        Automat solutiontmp = new Automat();
                        solutiontmp = Automat.fromVector(zListyNaStringa(discretePosition), currentStateNumber, (int)idealAutomat.getAlphabetLength());
                        BestAutomatForStates.Add(solutiontmp);
                        BestErrorsForAutomats.Add((double)particleError[i]);

                        break;
                    }

                }
                //////
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Fitness function execution time: " + elapsedMs);

                if (continuePSO == true)
                {
                    //ustalanieBestow

                    double smallestError = 100.0;
                    for (int i = 0; i < particlesNumber; i++)
                    {

                        if ((double)particleError[i] < smallestError)
                        {
                            smallestError = (double)particleError[i];
                            particlesGlobBest = i;
                        }

                        List<double> distances = new List<double>();
                        List<int> indexes = new List<int>();
                        for (int j = 0; j < particlesNumber; j++)
                        {
                            //tutaj od razu szukamu ktore particle sa najblizsze
                            // i z nich wylaniamy local besta analogicznie jak globala
                            // ale dla neighbournumber czy jakos tak
                            if (true)
                            {
                                double dist = findDistance(particlesPos[i], particlesPos[j]);
                                if (distances.Count < numberOfNeighbors)
                                {
                                    distances.Add(dist);
                                    indexes.Add(j);
                                }
                                else
                                {
                                    int maxInd = 0;
                                    double tempmaxVal = 0.0;
                                    for (int p = 0; p < distances.Count; p++)
                                    {
                                        if (distances[p] > tempmaxVal)
                                        {
                                            tempmaxVal = distances[p];
                                            maxInd = p;
                                        }
                                    }
                                    if (tempmaxVal > dist)
                                    {
                                        distances[maxInd] = dist;
                                        indexes[maxInd] = j;
                                    }
                                }
                            }
                        }

                        // teraz mam liste najblizszych particlow i szykamy tego z najmneijszym errorem.
                        double minLocErr = 100.0;
                        int minLocIndex = 0;
                        for (int j = 0; j < distances.Count; j++)
                        {
                            if (distances[j] < minLocErr)
                            {
                                minLocErr = distances[j];
                                minLocIndex = indexes[j];
                            }
                        }
                        particlesLocBest[i] = minLocIndex;

                    }


                    //teraz kazdy particle ma swojego globala i locala
                    // obliczamy nowa predkosc dla kazdego

                    for (int i = 0; i < particlesNumber; i++)
                    {
                        if (freez == true && i == particlesGlobBest)
                        {
                            //lol zamorozony best
                        }
                        else
                        {
                            particlesVel[i] = calculateVelocity(maxSpeed, particlesPos[particlesGlobBest], particlesPos[particlesLocBest[i]],
                            particlesVel[i], particlesPos[i], c1, c2);
                            //jest predkosc to mozemy aplikowac ja do pozycji

                            particlesPos[i] = calculatePosition(particlesVel[i], particlesPos[i]);

                            if (particlesStopNumber[i] >= maxStopError)
                            {
                                Console.WriteLine("Particle " + i + " has the same error " + particlesStopNumber[i] + " times. Restoring best");
                                particlesStopNumber[i] = 0;
                                particlesPos[i] = particlesBestPos[i];
                            }
                        }
                    }


                    counter++;

                    Console.WriteLine("Iteration : " + counter + " for : " + currentStateNumber + " states. (" + particlesGlobBest + ")GlobBestErr = " + (double)particleError[particlesGlobBest]);

                    //randomizacja particlow jezeli error sie zatrzymal
                    if (lastErrorValue == (double)particleError[particlesGlobBest])
                    {
                        errorRepeatCounter++;
                        if (errorRepeatCounter > iterationToRandomise)
                        {
                            Console.WriteLine("Randomizing position and velocity of " + particleRandomNumber + " particles");

                            double[][] errors =  new double[particlesNumber][];
                            for(int i=0;i<particlesNumber;i++){
                                double[] error = new double[2];
                                error[0]=particleError[i];
                                error[1]=i;
                                errors[i]=error;
                            }
                            
                            for(int i=0; i<particlesNumber;i++){
                                for(int j=0;j<particlesNumber-1;j++){
                                    if(errors[j][0]<errors[j+1][0]){
                                        double[] tmp = errors[j];
                                        errors[j] = errors[j+1];
                                        errors[j+1] = tmp;
                                    }
                                }
                            }





                           //Random random = new Random();
                            for (int i = 0; i < particleRandomNumber; i++)
                            {
                                int rand = (int)errors[i][1];
                                //losujemy pozycje i predkosc particles

                                List<double> singleVector = new List<double>();
                                List<double> singleSpeedVector = new List<double>();

                                for (int j = 0; j < dimensions; j++)
                                {
                                    double randomVal = GetRandomNumber();
                                    double randomVal2 = GetRandomParticleSpeed(speedLowerBound, speedUpperBound);
                                    singleVector.Add(randomVal);
                                    singleSpeedVector.Add(randomVal2);
                                }
                                particlesPos[rand] = singleSpeedVector;
                                particlesVel[rand] = singleSpeedVector;
                            }
                            errorRepeatCounter = 0;
                        }
                    }
                    else
                    {
                        lastErrorValue = (double)particleError[particlesGlobBest];
                        errorRepeatCounter = 0;
                    }

                }
                Console.WriteLine("------------------------------------------------------------------------");
                if (counter >= maxIteration)
                {
                    addStates = true;

                    double minimalFinalErrtmp = 100.0;
                    int bestFinalAutomatIndextmp = 0;
                    for (int i = 0; i < particlesNumber; i++)
                    {
                        if ((double)particleError[i] < minimalFinalErrtmp)
                        {
                            minimalFinalErrtmp = (double)particleError[i];
                            bestFinalAutomatIndextmp = i;
                        }
                    }

                    List<double> digitAutomattmp = makeVectorDiscrete(particlesPos[bestFinalAutomatIndextmp], particlesVel[bestFinalAutomatIndextmp], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                    Automat solutiontmp = new Automat();
                    solutiontmp = Automat.fromVector(zListyNaStringa(digitAutomattmp), currentStateNumber, (int)idealAutomat.getAlphabetLength());


                    BestAutomatForStates.Add(solutiontmp);
                    BestErrorsForAutomats.Add(minimalFinalErrtmp);
                }
            }

            //pso skonczone wybieramy najmniejsyz err


            //mamy najlepsze automaty i szukamy solutiona

            Console.WriteLine("PSO FINISHED");

            findRelationPairs(setOfWords);

            for (int i = 0; i < BestAutomatForStates.Count; i++)
            {
                double newError = 100.0;
                Automat Tempsolution = (Automat)BestAutomatForStates[i];
                newError = ErrorCalculation(setOfWords, Tempsolution);
                BestErrorsForAutomats2Set.Add(newError);
            }


            double minimalFinalErr = 100.0;
            int bestFinalAutomatIndex = 0;
            for (int i = 0; i < BestAutomatForStates.Count; i++)
            {
                if ((double)BestErrorsForAutomats2Set[i] < minimalFinalErr)
                {
                    minimalFinalErr = (double)BestErrorsForAutomats2Set[i];
                    bestFinalAutomatIndex = i;
                }
            }

          

            Automat solution = (Automat)BestAutomatForStates[bestFinalAutomatIndex];

            Console.WriteLine("SOLUTION error : " + minimalFinalErr);
            Console.WriteLine("" + (zListyNaStringa(solution.toVector())));


            List<int>[][] wynikDlaLukasza = doWydruku(solution);

            List<int>[][] idealnyDlaLukasza = doWydruku(idealAutomat);


        }

        String zListyNaStringa(List<double> lista)
        {
            String wynik = "";
            for (int i = 0; i < lista.Count; i++ )
            {
                wynik = wynik + lista[i];
            }
                return wynik;
        }

        /// <summary>
        /// aplikowane predkosci do pozycji
        /// </summary>
        /// 
        List<double> calculatePosition(List<double> currentVelocity, List<double> currentPosition)
        {
            List<double> newPosition = new List<double>();

            for (int i = 0; i < currentVelocity.Count; i++)
            {
                
                double pos = currentPosition[i] + currentVelocity[i];
                if (pos > 1.0)
                {
                    newPosition.Add(1.0);
                }
                else if (pos < 0.0)
                {
                    newPosition.Add(0.0);
                }
                else 
                {
                    newPosition.Add(pos);
                }
            }

            return newPosition;
        }

        /// <summary>
        /// aplikowane predkosci wedlug wzoru z dokumentacji
        /// nV = v + c1 *random * (local - position) + c2 * random*(global - position)
        /// random z (0;1)
        /// </summary>
        /// 
        List<double> calculateVelocity(double maxSpeed, List<double> globalBest, List<double> localBest, List<double> currentVelocity, List<double> currentPosition, double c1, double c2)
        {
            List<double> newVelocity = new List<double>();

            //c1 *random * (local - position)
            double part1 = c1 * GetRandomNumber();
            double part2 = c2 * GetRandomNumber();
            
            for (int i = 0; i < localBest.Count; i++ )
            {
                /*double tmp1 = ((double.Parse(localBest[i]+"") - double.Parse(""+currentPosition[i])) * part1);
                double tmp2 = ((double.Parse(""+globalBest[i]) - double.Parse(""+currentPosition[i])) * part2);*/

                double tmp1 = ((localBest[i] - currentPosition[i]) * part1);
                double tmp2 = ((globalBest[i] - currentPosition[i]) * part2);

                double speed = currentVelocity[i] + tmp1 + tmp2;
                
                if(speed> maxSpeed)
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

        double findDistance(List<double> vector1, List<double> vector2)
        {
            double distance = 0.0;

            for (int i = 0; i < vector1.Count; i++ )
            {
               /* double val1 = double.Parse(vector1[i].ToString());
                double val2 = double.Parse(vector2[i].ToString());
                */

                distance = distance + Math.Abs(vector1[i] - vector2[i]);
            }

            return distance;
        }

        /// <summary>
        /// dyskretyzacja vektora
        /// </summary>
        /// 
        List<double> makeVectorDiscrete(List<double> vector, List<double> speed, double roundparam, int _statesNumber, int _alphabetLength)
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
        /// random value from 0 to 1
        /// </summary>
        public double GetRandomNumber()
        {
            return random.NextDouble() * (1.0 - 0.0) + 0.0;
        }

        /// <summary>
        /// random value from -1 to 1
        /// </summary>
        public double GetRandomParticleSpeed(double lowerBound, double upperBound)
        {
            double speed;
            do
            {
                speed = (random.NextDouble() * 2.0) - 1.0;
            } while (speed < lowerBound || speed > upperBound);
            return speed;
        }

        bool valideData()
        {
            if (idealAutomat.getAlphabetLength() <= 0 || setOfWords.Count <2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void PSO_Click(object sender, RoutedEventArgs e)
        {
            if (valideData() == true)
            {
                PSO();
            }
            else
            {
                MessageBox.Show("Not all data loaded !");
                return;
            }
        }

        List<int>[][] doWydruku(Automat wynik)
        {
            int wymiar = wynik.getStatesNumber();

            List<int>[][] macierz = new List<int>[wymiar][];
            for (int i = 0; i < wymiar; i++)
            {
                List<int>[] tmp = new List<int>[wymiar];
                for (int j = 0; j < wymiar; j++)
                {
                    tmp[j] = new List<int>();
                }
                macierz[i] = tmp;
            }



            for (int i = 0; i < wynik.getAlphabetLength(); i++)
            {
                int[][] transition = wynik.getTransitionTableList()[i];
                for (int y = 0; y < wynik.getStatesNumber(); y++)
                {
                    for (int x = 0; x < wynik.getStatesNumber(); x++)
                    {
                        if (transition[x][y] == 1)
                        {
                            macierz[x][y].Add(i);
                        }
                    }
                }
            }

            return macierz;


        }

    }
}
