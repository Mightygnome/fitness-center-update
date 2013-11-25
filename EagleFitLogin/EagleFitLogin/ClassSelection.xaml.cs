using System;
using System.Collections.Generic;
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

namespace EagleFitLogin
{
    /// <summary>
    /// Interaction logic for ClassSelection.xaml
    /// </summary>
    public partial class ClassSelection : Window
    {
        private bool selection;
        public ClassSelection()
        {
            selection = true;
            InitializeComponent();
        }

        private void btn_fastFitness_Click(object sender, RoutedEventArgs e)
        {
            selection = false;
            this.DialogResult = true;
        }

        private void btn_groupExercise_Click(object sender, RoutedEventArgs e)
        {
            selection = false;
            this.DialogResult = false;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(selection)
            e.Cancel = true;
        }
    }
}
