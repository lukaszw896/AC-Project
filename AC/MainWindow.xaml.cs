using MahApps.Metro.Controls;
using System;
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
    public partial class MainWindow : MetroWindow

    {

        Automat idealAutomaton;
        int valueC;


        List<List<int>> setOfWords;
        List<List<int>> learningSetOfWords;
        List<List<int>> testingSetOfWords;

        int numberOfWordsSmallerEqualC;

        int[][] pairsOfRelation;
        static Random random = new Random();
        double speedLowerBound = -0.2;
        double speedUpperBound = 0.2;


        /****************************************************************
         *                  Variables used only in PSO                  *
         ****************************************************************/ 
             List<List<int>> slowa;


            double maxSpeed;

            int iterationToRandomise;

            List<Automat> BestAutomatForStates;
            List<double> BestErrorsForAutomats;

            List<double> BestErrorsForAutomats2Set;
            double roundAt;

            int particlesNumber;
            int maxIteration;
            double c2;
            double c1;
            int numberOfNeighbors;

            int maxStopError;
            int particleRandomNumber;


            List<List<double>> particlesPos;
            List<List<double>> particlesVel;
            List<List<double>> particlesBestPos;
            List<double> particleBest;
            List<int> particlesStopNumber;

            
            List<int> particlesLocBest;
            int particlesGlobBest;
            List<double> particleError;
            int dimensions;

            int minimumNumberOfStates;
            int currentStateNumber;

            double errTolerance;

            int maxStateNumber;

            bool continuePSO;
            bool addStates;
            int counter;

            double lastErrorValue;
            double errorRepeatCounter;

            bool freez;
            double minimalFinalErr;

            bool shouldStart;     

        public MainWindow()
        {
            InitializeComponent();


            setOfWords = new List<List<int>>();
          
            idealAutomaton = new Automat();
              
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
            idealAutomaton = automata;
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
            valueC = setOfWords[setOfWords.Count - 1][0];
            setOfWords.RemoveAt(setOfWords.Count - 1);

            int numOfWordsLengthC = 0;

            for (int i = 0; i < valueC+1; i++)
            {
                int tmp = 1;
                for (int j = 0; j < i; j++) {
                    tmp *= 5;
                }
                numOfWordsLengthC += tmp;
            }

            numberOfWordsSmallerEqualC = numOfWordsLengthC;

            learningSetOfWords = new List<List<int>>();
            testingSetOfWords = new List<List<int>>();
            

            String separator = "99999999999999999999";

            int flag = 0;
            if (setOfWords.Count >= 2)
            {
                for (int i = 0; i < setOfWords.Count - 2; i++)
                {
                    String tmp = "";
                    for (int w = 0; w < setOfWords[i].Count; w++ )
                    {
                        tmp = tmp + (setOfWords[i])[w];
                    }
                    numOfWordsLengthC--;

                    if (tmp.Equals(separator))
                    {
                        flag = 1;
                    }

                        if (flag == 0)
                        {
                            learningSetOfWords.Add(setOfWords[i]);
                        }
                        else if (flag == 1)
                        {
                            testingSetOfWords.Add(setOfWords[i + 1]);
                        }
                }
            }
        }       

        /// <summary>
        /// PSO ^^
        /// </summary>
        /// 
        async Task PSO()
        {
            Console.WriteLine("Learning set size : " + learningSetOfWords.Count + " out of total words : " + setOfWords.Count);

            InitPsoVariables();

            if (minimumNumberOfStates >= maxStateNumber)
            {
                MessageBox.Show("Maximal Number of States is too small !");
                continuePSO = false;
                shouldStart = false;
            }


            Automat solution = new Automat();
            await Task.Run(() =>
            {
                while (continuePSO == true && shouldStart == true)
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

                    dimensions = (int)Math.Pow(currentStateNumber, 2.0) * (int)idealAutomaton.AlphabetLength;
                    particlesGlobBest = 0;

                    ResetPsoData(particlesPos, particlesVel, particlesLocBest, particleError, particleBest, particlesStopNumber, particlesBestPos, particlesNumber, dimensions);

                    addStates = false;
                }

                var watch = Stopwatch.StartNew();

                PsoFitnessComputation();
                
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Fitness function execution time: " + elapsedMs);

                if (continuePSO == true)
                {
                    CalculateLocalAndGlobalBest();

                    CalculateNewParticleVelocity();

                    counter++;

                    Console.WriteLine("Iteration : " + counter + " for : " + currentStateNumber + " states. (" + particlesGlobBest + ")GlobBestErr = " + (double)particleError[particlesGlobBest]);

                    //randomizacja particlow jezeli error sie zatrzymal
                    if (lastErrorValue == (double)particleError[particlesGlobBest])
                    {
                        errorRepeatCounter++;
                        if (errorRepeatCounter > iterationToRandomise){
                            RandomizeNotMovingParticles(particleRandomNumber,particlesNumber,particlesPos,particlesVel,dimensions,particleError);
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

                    SaveBestAutomatonForGivenStateNumber();
                    
                }
            }

                if (shouldStart == true)
                {
                 solution =   FindTheBestSolution();
                }

            });

            if (shouldStart == true)
            {
                DisplayResultAsGraph(solution);
            }
            
        }

        /// <summary>
        /// In this function we are doing everythink what is needed to compute error for each of particles. Computations are run in parallel. If any of computed particles had error
        /// equal to 0 than we stop our computations.
        /// 
        /// </summary>
        private void PsoFitnessComputation()
        {
            particleError.Clear();

            Task<List<double>>[] taskArray = new Task<List<double>>[8];

            int numOfParticlesPerTask = (int)Math.Ceiling((double)particlesNumber / 8.0);

            int bottom = 0;
            int top = numOfParticlesPerTask;

            for (int i = 0; i < 8; i++)
            {
                int bottomCopy = bottom;
                int topCopy = top;
                taskArray[i] = Task<List<double>>.Factory.StartNew(() => CalculateError(bottomCopy, topCopy,(int)idealAutomaton.AlphabetLength));
                bottom = top;
                top += numOfParticlesPerTask;
                if (top > particlesNumber)
                {
                    top = particlesNumber;
                }
            }

            Task.WaitAll(taskArray);
            for (int i = 0; i < taskArray.Length; i++)
            {
                for (int j = 0; j < taskArray[i].Result.Count; j++)
                {
                    particleError.Add(taskArray[i].Result[j]);
                }
            }

            for (int i = 0; i < particlesNumber; i++)
            {
                if ((double)particleError[i] <= errTolerance)
                {
                    continuePSO = false;
                    Console.WriteLine("One of particle is very similar ! ");

                    Automat solutiontmp = new Automat();
                    List<double> discretePosition = new List<double>();
                    discretePosition = PsoHelper.AutomatonDiscretisation(particlesPos[i], particlesVel[i], roundAt, currentStateNumber, (int)idealAutomaton.AlphabetLength);
                    solutiontmp = Automat.fromVector(zListyNaStringa(discretePosition), currentStateNumber, (int)idealAutomaton.AlphabetLength);
                    BestAutomatForStates.Add(solutiontmp);
                    BestErrorsForAutomats.Add((double)particleError[i]);

                    break;
                }
            }
        }


        /// <summary>
        /// This function takes the best automaton from all particles with given number of states and saves it.
        /// </summary>
        private void SaveBestAutomatonForGivenStateNumber()
        {
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

            List<double> digitAutomattmp = PsoHelper.AutomatonDiscretisation(particlesPos[bestFinalAutomatIndextmp], particlesVel[bestFinalAutomatIndextmp], roundAt, currentStateNumber, (int)idealAutomaton.AlphabetLength);
            Automat solutiontmp = new Automat();
            solutiontmp = Automat.fromVector(zListyNaStringa(digitAutomattmp), currentStateNumber, (int)idealAutomaton.AlphabetLength);


            BestAutomatForStates.Add(solutiontmp);
            BestErrorsForAutomats.Add(minimalFinalErrtmp);
        }

        /// <summary>
        /// Function passing automaton to DipslayGraph constructor (in desired format). Open new window where graphs will be written.
        /// </summary>
        /// <param name="solution"></param>
        private void DisplayResultAsGraph(Automat solution)
        {
            List<int>[][] wynikDlaLukasza = doWydruku(solution);
            List<int>[][] idealnyDlaLukasza = doWydruku(idealAutomaton);

            DisplayGraph displayGraph = new DisplayGraph(idealnyDlaLukasza, wynikDlaLukasza, minimalFinalErr);
            displayGraph.Show();
        }

        private void CalculateLocalAndGlobalBest()
        {
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
                        double dist = PsoHelper.FindDistance(particlesPos[i], particlesPos[j]);
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
                    if ((double)particleError[indexes[j]] < minLocErr)
                    {
                        minLocErr = (double)particleError[indexes[j]];
                        minLocIndex = indexes[j];
                    }
                }
                particlesLocBest[i] = minLocIndex;

            }
        }

        /// <summary>
        /// Function calculating new particle velocity based on global and local best
        /// </summary>
        private void CalculateNewParticleVelocity()
        {
            for (int i = 0; i < particlesNumber; i++)
            {
                if (freez == true && i == particlesGlobBest)
                {
                    //lol zamorozony best
                }
                else
                {
                    particlesVel[i] = PsoHelper.CalculateVelocity(maxSpeed, particlesPos[particlesGlobBest], particlesPos[particlesLocBest[i]],
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
        }


        /// <summary>
        /// Funtcion finding the best solution from all found automatons
        /// </summary>
        private Automat FindTheBestSolution()
        {
            Console.WriteLine("PSO FINISHED");

            pairsOfRelation = PsoHelper.FindRelationPairs(testingSetOfWords, idealAutomaton);


            List<Automat> solucjekazde = new List<Automat>();
            List<double> wynikiKazde = new List<double>();
            List<double> learningwynikiKazde = new List<double>();

            List<double> listDoC = new List<double>();
            List<double> listOdC = new List<double>();




            for (int i = 0; i < BestAutomatForStates.Count; i++)
            {
                double newError = 100.0;
                // double newErrorDoC = 100.0;
                // double newErrorOdC = 100.0;
                Automat Tempsolution = (Automat)BestAutomatForStates[i];

                newError = PsoHelper.CalculateParticleError(testingSetOfWords, Tempsolution, pairsOfRelation);

                solucjekazde.Add(Tempsolution);
                wynikiKazde.Add(newError);
                learningwynikiKazde.Add(BestErrorsForAutomats[i]);
                BestErrorsForAutomats2Set.Add(newError);

            }
            List<List<int>> setDoC = new List<List<int>>();
            for (int p = 0; p < numberOfWordsSmallerEqualC; p++)
            {
                setDoC.Add(testingSetOfWords[p]);
            }
            pairsOfRelation = PsoHelper.FindRelationPairs(setDoC, idealAutomaton);
            for (int i = 0; i < BestAutomatForStates.Count; i++)
            {
                Automat Tempsolution = (Automat)BestAutomatForStates[i];
                double newErrorDoC = 100.0;
                newErrorDoC = PsoHelper.CalculateParticleError(setDoC, Tempsolution, pairsOfRelation);
                listDoC.Add(newErrorDoC);
            }
            setDoC.Clear();
            for (int p = numberOfWordsSmallerEqualC; p < testingSetOfWords.Count - 1; p++)
            {
                setDoC.Add(testingSetOfWords[p]);
            }
            pairsOfRelation = PsoHelper.FindRelationPairs(setDoC, idealAutomaton);
            for (int i = 0; i < BestAutomatForStates.Count; i++)
            {
                Automat Tempsolution = (Automat)BestAutomatForStates[i];
                double newErrorDoC = 100.0;
                newErrorDoC = PsoHelper.CalculateParticleError(setDoC, Tempsolution, pairsOfRelation);
                listOdC.Add(newErrorDoC);
            }

            minimalFinalErr = 100.0;
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
            return solution;
        }


        /// <summary>
        /// function initializing variables
        /// </summary>
        private void InitPsoVariables()
        {
            slowa = learningSetOfWords;

            speedLowerBound = double.Parse(speedLowerBoundTxt.Text, CultureInfo.InvariantCulture);
            speedUpperBound = double.Parse(speedUpperBoundTxt.Text, CultureInfo.InvariantCulture);

            maxSpeed = double.Parse(maxSpeedTxt.Text, CultureInfo.InvariantCulture);

            iterationToRandomise = int.Parse(errorSameRepetition.Text);

            BestAutomatForStates = new List<Automat>();
            BestErrorsForAutomats = new List<double>();

            BestErrorsForAutomats2Set = new List<double>();

            pairsOfRelation = PsoHelper.FindRelationPairs(slowa, idealAutomaton);
            roundAt = double.Parse(RountAtTxt.Text, CultureInfo.InvariantCulture);

            particlesNumber = int.Parse(ParticleAmountTxt.Text);
            maxIteration = int.Parse(IterationTxt.Text);
            c2 = double.Parse(c1Txt.Text, CultureInfo.InvariantCulture);
            c1 = double.Parse(c2Txt.Text, CultureInfo.InvariantCulture);
            numberOfNeighbors = int.Parse(neighboursTxt.Text);

            maxStopError = int.Parse(maxErrIterationTxt.Text);
            particleRandomNumber = int.Parse(particlerandomnumber.Text);


            particlesPos = new List<List<double>>();
            particlesVel = new List<List<double>>();
            particlesBestPos = new List<List<double>>();
            particleBest = new List<double>();
            particlesStopNumber = new List<int>();


            particlesLocBest = new List<int>();
            particlesGlobBest = 0;
            particleError = new List<double>();
            dimensions = 0;

            minimumNumberOfStates = int.Parse(MinstatesTXT.Text);
            if (finMin.IsChecked == true)
            {
                minimumNumberOfStates = PsoHelper.MinimumNumberOfStates(idealAutomaton, learningSetOfWords);
            }

            currentStateNumber = minimumNumberOfStates - 1;

            errTolerance = double.Parse(ToleranceTxt.Text, CultureInfo.InvariantCulture);

            maxStateNumber = int.Parse(MaxstatesTXT.Text);
            maxStateNumber++;

            continuePSO = true;
            addStates = true;
            counter = 0;

            lastErrorValue = 0;
            errorRepeatCounter = 0;

            freez = freezGlobal.IsChecked.Value;
            minimalFinalErr = 100.0;

            shouldStart = true;
        }


        /// <summary>
        /// Function reseting data of PSO after increasing number of states of generated particles
        /// </summary>
        /// <param name="particlesPos"></param>
        /// <param name="particlesVel"></param>
        /// <param name="particlesLocBest"></param>
        /// <param name="particleError"></param>
        /// <param name="particleBest"></param>
        /// <param name="particlesStopNumber"></param>
        /// <param name="particlesBestPos"></param>
        /// <param name="particlesNumber"></param>
        /// <param name="dimensions"></param>
        private void ResetPsoData(List<List<double>> particlesPos, List<List<double>> particlesVel, List<int> particlesLocBest, List<double> particleError,
                                  List<double> particleBest, List<int> particlesStopNumber, List<List<double>> particlesBestPos, int particlesNumber, int dimensions)
        {
            particlesPos.Clear();
            particlesVel.Clear();
            particlesLocBest.Clear();
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
                    double randomVal = PsoHelper.GetRandomNumber();
                    double randomVal2 = GetRandomParticleSpeed(speedLowerBound, speedUpperBound);
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
        }



        /// <summary>
        /// Function randomizing position of particles with the biggest error if global best error is not changing for specified number of times
        /// </summary>
        /// <param name="particleRandomNumber"></param>
        /// <param name="particlesNumber"></param>
        /// <param name="particlesPos"></param>
        /// <param name="particlesVel"></param>
        /// <param name="numOfDim"></param>
        /// <param name="particleError"></param>
        private void RandomizeNotMovingParticles( int particleRandomNumber, int particlesNumber,
                                                 List<List<double>> particlesPos, List<List<double>> particlesVel, int numOfDim,List<double> particleError)
        {
                Console.WriteLine("Randomizing position and velocity of " + particleRandomNumber + " particles");

                double[][] errors = new double[particlesNumber][];
                for (int i = 0; i < particlesNumber; i++)
                {
                    double[] error = new double[2];
                    error[0] = particleError[i];
                    error[1] = i;
                    errors[i] = error;
                }

                for (int i = 0; i < particlesNumber; i++)
                {
                    for (int j = 0; j < particlesNumber - 1; j++)
                    {
                        if (errors[j][0] < errors[j + 1][0])
                        {
                            double[] tmp = errors[j];
                            errors[j] = errors[j + 1];
                            errors[j + 1] = tmp;
                        }
                    }
                }

                //Random random = new Random();
                for (int i = 0; i < particleRandomNumber; i++)
                {
                    RandomizePositionAndVelocity(i, particlesPos, particlesVel, numOfDim, errors);
                }
                
        }


        /// <summary>
        /// In this function we calculate error of one particle
        /// </summary>
        /// <param name="bottom"></param>
        /// <param name="up"></param>
        /// <param name="alphabetLength"></param>

        /// <returns></returns>
       List<double> CalculateError(int bottom, int up, int alphabetLength)
        {
            List<double> particleError = new List<double>();
            for (int i = bottom; i < up; i++)
            {

                List<double> discretePosition = new List<double>();

                discretePosition = PsoHelper.AutomatonDiscretisation(particlesPos[i], particlesVel[i], roundAt, currentStateNumber, alphabetLength);
                double error = 0.0;

                // watch = Stopwatch.StartNew();
                //////
                Automat currentParticle = new Automat();

                currentParticle = Automat.fromVector(zListyNaStringa(discretePosition), currentStateNumber, alphabetLength);

                error = PsoHelper.CalculateParticleError(slowa, currentParticle, pairsOfRelation);
                particleError.Add(error);

                if (particleBest[i] != error)
                {
                    particlesStopNumber[i] = 0;
                }

                if (particleBest[i] > error)
                {
                    particleBest[i] = error;
                    particlesBestPos[i] = particlesPos[i];

                }

                if (particleBest[i] == error)
                {
                    particlesStopNumber[i] = particlesStopNumber[i] + 1;
                }                
            }

            return particleError;
        }

        private void RandomizePositionAndVelocity(int i, List<List<double>> particlesPos, List<List<double>> particlesVel, int numOfDim, double[][] errors)
        {
            int rand = (int)errors[i][1];
            //losujemy pozycje i predkosc particles

            List<double> singleVector = new List<double>();
            List<double> singleSpeedVector = new List<double>();

            for (int j = 0; j < numOfDim; j++)
            {
                double randomVal = PsoHelper.GetRandomNumber();
                double randomVal2 = GetRandomParticleSpeed(speedLowerBound, speedUpperBound);
                singleVector.Add(randomVal);
                singleSpeedVector.Add(randomVal2);
            }
            particlesPos[rand] = singleSpeedVector;
            particlesVel[rand] = singleSpeedVector;
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
            if (idealAutomaton.AlphabetLength <= 0 || setOfWords.Count <2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void PSO_Click(object sender, RoutedEventArgs e)
        {
            if (valideData() == true)
            {
                progressRing.Visibility = Visibility.Visible;
                progressRingBackground.Visibility = Visibility.Visible;
              await  PSO();
              progressRing.Visibility = Visibility.Collapsed;
              progressRingBackground.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Not all data loaded !");
                return;
            }
        }


        private void CreateAutomat_Click(object sender, RoutedEventArgs e)
        {
            AutomatGenerator generator2 = new AutomatGenerator();

            Nullable<bool> result = generator2.ShowDialog();

            if (result == true)
            {
                int nrstates = generator2.getStates();
                int nralphabet = generator2.getAlp();
                List<int[]> listtransition = generator2.gettrans();

                String a = nrstates + "," + nralphabet;

                for (int j = 0; j < nrstates; j++)
                {
                    int[] row = listtransition[j];
                    for (int i = 0; i < nralphabet; i++)
                    {
                        a = a + "," + (row[i] + 1);
                    }
                }

                idealAutomaton = new Automat();
                idealAutomaton.fromString(a);
                Console.WriteLine("Automat Generated");
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

        List<int>[][] doWydruku(Automat wynik)
        {
            int wymiar = wynik.StatesNumber;

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



            for (int i = 0; i < wynik.AlphabetLength; i++)
            {
                int[] transition = wynik.TransitiontableList[i];
                for (int y = 0; y < wynik.StatesNumber; y++)
                {
                    for (int x = 0; x < wynik.StatesNumber; x++)
                    {
                        if (transition[x] == y)
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
