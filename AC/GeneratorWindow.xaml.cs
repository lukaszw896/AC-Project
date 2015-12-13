using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AC
{
    /// <summary>
    /// Interaction logic for GeneratorWindow.xaml
    /// </summary>
    public partial class GeneratorWindow : Window
   {

        public List<List<int>> setOfWords;

        public GeneratorWindow()
        {
            InitializeComponent();
            setOfWords = new List<List<int>>();
            
        }

        public List<List<int>> getWords()
        {
            return setOfWords;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            setOfWords.Clear();


            //walidacja inputu

            Random random = new Random();
            int C = int.Parse(MinWordTxt.Text);
            int maxLen = int.Parse(MaxWordTxt.Text);
            int setNumber = int.Parse(SetNmbrTxt.Text);
            int maxSetSize = int.Parse(MaxBigSetTxt.Text);


            if(validateInput() == false)
            {
                MessageBox.Show("Incorrect input");
                return;
            }


            ArrayList Bigwordlengths = new ArrayList();

            int step = (int)Math.Round((double)(((double)(maxLen - (C))) / (double)setNumber), 0);

            for(int i = 0 ; i < setNumber; i ++)
            {
                /*if(i==0)
                {
                    Bigwordlengths.Add(C + 1);
                }*/
                if (i == setNumber - 1)
                {
                    Bigwordlengths.Add(maxLen);
                }
                else
                {
                    int lng = 0;
                    lng = (C) + (i * step);
                    if(lng == C)
                    {
                        lng++;
                    }
                    Bigwordlengths.Add(lng);
                }
            }

            //AlphabetTxt
            
            ArrayList letters = new ArrayList();
            int inLetters = int.Parse(AlphabetTxt.Text);
            // string[] tokens = inLetters.Split(',');
            for (int i = 0; i < inLetters; i++)
            {
                letters.Add(i);
            }
            
            List<List<int>> setOfSmallWords = new List<List<int>>();
            List<List<int>>  setOfBigWords = new List<List<int>>();



            for(int i = 0 ; i <= C ; i++)
            {

                ArrayList templist = new ArrayList();
                int ileslow = maxCombination(letters, i);


                for (int j = 0; j < ileslow; j++)
                {
                    String tempWord = "";
                    bool canAdd = false;

                    while (canAdd == false)
                    {
                        for (int l = 0; l < i; l++)
                        {
                            int index = random.Next(0, letters.Count);
                            tempWord = tempWord + letters[index];
                        }

                        if (validateWord(templist, tempWord))
                        {
                            canAdd = true;
                        }
                        else
                        {
                            tempWord = "";
                        }
                    }
                    templist.Add(tempWord);
                }


                for (int p = 0; p < templist.Count; p++)
                {
                    List<int> word = new List<int>();
                    foreach (char c in (string)templist[p])
                    {
                        word.Add(int.Parse(c.ToString()));
                    }
                    setOfSmallWords.Add(word);
                    setOfWords.Add(word);
                }
            }




            if(maxSetSize < setOfSmallWords.Count)
            {
                maxSetSize = setOfSmallWords.Count + 1;
            }

            ArrayList nieMozemypowtorzyc = new ArrayList();
            int wordsPerGroup = (int)(maxSetSize / setNumber);


            for (int i = 0; i < Bigwordlengths.Count; i++)
            {
                int currLeng = (int)Bigwordlengths[i];
                ArrayList templist = new ArrayList();
                int ileslow = maxCombination(letters, currLeng);


                if (ileslow > wordsPerGroup)
                {
                    ileslow = wordsPerGroup;
                }


                if (i == Bigwordlengths.Count - 1)
                {
                    
                    if (setOfBigWords.Count + ileslow <= maxSetSize)
                    {
                        ileslow = maxSetSize - setOfBigWords.Count + 1;
                    }
                    if (setOfBigWords.Count + ileslow <= setOfSmallWords.Count)
                    {
                        ileslow = setOfSmallWords.Count - setOfBigWords.Count + 1;
                    }
                }

                for (int j = 0; j < ileslow; j++)
                {
                    String tempWord = "";
                    bool canAdd = false;

                    while (canAdd == false)
                    {
                        for (int l = 0; l < currLeng; l++)
                        {
                            int index = random.Next(0, letters.Count);
                            tempWord = tempWord + letters[index];
                        }

                        if (validateWord(templist, tempWord))
                        {
                            canAdd = true;
                        }
                        else
                        {
                            tempWord = "";
                        }
                    }
                    templist.Add(tempWord);
                }


                for (int p = 0; p < templist.Count; p++)
                {
                    List<int> word = new List<int>();
                    foreach (char c in (string)templist[p])
                    {
                        word.Add(int.Parse(c.ToString()));
                    }
                    setOfWords.Add(word);
                    setOfBigWords.Add(word);
                    nieMozemypowtorzyc.Add(templist[p]);
                }
            }

            String separator = "99999999999999999999";
            List<int> separator2 = new List<int>();
            foreach (char c in (string)separator)
                   {
                        separator2.Add(int.Parse(c.ToString()));
                   }
            setOfWords.Add(separator2);

            //console print
            /*for(int i = 0 ; i < setOfWords.Count ; i++)
            {
                Console.WriteLine(i+". - "+setOfWords[i]);
            }*/


            //setOfWords = setOfSmallWords + setOfBigWords;




            List<List<int>> TestingSetWords = new List<List<int>>();


            for (int i = 0; i < Bigwordlengths.Count; i++)
            {
                int currLeng = (int)Bigwordlengths[i];
                ArrayList templist = new ArrayList();
                int ileslow = maxCombination(letters, currLeng);
                bool canGenerateMorewords = true;
                if (2 * ileslow <= wordsPerGroup)
                {
                    canGenerateMorewords = false;
                }

                if (canGenerateMorewords == true)
                {
                    if (ileslow > wordsPerGroup)
                    {
                        ileslow = wordsPerGroup;
                    }

                    if (i == Bigwordlengths.Count - 1)
                    {
                        if (TestingSetWords.Count + ileslow <= setOfBigWords.Count + setOfSmallWords.Count)
                        {
                            ileslow = setOfBigWords.Count + setOfSmallWords.Count - TestingSetWords.Count + 1;
                        }
                        if (TestingSetWords.Count + ileslow <= maxSetSize)
                        {
                            ileslow = maxSetSize - TestingSetWords.Count + 1;
                        }
                    }

                    for (int j = 0; j < ileslow; j++)
                    {
                        String tempWord = "";
                        bool canAdd = false;

                        while (canAdd == false)
                        {
                            for (int l = 0; l < currLeng; l++)
                            {
                                int index = random.Next(0, letters.Count);
                                tempWord = tempWord + letters[index];
                            }

                            if (validateWord(templist, tempWord) )
                            {
                                if(validateWord(nieMozemypowtorzyc, tempWord))
                                {
                                      canAdd = true;
                                }
                            }
                            else
                            {
                                tempWord = "";
                            }
                        }
                        templist.Add(tempWord);
                       // Console.WriteLine(j + " / " + ileslow);
                    }


                    for (int p = 0; p < templist.Count; p++)
                    {
                        List<int> word = new List<int>();
                        foreach (char c in (string)templist[p])
                        {
                            word.Add(int.Parse(c.ToString()));
                        }
                        TestingSetWords.Add(word);
                        setOfWords.Add(word);
                    }
                    List<int> tmp = new List<int>();
                    tmp.Add(C);
                    setOfWords.Add(tmp);
                }
            }





            Console.WriteLine("Generated " + setOfWords.Count+ " words.");

            saveToFile();

            
        }

        bool validateWord(ArrayList lista , String word)
        {
            bool returned = true;

            if (lista.Count >= 1)
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    String tmpwrd = (String)lista[i];

                    if (tmpwrd.Equals(word))
                    {
                        returned = false;
                        //Console.WriteLine("Wykrylem takie samo slowo ! : "+ word);
                    }
                }
            }
            return returned;
        }
        int maxCombination(ArrayList letters, int length)//, int words)
        {
            int returned = 0;
            returned = (int)Math.Pow(letters.Count,length);
            int tmp = int.Parse(""+returned);
            if (tmp < 0)
               returned = 100000;
            return returned;
        }
        bool validateInput()
        {
            bool returned = true;

            if (int.Parse(MinWordTxt.Text) < 0 || int.Parse(MinWordTxt.Text)>int.Parse(MaxWordTxt.Text))
            {
                returned = false;
            }

            if (int.Parse(SetNmbrTxt.Text) > (int.Parse(MaxWordTxt.Text) - int.Parse(MinWordTxt.Text) + 1))
            {
                returned = false;
            }

            /*if (int.Parse(SetNmbrTxt.Text) < 1 || int.Parse(WordAmountTxt.Text) < 1)
            {
                returned = false;
            }*/
            if (AlphabetTxt.Text.Length <1)
            {
                returned = false;
            }

            if (int.Parse(MinWordTxt.Text) > 5)
            {
                returned = false;
            }

            return returned;
        }
        void saveToFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog()
            { 
                Filter = "Text Files(*.txt)|*.txt" 
            }; 

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                using (StreamWriter sw = new StreamWriter(dlg.FileName))
                {
                    for (int i = 0; i < setOfWords.Count; i++)
                    {
                        string tmpString = "";
                        foreach (int num in setOfWords[i])
                        {
                            tmpString = tmpString + num;
                        }
                        sw.WriteLine(tmpString);
                    }
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