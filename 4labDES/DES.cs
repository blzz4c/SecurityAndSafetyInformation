using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4labDES
{
    internal class DES
    {
        // Начальная перестановка (IP)
        private static readonly int[] IP = { 5, 2, 7, 1, 15, 3, 0, 9, 6, 14, 11, 8, 12, 4, 13, 10 };

        // Конечная перестановка (IP⁻¹)
        private static readonly int[] FP = { 6, 3, 1, 5, 13, 0, 8, 2, 11, 7, 15, 10, 12, 14, 9, 4 };

        // Перестановка P в функции Фейстеля
        private static readonly int[] P = { 1, 3, 0, 2, 5, 7, 4, 6 };

        // S-боксы
        private static readonly byte[] SBox1 = { 0xE, 0x4, 0xD, 0x1, 0x2, 0xF, 0xB, 0x8, 0x3, 0xA, 0x6, 0xC, 0x5, 0x9, 0x0, 0x7 };
        private static readonly byte[] SBox2 = { 0x5, 0x9, 0x3, 0xB, 0x8, 0x0, 0xF, 0x4, 0x1, 0x7, 0x2, 0xE, 0x6, 0xC, 0xA, 0xD };

        // Генерация раундовых ключей
        private static List<byte> GenerateRoundKeys(ushort key)
        {
            var roundKeys = new List<byte>();
            byte k1 = (byte)(key >> 8);
            byte k2 = (byte)(key & 0xFF);

            for (int i = 0; i < 16; i++)
            {
                roundKeys.Add((i % 2 == 0) ? k1 : k2);
            }
            return roundKeys;
        }

        // Начальная перестановка
        private static ushort InitialPermutation(ushort block)
        {
            return Permute(block, IP);
        }

        // Конечная перестановка
        private static ushort FinalPermutation(ushort block)
        {
            return Permute(block, FP);
        }

        // Общая функция перестановки
        private static ushort Permute(ushort block, int[] table)
        {
            ushort result = 0;
            for (int i = 0; i < table.Length; i++)
            {
                int srcBit = table[i];
                ushort bit = (ushort)((block >> (15 - srcBit)) & 1);
                result |= (ushort)(bit << (15 - i));
            }
            return result;
        }

        // Функция Фейстеля
        private static byte F(byte r, byte k)
        {
            byte xored = (byte)(r ^ k);
            byte sOut = SBoxSubstitution(xored);
            return Permute(sOut);
        }

        // Подстановка S-боксов
        private static byte SBoxSubstitution(byte input)
        {
            byte upper = (byte)(input >> 4);
            byte lower = (byte)(input & 0x0F);
            return (byte)((SBox1[upper] << 4) | SBox2[lower]);
        }

        // Перестановка P
        private static byte Permute(byte b)
        {
            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                int srcPos = P[i];
                byte bit = (byte)((b >> (7 - srcPos)) & 1);
                result |= (byte)(bit << (7 - i));
            }
            return result;
        }

        // Шифрование блока
        public static ushort EncryptBlock(ushort block, ushort key)
        {
            block = InitialPermutation(block);
            byte l = (byte)(block >> 8);
            byte r = (byte)(block & 0xFF);
            var roundKeys = GenerateRoundKeys(key);

            for (int i = 0; i < 16; i++)
            {
                byte temp = r;
                r = (byte)(l ^ F(r, roundKeys[i]));
                l = temp;
            }

            ushort encrypted = (ushort)((r << 8) | l);
            return FinalPermutation(encrypted);
        }

        // Дешифрование блока
        public static ushort DecryptBlock(ushort block, ushort key)
        {
            block = InitialPermutation(block);
            byte r = (byte)(block >> 8);
            byte l = (byte)(block & 0xFF);
            var roundKeys = GenerateRoundKeys(key);

            for (int i = 15; i >= 0; i--)
            {
                byte temp = l;
                l = (byte)(r ^ F(l, roundKeys[i]));
                r = temp;
            }

            ushort decrypted = (ushort)((l << 8) | r);
            return FinalPermutation(decrypted);
        }

        // Дополнение данных
        public static byte[] AddPadding(byte[] data)
        {
            int blockSize = 2;
            int paddingLength = blockSize - (data.Length % blockSize);
            if (paddingLength == 0) paddingLength = blockSize;
            byte[] padded = new byte[data.Length + paddingLength];
            Array.Copy(data, padded, data.Length);
            for (int i = data.Length; i < padded.Length; i++)
                padded[i] = (byte)paddingLength;
            return padded;
        }

        // Удаление дополнения
        public static byte[] RemovePadding(byte[] data)
        {
            int paddingLength = data[data.Length - 1];
            if (paddingLength < 1 || paddingLength > 2)
                throw new ArgumentException("Invalid padding");
            byte[] result = new byte[data.Length - paddingLength];
            Array.Copy(data, result, result.Length);
            return result;
        }

        public static void WriteStringToFile(string text, string filePath = "input.bin")
        {
            // Преобразование строки в байты с использованием UTF-8 кодировки
            byte[] data = Encoding.UTF8.GetBytes(text);

            // Запись в бинарный файл
            File.WriteAllBytes(filePath, data);

            Console.WriteLine($"Строка успешно записана в файл: {filePath}");
        }

        // Шифрование файла
        public static void EncryptFile(string inputFile, string outputFile, ushort key)
        {
            byte[] data = File.ReadAllBytes(inputFile);
            byte[] paddedData = AddPadding(data);

            using (var fs = new FileStream(outputFile, FileMode.Create))
            {
                for (int i = 0; i < paddedData.Length; i += 2)
                {
                    ushort block = (ushort)((paddedData[i] << 8) | paddedData[i + 1]);
                    ushort encryptedBlock = EncryptBlock(block, key);
                    fs.WriteByte((byte)(encryptedBlock >> 8));
                    fs.WriteByte((byte)encryptedBlock);
                }
            }
        }

        // Дешифрование файла
        public static void DecryptFile(string inputFile, string outputFile, ushort key)
        {
            byte[] data = File.ReadAllBytes(inputFile);
            byte[] decryptedData = new byte[data.Length];

            using (var fs = new FileStream(outputFile, FileMode.Create))
            {
                for (int i = 0; i < data.Length; i += 2)
                {
                    ushort block = (ushort)((data[i] << 8) | data[i + 1]);
                    ushort decryptedBlock = DecryptBlock(block, key);
                    fs.WriteByte((byte)(decryptedBlock >> 8));
                    fs.WriteByte((byte)decryptedBlock);
                }
            }

            byte[] unpaddedData = RemovePadding(File.ReadAllBytes(outputFile));
            File.WriteAllBytes(outputFile, unpaddedData);
        }
    }
}
