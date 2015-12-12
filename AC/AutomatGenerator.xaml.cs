using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AC
{
    /// <summary>
    /// Interaction logic for AutomatGenerator.xaml
    /// </summary>
    public partial class AutomatGenerator : Window
    {
        Random random = new Random();
        public int StateNumber;
        public int SymbolNumber;
        public List<int[]> transitionTableFinal;
        public bool isGenerated = false;
        public AutomatGenerator()
        {
            InitializeComponent();
            transitionTableFinal = new List<int[]>();
        }


        public int getStates()
        {
            return StateNumber;
        }
        public int getAlp()
        {
            return SymbolNumber;
        }
        public List<int[]> gettrans()
        {
            return transitionTableFinal;
        }


        private void Create_Click(object sender, RoutedEventArgs e)
        {

            isGenerated = false;
            StateNumber = int.Parse(StateNumberTxt.Text);
            SymbolNumber = int.Parse(SymbolNumberTxt.Text);

            List<int[]> transitionTable = new List<int[]>();

            bool isInCorrect = true;


            for (int i = 0; i < StateNumber; i++)
            {
                int[] row = new int[SymbolNumber];

                for (int y = 0; y < SymbolNumber; y++)
                {
                    int transition = randInt(0, StateNumber - 1, random);
                    row[y] = transition;

                }

                transitionTable.Add(row);
            }


            while (isInCorrect)
            {
                int wynikSprawdzenia = validateAutomat(transitionTable, StateNumber, SymbolNumber);
                if (wynikSprawdzenia == 0)
                {
                    isInCorrect = false;
                }

                if (isInCorrect == true)
                {
                    transitionTable = RepairAutomat(transitionTable, StateNumber, SymbolNumber, wynikSprawdzenia);
                }
            }


            Console.WriteLine("-----GOOD ONE AUTOMAT CREATED-------");

            transitionTableFinal = transitionTable;
            isGenerated = true;
        }

        public List<int[]> RepairAutomat(List<int[]> transitionTable, int StateNumber, int SymbolNumber, int coNaprawic)
        {
            if (coNaprawic == 1 || coNaprawic == 3)
            {
                int[] Initialrow = transitionTable[0];

                bool initial = false;

                while (initial == false)
                {
                    int transition = randInt(0, StateNumber - 1, random);
                    int position = randInt(0, SymbolNumber - 1, random);

                    if (transition != 0)
                    {
                        initial = true;
                        Initialrow[position] = transition;
                    }
                }

                transitionTable[0] = Initialrow;
            }

            if (coNaprawic == 2 || coNaprawic == 3)
            {
                /*
                 * do stanu N nie ma dojscia.
                 * lece po pozostalych stanach i szukam pierwszego w ktorym 
                 * jest tranzycja na siebie samego i ja zamieniam na stan N
                 * jak nei ma stanow z tranzycja na siebie to losowemy stanowi
                 * na losowej pozycji wstawiam N
                 */
                for (int i = 1; i < StateNumber; i++)
                {
                    bool CzyStateJestOk = false;
                    for (int j = 0; j < StateNumber; j++)
                    {
                        if (i != j)
                        {
                            int[] row = transitionTable[j];

                            for (int y = 0; y < SymbolNumber; y++)
                            {
                                if (row[y] == i)
                                {
                                    CzyStateJestOk = true;
                                }
                            }
                        }
                        if (CzyStateJestOk == true)
                        {
                            break;
                        }
                    }
                    if (CzyStateJestOk == false)
                    {
                        bool dodalem = false;
                        for (int j = 0; j < StateNumber; j++)
                        {
                            if (i != j)
                            {
                                int[] row = transitionTable[j];

                                for (int y = 0; y < SymbolNumber; y++)
                                {
                                    if (row[y] == j)
                                    {
                                        row[y] = i;
                                        dodalem = true;
                                        break;
                                    }
                                }
                                transitionTable[j] = row;
                            }
                            if (dodalem == true)
                            {
                                break;
                            }
                        }

                        if (dodalem == false)
                        {
                            int state = randInt(1, StateNumber - 1, random);
                            int position = randInt(0, SymbolNumber - 1, random);

                            int[] row = transitionTable[state];
                            row[position] = i;

                            transitionTable[state] = row;
                        }
                    }
                }

            }

            return transitionTable;
        }

        public int validateAutomat(List<int[]> transitionTable, int StateNumber, int SymbolNumber)
        {
            /*
             * jezeli dla kazdego stanu ( np i ) w jakimkolwiek rzedzie 
             * oprocz rzedu i, znajdziemy chodziaz jedna liczba i 
             * to oznacza ze :
             * 1 - jest to stan osiagalny. ( wyjatek stanowi stan 0 )
             * 2 - jest ten stan polaczaony z calym grafem 
             * dla stanu 0 ktory nie musi byc osiagalny sprawdzamy czy jest chociaz jedno wyjdzie dalej:
             * jezeli w rzedzie 0 jest conajmniej jeden symbol onny niz 0
             */

            bool CzyJestOkSumbols = true;
            bool CzyJestOkInitial = true;

            for (int i = 1; i < StateNumber; i++)
            {
                bool CzyStateJestOk = false;
                for (int j = 0; j < StateNumber; j++)
                {
                    if (i != j)
                    {
                        int[] row = transitionTable[j];

                        for (int y = 0; y < SymbolNumber; y++)
                        {
                            if (row[y] == i)
                            {
                                CzyStateJestOk = true;
                            }
                        }
                    }
                    if (CzyStateJestOk == true)
                    {
                        break;
                    }
                }
                if (CzyStateJestOk == false)
                {
                    CzyJestOkSumbols = false;
                    break;
                }

            }


            CzyJestOkInitial = false;
            int[] row2 = transitionTable[0];
            for (int y = 0; y < SymbolNumber; y++)
            {
                if (row2[y] != 0)
                {
                    CzyJestOkInitial = true;
                    break;
                }
            }


            if (CzyJestOkSumbols == true && CzyJestOkInitial == true)
            {
                return 0;
            }
            else if (CzyJestOkSumbols == false && CzyJestOkInitial == true)
            {
                return 2;
            }
            else if (CzyJestOkSumbols == true && CzyJestOkInitial == false)
            {
                return 1;
            }
            else
            {
                return 3;
            }
        }

        int randInt(int from, int to, Random random)
        {

            int wynik = 0;

            double x = random.NextDouble() * (to - from);

            wynik = from + (int)Math.Round(x);

            return wynik;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {


            if(isGenerated == false)
            {
                MessageBox.Show("No automaton created");
                return;
            }


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                using (StreamWriter sw = new StreamWriter(dlg.FileName))
                {
                    String a = StateNumber + "," + SymbolNumber;

                    for (int j = 0; j < StateNumber; j++)
                    {
                        int[] row = transitionTableFinal[j];
                        for(int i = 0 ; i < SymbolNumber ; i++)
                        {
                            a = a + ","+(row[i] + 1 );
                        }
                    }

                    sw.WriteLine(a);
                }
                MessageBox.Show("Words Created and file saved!");

                this.DialogResult = true;
                this.Close();

            }
            else
            {
                MessageBox.Show("Words Created ! Saving Aborted!");
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
