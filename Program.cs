using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PEA2
{
    public class Program
    {
        private static void Main()
        {
            Console.WriteLine("data42");
            var percentage = new[] {0.2, 0.5, 0.8};
            var popsize = new[] {10, 100, 500};
            for (int par = 0; par < 3; par++)
            {
                Console.WriteLine("par: " + percentage[par]);
                for (int tour = 0; tour < 3; tour++)
                {
                    Console.WriteLine("tour: "+ percentage[tour]);
                    for (int i = 0; i < 3; i++)
                    {
                        Console.WriteLine("pop: "+popsize[i]);
                        var ga = new Ga(LoadFile("data42.txt"), popsize[i], percentage[par], percentage[tour]);
                        ga.DoAlgorithm();
                    }
                }
            }

            Console.ReadLine();
        }

        public static int[,] LoadFile(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open);

            var sr = new StreamReader(fs);
            sr.ReadLine();
            var x = sr.ReadLine();
            var rows = int.Parse(x ?? throw new InvalidOperationException());
            var matrix = new int[rows, rows];
            x = sr.ReadLine();
            for (var i = 0; i < rows; i++)
            {
                var nums = Regex.Replace(x ?? throw new InvalidOperationException(), " {2,}", " ").Trim().Split(' ');
                for (var j = 0; j < rows; j++) matrix[i, j] = int.Parse(nums[j]);
                x = sr.ReadLine();
            }

            sr.Close();
            return matrix;
        }
    }
}