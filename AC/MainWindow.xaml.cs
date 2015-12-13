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

        Automat idealAutomat;

        List<Automat> idealneSolucje;
        List<double> idealneWyniki;

        List<List<Automat>> wszystkieSolucje;

        int valueC;

        List<List<double>> wszystkiedoC;
        List<List<double>> wszystkieodC;
        List<List<double>> wszystkieLearningoweWyniki;
        List<List<double>> wszystkieWyniki;


        List<List<int>> setOfWords;
        List<List<int>> learningSetOfWords;
        List<List<int>> testingSetOfWords;

        int numberOfWordsSmallerEqualC;

        int[][] pairsOfRelation;
        static Random random = new Random();
        double speedLowerBound = -0.2;
        double speedUpperBound = 0.2;

        public MainWindow()
        {
            InitializeComponent();


            setOfWords = new List<List<int>>();
          
            idealAutomat = new Automat();

            idealneSolucje = new List<Automat>();
            idealneWyniki = new List<double>();

            wszystkieSolucje = new List<List<Automat>>();
            wszystkieWyniki = new List<List<double>>();
            wszystkieLearningoweWyniki = new List<List<double>>();
            wszystkiedoC = new List<List<double>>();
            wszystkieodC = new List<List<double>>();
              
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

                idealAutomat = new Automat();
                idealAutomat.fromString(a);
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
        /// uzypelniamy tutaj tabele parami slow ktore sa w relacji
        /// </summary>
        /// 
        void FindRelationPairs(List<List<int>> words)
        {
            //pairsOfRelation
            pairsOfRelation = new int[words.Count][];

            for (int j = 0; j < words.Count; j++)
            {
                pairsOfRelation[j] = new int[words.Count];
            }

            int[] finishingStates = new int[words.Count];

            Parallel.For(0, words.Count, i =>
            {
                int tmp = PsoHelper.WordComputationFinishingState(idealAutomat, words[i]);
                finishingStates[i] = tmp;
            }
           );

            for (int i = 0; i < words.Count; i++)
            {
                for (int j = 0; j < words.Count; j++)
                {
                    if(i != j)
                    {
                        if (finishingStates[i] == finishingStates[j])
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
            int a = 3;
            a++;
        }       


        /// <summary>
        /// PSO ^^
        /// </summary>
        /// 
        async Task PSO()
        {
            Console.WriteLine("Learning set size : " + learningSetOfWords.Count + " out of total words : " + setOfWords.Count);
            List<List<int>> slowa = new List<List<int>>();
            slowa = learningSetOfWords;

            speedLowerBound = double.Parse(speedLowerBoundTxt.Text, CultureInfo.InvariantCulture);
            speedUpperBound = double.Parse(speedUpperBoundTxt.Text, CultureInfo.InvariantCulture);

            double maxSpeed = double.Parse(maxSpeedTxt.Text, CultureInfo.InvariantCulture);

            int iterationToRandomise = int.Parse(errorSameRepetition.Text);

            List<Automat> BestAutomatForStates = new List<Automat>();
            List<double> BestErrorsForAutomats = new List<double>();

            List<double> BestErrorsForAutomats2Set = new List<double>();

            FindRelationPairs(slowa);
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

            int minimumNumberOfStates = int.Parse(MinstatesTXT.Text);
            if (finMin.IsChecked == true)
            {
                minimumNumberOfStates = PsoHelper.MinimumNumberOfStates(idealAutomat, learningSetOfWords);
            }

            int currentStateNumber = minimumNumberOfStates - 1;

            double errTolerance = double.Parse(ToleranceTxt.Text, CultureInfo.InvariantCulture);

            int maxStateNumber = int.Parse(MaxstatesTXT.Text);
            maxStateNumber++;

            bool continuePSO = true;
            bool addStates = true;
            int counter = 0;

            double lastErrorValue = 0;
            double errorRepeatCounter = 0;

            bool freez = freezGlobal.IsChecked.Value;
            double minimalFinalErr = 100.0;

            bool shouldStart = true;
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
                            double randomVal = PsoHelper.GetRandomNumber();
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

                Task<List<double>>[] taskArray = new Task<List<double>>[8];

                int numOfParticlesPerTask = (int)Math.Ceiling((double)particlesNumber / 8.0);

                int bottom = 0;
                int top = numOfParticlesPerTask;
                
                for (int i = 0; i < 8; i++)
                {
                    int bottomCopy = bottom;
                    int topCopy = top;
                    taskArray[i] = Task<List<double>>.Factory.StartNew(() => CalculateError(bottomCopy, topCopy, roundAt, currentStateNumber, slowa, (int)idealAutomat.getAlphabetLength(), particlesPos, particlesVel, particleBest, particlesStopNumber, particlesBestPos));
                    bottom = top;
                    top += numOfParticlesPerTask;
                    if (top > particlesNumber)
                    {
                        top = particlesNumber;
                    }
                }

                /*Task<List<double>>[] taskArray = {Task<List<double>>.Factory.StartNew(()=>CalculateError(0,6,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(6,12,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(12,18,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(18,24,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(24,30,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(30,36,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                    Task<List<double>>.Factory.StartNew(()=>CalculateError(36,43,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos)),
                                     Task<List<double>>.Factory.StartNew(()=>CalculateError(43,50,roundAt,currentStateNumber,slowa,(int)idealAutomat.getAlphabetLength(),particlesPos,particlesVel,particleBest, particlesStopNumber, particlesBestPos))
                                   };*/

                Task.WaitAll(taskArray);
                for (int i = 0; i < taskArray.Length; i++)
                {
                    for (int j = 0; j < taskArray[i].Result.Count; j++)
                    {
                        particleError.Add(taskArray[i].Result[j]);
                    }
                }

                for (int i = 0; i < particlesNumber;i++ )
                {
                    if ((double)particleError[i] <= errTolerance)
                    {
                        continuePSO = false;
                        Console.WriteLine("One of particle is very similar ! ");

                        //List<double> digitAutomattmp = makeVectorDiscrete(particlesPos[bestFinalAutomatIndextmp], particlesVel[bestFinalAutomatIndextmp], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                        Automat solutiontmp = new Automat();
                        List<double> discretePosition = new List<double>();
                        discretePosition = PsoHelper.AutomatonDiscretisation(particlesPos[i], particlesVel[i], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                        solutiontmp = Automat.fromVector(zListyNaStringa(discretePosition), currentStateNumber, (int)idealAutomat.getAlphabetLength());
                        BestAutomatForStates.Add(solutiontmp);
                        BestErrorsForAutomats.Add((double)particleError[i]);

                        break;
                    }
                }

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
                                RandomizePositionAndVelocity(i, particlesPos, particlesVel,dimensions,errors);
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

                    List<double> digitAutomattmp = PsoHelper.AutomatonDiscretisation(particlesPos[bestFinalAutomatIndextmp], particlesVel[bestFinalAutomatIndextmp], roundAt, currentStateNumber, (int)idealAutomat.getAlphabetLength());
                    Automat solutiontmp = new Automat();
                    solutiontmp = Automat.fromVector(zListyNaStringa(digitAutomattmp), currentStateNumber, (int)idealAutomat.getAlphabetLength());


                    BestAutomatForStates.Add(solutiontmp);
                    BestErrorsForAutomats.Add(minimalFinalErrtmp);
                }
            }

            //pso skonczone wybieramy najmniejsyz err


            //mamy najlepsze automaty i szukamy solutiona

            if (shouldStart == true)
            {
                Console.WriteLine("PSO FINISHED");

                FindRelationPairs(testingSetOfWords);


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
                FindRelationPairs(setDoC);
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
                FindRelationPairs(setDoC);
                for (int i = 0; i < BestAutomatForStates.Count; i++)
                {
                    Automat Tempsolution = (Automat)BestAutomatForStates[i];
                    double newErrorDoC = 100.0;
                    newErrorDoC = PsoHelper.CalculateParticleError(setDoC, Tempsolution, pairsOfRelation);
                    listOdC.Add(newErrorDoC);
                }

                wszystkieSolucje.Add(solucjekazde);
                wszystkieWyniki.Add(wynikiKazde);
                wszystkieLearningoweWyniki.Add(learningwynikiKazde);

                wszystkiedoC.Add(listDoC);
                wszystkieodC.Add(listOdC);

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



                solution = (Automat)BestAutomatForStates[bestFinalAutomatIndex];

                Console.WriteLine("SOLUTION error : " + minimalFinalErr);
                Console.WriteLine("" + (zListyNaStringa(solution.toVector())));

            }
            });

            if (shouldStart == true)
            {
                List<int>[][] wynikDlaLukasza = doWydruku(solution);

                List<int>[][] idealnyDlaLukasza = doWydruku(idealAutomat);

                idealneSolucje.Add(solution);
                idealneWyniki.Add(minimalFinalErr);

                //DisplayGraph displayGraph = new DisplayGraph(idealnyDlaLukasza, wynikDlaLukasza, minimalFinalErr);
                //displayGraph.s
                //displayGraph.Show();
            }
            
        }


       List<double> CalculateError(int bottom, int up, double roundAt, int currentStateNumber, List<List<int>> slowa, int alphabetLength, List<List<double>> particlesPos, List<List<double>> particlesVel, 
            List<double> particleBest, List<int> particlesStopNumber, List<List<double>> particlesBestPos)
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
            if (idealAutomat.getAlphabetLength() <= 0 || setOfWords.Count <2)
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
                int[] transition = wynik.getTransitionTableList()[i];
                for (int y = 0; y < wynik.getStatesNumber(); y++)
                {
                    for (int x = 0; x < wynik.getStatesNumber(); x++)
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

        public String prepareStringToWynik(Automat solucja)
        {
         
            String tmpString = "";
            tmpString = tmpString + solucja.getStatesNumber() + "," + solucja.getAlphabetLength();
            List<int[]> transitionList = solucja.getTransitionTableList();


            for (int x = 0; x < solucja.getStatesNumber(); x++)
            {
                for (int y = 0; y < solucja.getAlphabetLength(); y++)
                {
                    tmpString = tmpString + "," + (transitionList[y][x] + 1);
                }
            }
            return tmpString;
        }

        public void zapiszWynik(String path, int mode, String pathZdjecie)
        {
            List<Automat> jedneRozwiaznia = new List<Automat>();
            List<double> jedneWyniki = new List<double>();
            List<double> jedneLearningoweWyniki = new List<double>();

            List<double> jedneDoC = new List<double>();
            List<double> jedneOdC = new List<double>();

            if (mode == 0)
            {
                jedneRozwiaznia = wszystkieSolucje[wszystkieSolucje.Count - 1];
                jedneWyniki = wszystkieWyniki[wszystkieWyniki.Count - 1];
                jedneLearningoweWyniki = wszystkieLearningoweWyniki[wszystkieLearningoweWyniki.Count - 1];
                jedneDoC = wszystkiedoC[wszystkiedoC.Count - 1];
                jedneOdC = wszystkieodC[wszystkieodC.Count - 1];
            }
            else if (mode == 1)
            {
                for(int l = 0 ; l < 5 ; l ++)
                {
                    Automat tmp = wszystkieSolucje[l][0];
                    jedneRozwiaznia.Add(tmp);
                    jedneWyniki.Add(wszystkieWyniki[l][0]);
                    jedneLearningoweWyniki.Add(wszystkieLearningoweWyniki[l][0]);
                }
            }


            using (StreamWriter sw = new StreamWriter(path))
            {
                List<double> wynikiKazde = new List<double>();
                for (int p = 0; p < jedneRozwiaznia.Count; p++)
                {
                    Automat solucja = jedneRozwiaznia[p];
                    String tmpString = prepareStringToWynik(solucja);
                   
                    String wynik = "";
                    wynik = "To C Error = " + jedneDoC[p] + " From C = " + jedneOdC[p] + " Learning Error = " + jedneLearningoweWyniki[p] + " || Final Error = " + jedneWyniki[p];
                    //wynik = "Learning Error = " + jedneLearningoweWyniki[p] + " || Final Error = " + jedneWyniki[p];
                    sw.WriteLine("----------------------------------------------------");
                    sw.WriteLine(tmpString);
                    sw.WriteLine(wynik);
                    wynikiKazde.Add(jedneWyniki[p]);
                }

                if (mode == 0)
                {
                    //na dole najlepszy wynik
                    sw.WriteLine("----------------------------------------------------");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("--------------------BEST SOLUTION-------------------");

                    Automat bestSolucja = idealneSolucje[idealneSolucje.Count - 1];
                    String tmpString2 = prepareStringToWynik(bestSolucja);
                   
                    String wynik2 = "error = " + idealneWyniki[idealneWyniki.Count - 1];
                    sw.WriteLine(tmpString2);
                    sw.WriteLine(wynik2);

                    DisplayGraph displayGraph = new DisplayGraph(doWydruku(idealAutomat), doWydruku(bestSolucja), (double)idealneWyniki[idealneWyniki.Count - 1], pathZdjecie);
                    //TUTAJ TRZA ZPAISAC
                    //BitmapImage test = displayGraph;

                }
                else if (mode == 1)
                {
                    List<Automat> solucjekazde = new List<Automat>();
                    List<double> learningwynikiKazde = new List<double>();
                    
                    double minimalFinalErr = 100.0;
                    int bestFinalAutomatIndex = 0;
                    for (int i = 0; i < jedneRozwiaznia.Count; i++)
                    {
                        if ((double)wynikiKazde[i] < minimalFinalErr)
                        {
                            minimalFinalErr = (double)wynikiKazde[i];
                            bestFinalAutomatIndex = i;
                        }
                    }

                    Automat Finalsolution = (Automat)jedneRozwiaznia[bestFinalAutomatIndex];

                    sw.WriteLine("----------------------------------------------------");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("--------------------BEST SOLUTION-------------------");


                    String tmpString2 = prepareStringToWynik(Finalsolution);

                    String wynik2 = "error = " + minimalFinalErr;
                    sw.WriteLine(tmpString2);
                    sw.WriteLine(wynik2);

                    //DisplayGraph displayGraph = new DisplayGraph(doWydruku(idealAutomat), doWydruku(Finalsolution), minimalFinalErr);
                    DisplayGraph displayGraph = new DisplayGraph(doWydruku(idealAutomat), doWydruku(Finalsolution), minimalFinalErr, pathZdjecie);
                    //TUTAJ TRZA ZPAISAC
                }
            }
        }

        private async void TEST_Click(object sender, RoutedEventArgs e)
        {
            //slowa wgrane
            LoadWordSet("H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\malySet.txt");
            //LoadWordSet("H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\duzySet.txt");

            //dla czterech typow automatu //4
            for(int typy = 0 ; typy < 4 ; typy ++)
            {               
                idealAutomat = new Automat();

                //10 typow automatu z kazdego typu //10
                for( int aut = 0 ; aut < 10 ; aut ++)
                {
                    String sciezka = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\AUTOMATY\\";
                    String sciezkaWynik = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\AUTOMATY\\";
                    String sciezkaZdjecie = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\AUTOMATY\\";

                    switch(typy)
                    {
                        case 0 :
                            sciezka = sciezka + "4statesAutomat\\4_5_";
                            sciezkaWynik = sciezkaWynik + "4statesAutomat\\wyniki\\4_5_";
                            break;
                        case 1 :
                            sciezka = sciezka + "6statesAutomat\\6_5_";
                            sciezkaWynik = sciezkaWynik + "6statesAutomat\\wyniki\\6_5_";
                            break;
                        case 2 :
                            sciezka = sciezka + "10statesAutomat\\10_5_";
                            sciezkaWynik = sciezkaWynik + "10statesAutomat\\wyniki\\10_5_";
                            break;
                        case 3 :
                            sciezka = sciezka + "15statesAutomat\\15_5_";
                            sciezkaWynik = sciezkaWynik + "15statesAutomat\\wyniki\\15_5_";
                            break;
                    }
                    sciezkaZdjecie = sciezkaWynik;
                    switch(aut)
                    {
                        case 0 :
                            sciezka = sciezka + "1automat.txt";
                            sciezkaWynik = sciezkaWynik + "1automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "1automatpicture";
                            break;
                        case 1 :
                            sciezka = sciezka + "2automat.txt";
                            sciezkaWynik = sciezkaWynik + "2automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "2automatpicture";
                            break;
                        case 2 :
                            sciezka = sciezka + "3automat.txt";
                            sciezkaWynik = sciezkaWynik + "3automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "3automatpicture";
                            break;
                        case 3 :
                            sciezka = sciezka + "4automat.txt";
                            sciezkaWynik = sciezkaWynik + "4automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "4automatpicture";
                            break;
                        case 4 :
                            sciezka = sciezka + "5automat.txt";
                            sciezkaWynik = sciezkaWynik + "5automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "5automatpicture";
                            break;
                        case 5 :
                            sciezka = sciezka + "6automat.txt";
                            sciezkaWynik = sciezkaWynik + "6automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "6automatpicture";
                            break;
                        case 6 :
                            sciezka = sciezka + "7automat.txt";
                            sciezkaWynik = sciezkaWynik + "7automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "7automatpicture";
                            break;
                        case 7 :
                            sciezka = sciezka + "8automat.txt";
                            sciezkaWynik = sciezkaWynik + "8automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "8automatpicture";
                            break;
                        case 8 :
                            sciezka = sciezka + "9automat.txt";
                            sciezkaWynik = sciezkaWynik + "9automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "9automatpicture";
                            break;
                        case 9 :
                            sciezka = sciezka + "10automat.txt";
                            sciezkaWynik = sciezkaWynik + "10automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "10automatpicture";
                            break;
                    }
                    
                    LoadAutomata(sciezka);

                    Console.WriteLine("puszczam PSO dla kolejnego zestawu");
                    //mamy tu juz slowa i mamy tu juz automat.

                    if (valideData() == true)
                    {
                        progressRing.Visibility = Visibility.Visible;
                        progressRingBackground.Visibility = Visibility.Visible;
                        await PSO();
                        progressRing.Visibility = Visibility.Collapsed;
                        progressRingBackground.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("Not all data loaded !");
                        return;
                    }

                    zapiszWynik(sciezkaWynik, 0, sciezkaZdjecie);

                    
                }
            }

            Console.WriteLine("RECONSTRUCTION FINISHED");

        }

        private async void TEST2_Click(object sender, RoutedEventArgs e)
        {
            idealneSolucje.Clear();
            idealneWyniki.Clear();
            wszystkieSolucje.Clear();
            wszystkieLearningoweWyniki.Clear();
            wszystkieWyniki.Clear();

            finMin.IsChecked = false;
            LoadWordSet("H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\malySet.txt");
            //LoadWordSet("H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\duzySet.txt");

            //dla czterech typow automatu //4
            for (int typy = 0; typy < 4; typy++)
            {
                idealAutomat = new Automat();

                //10 typow automatu z kazdego typu //10
                for (int aut = 0; aut < 10; aut++)
                {
                    idealneSolucje.Clear();
                    idealneWyniki.Clear();
                    wszystkieSolucje.Clear();
                    wszystkieLearningoweWyniki.Clear();
                    wszystkieWyniki.Clear();

                    String sciezka = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\APPROXIMATION\\";
                    String sciezkaWynik = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\APPROXIMATION\\";
                    String sciezkaZdjecie = "H:\\Windows7\\Documents\\Visual Studio 2013\\Projects\\AC\\APPROXIMATION\\";

                    switch (typy)
                    {
                        case 0:
                            sciezka = sciezka + "20states\\20_5_";
                            sciezkaWynik = sciezkaWynik + "20states\\wyniki\\20_5_";
                            break;
                        case 1:
                            sciezka = sciezka + "30states\\30_5_";
                            sciezkaWynik = sciezkaWynik + "30states\\wyniki\\30_5_";
                            break;
                        case 2:
                            sciezka = sciezka + "50states\\50_5_";
                            sciezkaWynik = sciezkaWynik + "50states\\wyniki\\50_5_";
                            break;
                        case 3:
                            sciezka = sciezka + "80states\\80_5_";
                            sciezkaWynik = sciezkaWynik + "80states\\wyniki\\80_5_";
                            break;
                    }

                    sciezkaZdjecie = sciezkaWynik;

                    switch (aut)
                    {
                        case 0:
                            sciezka = sciezka + "1automat.txt";
                            sciezkaWynik = sciezkaWynik + "1automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "1automatpicture";
                            break;
                        case 1:
                            sciezka = sciezka + "2automat.txt";
                            sciezkaWynik = sciezkaWynik + "2automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "2automatpicture";
                            break;
                        case 2:
                            sciezka = sciezka + "3automat.txt";
                            sciezkaWynik = sciezkaWynik + "3automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "3automatpicture";
                            break;
                        case 3:
                            sciezka = sciezka + "4automat.txt";
                            sciezkaWynik = sciezkaWynik + "4automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "4automatpicture";
                            break;
                        case 4:
                            sciezka = sciezka + "5automat.txt";
                            sciezkaWynik = sciezkaWynik + "5automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "5automatpicture";
                            break;
                        case 5:
                            sciezka = sciezka + "6automat.txt";
                            sciezkaWynik = sciezkaWynik + "6automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "6automatpicture";
                            break;
                        case 6:
                            sciezka = sciezka + "7automat.txt";
                            sciezkaWynik = sciezkaWynik + "7automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "7automatpicture";
                            break;
                        case 7:
                            sciezka = sciezka + "8automat.txt";
                            sciezkaWynik = sciezkaWynik + "8automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "8automatpicture";
                            break;
                        case 8:
                            sciezka = sciezka + "9automat.txt";
                            sciezkaWynik = sciezkaWynik + "9automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "9automatpicture";
                            break;
                        case 9:
                            sciezka = sciezka + "10automat.txt";
                            sciezkaWynik = sciezkaWynik + "10automatWYNIK.txt";
                            sciezkaZdjecie = sciezkaZdjecie + "10automatpicture";
                            break;
                    }

                    LoadAutomata(sciezka);

                    Console.WriteLine("puszczam PSO dla kolejnego zestawu");

                    int [] stany = {4,6,8,10,12};

                    for (int s = 0; s < 5; s++ )
                    {
                        MinstatesTXT.Text = ""+stany[s];
                        MaxstatesTXT.Text = ""+stany[s];

                        if (valideData() == true)
                        {
                            progressRing.Visibility = Visibility.Visible;
                            progressRingBackground.Visibility = Visibility.Visible;
                            await PSO();
                            progressRing.Visibility = Visibility.Collapsed;
                            progressRingBackground.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            MessageBox.Show("Not all data loaded !");
                            return;
                        }
                    }

                    zapiszWynik(sciezkaWynik, 1, sciezkaZdjecie);
                }
            }

            Console.WriteLine("APPROXIMATION FINISHED");
        }
    }
}
