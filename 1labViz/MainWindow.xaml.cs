using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace _1labViz
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] phrase = Phrase.Text.ToLower().ToCharArray();
            char[] key = Key.Text.ToLower().Replace(" ", "").ToCharArray();
            
            char[] endPhrase = phrase;
            bool? isRussian = ruLayout.IsChecked;

            if (isRussian == true)
            {
                int j = 0;
                if (key.Length == 0) key = new char[] { 'а' };
                for (int i = 0; i < phrase.Length; i++)
                {
                    if (j >= key.Length) j = 0;
                    if (endPhrase[i] == ' ') continue;
                    if (phrase[i] + (key[j] - 'а') <= 'я')
                    {
                        endPhrase[i] = (char)(phrase[i] + (key[j] - 'а'));
                    }
                    else
                    {
                        endPhrase[i] = (char)(phrase[i] + (key[j] - 'а') - 32);
                    }
                    j++;
                }
            }
            else
            {
                int j = 0;
                if (key.Length == 0) key = new char[] { 'a' };
                for (int i = 0; i < phrase.Length; i++)
                {
                    if (j >= key.Length) j = 0;
                    if (endPhrase[i] == ' ') continue;
                    if (phrase[i] + (key[j] - 'a') <= 'z')
                    {
                        endPhrase[i] = (char)(phrase[i] + (key[j] - 'a'));
                    }
                    else
                    {
                        endPhrase[i] = (char)(phrase[i] + (key[j] - 'a') - 26);
                    }
                    j++;
                }
            }
            EndPhrase.Content = new string(endPhrase);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
