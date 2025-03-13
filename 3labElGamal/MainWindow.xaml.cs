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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Policy;

namespace _3labElGamal
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

        private BigInteger keyX;
        public BigInteger keyY;
        public BigInteger keyG;
        public BigInteger keyP;
        List<Message> messages = new List<Message>();

        private void generateKeysButton_Click(object sender, RoutedEventArgs e)
        {
            keyP = GenerateLargePrime(20);
            keyG = FindPrimitiveRoot(keyP);
            keyX = RandomBigInteger(0, keyP, _random);
            keyY = BigInteger.ModPow(keyG, keyX, keyP);

            pLabel.Text = keyP.ToString();
            gLabel.Text = keyG.ToString();
            xLabel.Text = keyX.ToString();
            yLabel.Text = keyY.ToString();
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            char[] text = encryptPhrase.Text.ToCharArray();
            if (text.Length == 0)
            {
                encryptPhrase.Text = string.Empty;
            }
            string result = "";
            messages.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                BigInteger message = text[i];
                BigInteger k = GenerateK(keyP - 1);
                BigInteger a = BigInteger.ModPow(keyG, k, keyP);
                BigInteger b = message ^ BigInteger.ModPow(keyY, k, keyP);
                BigInteger r = a;
                BigInteger kInverse = CalculateModInverse(k, keyP - 1);
                BigInteger s = kInverse * ((message - keyX * r) % (keyP - 1) + (keyP - 1)) % (keyP - 1);
                result += $"({a},{b})\n";
                messages.Add(new Message(message, r, s));
            }
            decryptPhrase.Text = result;
        }


        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            string text = decryptPhrase.Text;
            string result = "";
            if (text.Length == 0)
            {
                decryptPhrase.Text = string.Empty;
            }

            try
            {
                string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach(string line in lines)
                {
                    string[] row = new string[2];
                    row = line.Replace("(", "").Replace(")", "").Split(',');
                    BigInteger a = BigInteger.Parse(row[0]);
                    BigInteger b = BigInteger.Parse(row[1]);
                    BigInteger message = BigInteger.ModPow(a, keyX, keyP) ^ b;
                    result += (char)message;
                }
                encryptPhrase.Text = result;
                
                //byte[] messageBytes = message.ToByteArray();

                //Array.Reverse(messageBytes);
                //string decryptedText = Encoding.ASCII.GetString(messageBytes).TrimEnd('\0');

                //encryptPhrase.Text = decryptedText;
            }
            catch
            {
                throw new Exception("Error. The text is not decipherable");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public static BigInteger GenerateK(BigInteger f)
        {
            BigInteger K;

            do
            {
                K = GenerateLargePrime(20);
            }
            while (!IsCoprime(K,f) && K > f);

            return K;
        }

        // Проверка двух чисел на взаимную простоту
        static bool IsCoprime(BigInteger e, BigInteger phi)
        {
            var (gcd, x, _) = ExtendedGcd(e, phi);
            return gcd == 1;
        }

        public static BigInteger FindPrimitiveRoot(BigInteger p)
        {
            if (p < 2)
                throw new ArgumentException("p must be at least 2");
            if (p == 2)
                return 1;
            if (!IsProbablyPrime(p, 5))
                throw new ArgumentException("p must be a prime number");

            BigInteger phi = p - 1;
            List<BigInteger> primeFactors = GetPrimeFactors(phi);

            for (BigInteger g = 10000000000000000000; g < p; g++)
            {
                bool isPrimitive = true;
                foreach (BigInteger q in primeFactors)
                {
                    if (BigInteger.ModPow(g, phi / q, p) == 1)
                    {
                        isPrimitive = false;
                        break;
                    }
                }
                if (isPrimitive)
                    return g;
            }

            throw new InvalidOperationException("Primitive root not found, but p is prime. This should not happen.");
        }

        private static List<BigInteger> GetPrimeFactors(BigInteger n)
        {
            List<BigInteger> factors = new List<BigInteger>();
            if (n % 2 == 0)
            {
                factors.Add(2);
                while (n % 2 == 0)
                    n /= 2;
            }
            for (BigInteger i = 3; (BigInteger)i * i <= n; i += 2)
            {
                if (n % i == 0)
                {
                    factors.Add(i);
                    while (n % i == 0)
                        n /= i;
                }
            }
            if (n > 1)
                factors.Add(n);
            return factors;
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

        public BigInteger CalculateModInverse(BigInteger number, BigInteger mod)
        {
            var (gcd,a,b) = ExtendedGcd(number,mod);

            if (gcd != 1)
            {
                throw new ArgumentException("Error! Numbers are not coprime!");
            }

            return a < 0 ? a + mod : a;
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

        private void checkSignature_Click(object sender, RoutedEventArgs e)
        {

            try
            {
#if DEBUG
                Console.Clear();
                Console.WriteLine($"p = {keyP}");
                Console.WriteLine($"g = {keyG}");
                Console.WriteLine($"x = {keyX}");
                Console.WriteLine($"y = {keyY}");
                Console.WriteLine();
                Console.WriteLine("y^r * r^s == g^m mod p");
#endif

                bool isMatches = true;
                for (int i = 0; isMatches && i < messages.Count; i++)
                {
                    BigInteger leftPart = BigInteger.ModPow(keyY, messages[i].r, keyP) % keyP *
                       BigInteger.ModPow(messages[i].r, messages[i].s, keyP) % keyP;

                    BigInteger rightPart = BigInteger.ModPow(keyG, messages[i].hash, keyP);

                    isMatches = leftPart == rightPart;

#if DEBUG
                    Console.WriteLine($"{leftPart} == {rightPart}");
#endif
                }
            }
            catch
            {
                throw new Exception("Error. The text is not decipherable");
            }
        }
    }
}
