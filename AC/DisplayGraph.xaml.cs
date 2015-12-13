using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for DisplayGraph.xaml
    /// </summary>
    public partial class DisplayGraph : MetroWindow
    {

        
        public  DisplayGraph(List<int>[][] orginalAutomaton, List<int>[][] foundAutomaton, double error, String path)
        {
            
            InitializeComponent();
            // These three instances can be injected via the IGetStartProcessQuery, 
            //                                               IGetProcessStartInfoQuery and 
            //                                               IRegisterLayoutPluginCommand interfaces

            ErrorResultLabel.Content = "Error for test set of found automaton: " + error;

            /*var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);*/

            List<byte[]> graphs = new List<byte[]>();
            Wut(graphs, orginalAutomaton, foundAutomaton, path);

            //byte[] orginal = wrapper.GenerateGraph(GenerateDotString(orginalAutomaton), Enums.GraphReturnType.Png);
            //byte[] found = wrapper.GenerateGraph(GenerateDotString(foundAutomaton), Enums.GraphReturnType.Png);

            
            //graphImage.Source = LoadImage(orginal);
        }

        async void Wut(List<byte[]> graphs,List<int>[][] orginalAutomaton, List<int>[][] foundAutomaton, String path)
        {
             graphs = await DrawGraph(orginalAutomaton, foundAutomaton);

             //orginalAutomatonImage.Source = 
            LoadImage(graphs[0], path+".jpg");
             //foundAutomatonImage.Source = 
            LoadImage(graphs[1], path+"2.jpg");

             

             int a = 1;
             a++;
        }

        public async Task<List<byte[]>> DrawGraph(List<int>[][] orginalAutomaton, List<int>[][] foundAutomaton)
        {
            List<byte[]> graphs = new List<byte[]>();
            await Task.Run(() => 
                {
                    var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);



            byte[] orginal = wrapper.GenerateGraph(GenerateDotString(orginalAutomaton), Enums.GraphReturnType.Png);
            byte[] found = wrapper.GenerateGraph(GenerateDotString(foundAutomaton), Enums.GraphReturnType.Png);

            graphs.Add(orginal);
            graphs.Add(found);
                }
            );
            return graphs;
            
        }

        public string GenerateDotString(List<int>[][] automaton)
        {
            string dotString = "digraph{";

            for (int i = 0; i < automaton.Count(); i++)
            {
                for (int j = 0; j < automaton[i].Count(); j++)
                {
                    if (automaton[i][j].Count > 0)
                    {
                        string stateToState = "" + i + " -> " + j + @" [label = """;
                        for (int k = 0; k < automaton[i][j].Count; k++)
                        {
                            stateToState += automaton[i][j][k];
                            if (k < automaton[i][j].Count - 1)
                            {
                                stateToState += ",";
                            }
                        }
                        stateToState += @"""] ;";
                        dotString += stateToState;
                    }
                }
            }

            dotString += "}";
                return dotString;
        }
        private static void LoadImage(byte[] imageData,String path)
        {
            /*if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;*/

            Bitmap bmp;
            using (var ms = new MemoryStream(imageData))
            {
                bmp = new Bitmap(ms);
                bmp.Save(path);
            }



        }
      
    }
}
