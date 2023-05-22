using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Содержит типы, позволяющие осуществлять чтение и запись в файлы и
                 // потоки данных, а также типы для базовой поддержки файлов и папок.

namespace CG_Lab_2
{
    class Bin
    {
        public static int X, Y, Z;
        public static short[] array; //short потому что мы будем использовать ReadInt16
        
        public Bin() { }

        public static void readBin(string path)
        {
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)); 
                //Считывает примитивные типы данных как двоичные значения в заданной кодировке.

                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                Z = reader.ReadInt32();

                int arraySize = X * Y * Z;
                array = new short[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    array[i] = reader.ReadInt16(); // массив хранения плотностей вокселей
                                                   // ReadInt16 - для хранения плотностей вокселей используется 2х байтовое число
                }
            }
        }
    }
}
