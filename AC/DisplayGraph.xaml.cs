using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using MahApps.Metro.Controls;
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
    /// Interaction logic for DisplayGraph.xaml
    /// </summary>
    public partial class DisplayGraph : MetroWindow
    {
        public DisplayGraph(List<int>[][] orginalAutomaton, List<int>[][] foundAutomaton, double error)
        {
            InitializeComponent();
            // These three instances can be injected via the IGetStartProcessQuery, 
            //                                               IGetProcessStartInfoQuery and 
            //                                               IRegisterLayoutPluginCommand interfaces

            ErrorResultLabel.Content = "Error for test set of found automaton: " + error;

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] orginal = wrapper.GenerateGraph(GenerateDotString(orginalAutomaton), Enums.GraphReturnType.Png);
            byte[] found = wrapper.GenerateGraph(GenerateDotString(foundAutomaton), Enums.GraphReturnType.Png);
            orginalAutomatonImage.Source = LoadImage(orginal);
            foundAutomatonImage.Source = LoadImage(found);
           // graphImage.Source = LoadImage(orginal);
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
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
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
            return image;
        }
    }
}
