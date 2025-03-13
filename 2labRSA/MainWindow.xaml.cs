using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using System.Numerics;
using System.Security.Permissions;
using static System.Net.Mime.MediaTypeNames;

namespace _2labRSA
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

        private static Random _random = new Random();
        public BigInteger keyE;
        private BigInteger keyD;
        public BigInteger keyN;

        private void generateKeysButton_Click(object sender, RoutedEventArgs e)
        {
            BigInteger keyP = GenerateLargePrime(12);
            BigInteger keyQ = GenerateLargePrime(12);
            keyN = keyP * keyQ;
            BigInteger keyPhi = (keyP - 1) * (keyQ - 1);
            keyE = GenerateE(keyPhi, keyN);
            BigInteger keyY;
            (keyD, keyY) = FindDY(keyE, keyPhi);

            pLabel.Text = keyP.ToString();
            qLabel.Text = keyQ.ToString();
            nLabel.Text = keyN.ToString();
            phiLabel.Text = keyPhi.ToString();
            eLabel.Text = keyE.ToString();
            dLabel.Text = keyD.ToString();
            yLabel.Text = keyY.ToString();
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            string text = encryptPhrase.Text;
            if (text.Length == 0)
            {
                encryptPhrase.Text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            byte[] textData = Encoding.ASCII.GetBytes(text);
            int maxBlockSize = keyN.ToByteArray().Length - 1;

            int index = 0;
            while (index < textData.Length)
            {
                int blockSize = Math.Min(maxBlockSize, textData.Length - index);
                var block = textData.Skip(index).Take(blockSize).ToArray();
                Array.Reverse(block);

                BigInteger biFromBlock = new BigInteger(block);
                Console.WriteLine(biFromBlock);
                sb.Append($" {BigInteger.ModPow(biFromBlock, keyE, keyN)}");

                index += blockSize;
            }


            decryptPhrase.Text = sb.ToString(1, sb.Length - 1);
        }


        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            string text = decryptPhrase.Text;
            if (text.Length == 0)
            {
                decryptPhrase.Text = string.Empty;
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in text.Split().Select(t => BigInteger.Parse(t)))
                {
                    var bytes = BigInteger.ModPow(item, keyD, keyN).ToByteArray();
                    Array.Reverse(bytes);

                    sb.Append(Encoding.ASCII.GetString(bytes));
                }

                encryptPhrase.Text = sb.ToString();
            }
            catch
            {
                throw new Exception("Error. The text is not decipherable");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        static BigInteger GenerateE(BigInteger f, BigInteger n)
        {
            BigInteger E;

            do
            {
                E = RandomBigInteger(0, n, _random);
            }
            while (IsCoprime(E, f) != true);

            return E;
        }

        // Проверка двух чисел на взаимную простоту
        static bool IsCoprime(BigInteger e, BigInteger phi)
        {
            var (gcd, x, _) = ExtendedGcd(e, phi);
            return gcd == 1;
        }

        public static (BigInteger, BigInteger) FindDY(BigInteger e, BigInteger phi)
        {
            var (gcd, d, y) = ExtendedGcd(e, phi);

            if (gcd != 1)
                throw new ArgumentException("e и φ(n) должны быть взаимно простыми.");

            // Обеспечиваем, что x положительный (модуль phi)
            return ((d % phi + phi) % phi, y);
        }

        public static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            if (b == 0)
                return (a, 1, 0);

            var (gcd, x1, y1) = ExtendedGcd(b, a % b);
            BigInteger x = y1;
            BigInteger y = x1 - (a / b) * y1;

            return (gcd, x, y);
        }

        static bool IsProbablyPrime(BigInteger n, int k) //k - количество итераций алгоритма Кевина-Миллера(проверка на простату)
        {
            if (n < 2)
                return false;
            if (n == 2 || n == 3)
                return true;
            if (n % 2 == 0)
                return false;

            BigInteger d = n - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            for (int i = 0; i < k; i++)
            {
                BigInteger a = RandomBigInteger(2, n - 2, _random);
                BigInteger x = BigInteger.ModPow(a, d, n);

                if (x == 1 || x == n - 1)
                    continue;

                for (int j = 0; j < s - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == 1)
                        return false;
                    if (x == n - 1)
                        break;
                }

                if (x != n - 1)
                    return false;
            }

            return true;
        }

        static BigInteger GenerateLargePrime(int minDigits)
        {
            BigInteger prime;

            do
            {
                // Генерация случайного числа с minDigits разрядами
                string digits = "";

                // Первая цифра должна быть от 1 до 9
                digits += _random.Next(1, 10).ToString();

                // Остальные цифры могут быть от 0 до 9
                for (int i = 1; i < minDigits; i++)
                {
                    digits += _random.Next(0, 10).ToString();
                }

                prime = BigInteger.Parse(digits);
            }
            while (!IsProbablyPrime(prime, 5)); // Проверка на простоту

            return prime;
        }

        static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random random)
        {
            byte[] bytes = max.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; // Убедимся, что число положительное
                result = new BigInteger(bytes);
            }
            while (result < min || result >= max);

            return result;
        }

        private void clearEncrypt_Click(object sender, RoutedEventArgs e)
        {
            encryptPhrase.Text = string.Empty;
        }

        private void clearDecrypt_Click(object sender, RoutedEventArgs e)
        {
            decryptPhrase.Text = string.Empty;
        }

        private void eLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            keyE = BigInteger.Parse(eLabel.Text);
        }
    }
}
