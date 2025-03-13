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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _4labDES
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

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            Console.Clear();
            var rnd = new Random();
            ushort key = Convert.ToUInt16(rnd.Next(65535)); // Пример 16-битного ключа
            Console.WriteLine($"Key: {Convert.ToString(key, 2).PadLeft(16, '0')}");
            DES.WriteStringToFile(inputText.Text);
            DES.EncryptFile("input.bin", "encrypted.bin", key);
            Console.WriteLine("\nЗашифрованный текст");
            PrintBinaryFile("encrypted.bin");
            DES.DecryptFile("encrypted.bin", "decrypted.bin", key);
            Console.WriteLine("\nДешифрованный текст");
            PrintBinaryFile("decrypted.bin");
        }

        public static void PrintBinaryFile(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);

            // Вывод в HEX-формате
            Console.WriteLine("HEX содержимое:");
            Console.WriteLine(BitConverter.ToString(data).Replace("-", " "));

            // Вывод в ASCII (если данные текстовые)
            Console.WriteLine("\nASCII содержимое:");
            string ascii = Encoding.UTF8.GetString(data);
            Console.WriteLine(ascii);
        }

        private void inputFile_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
