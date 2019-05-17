using System;
using System.Collections.Generic;

namespace PEA2
{
    public class Tabu
    {
        private readonly int[] bestSolv;
        private readonly int dimension;
        private readonly bool diversification;
        private readonly int frame;
        private readonly int max;
        private readonly int min;
        private readonly int neighMode;
        private readonly int noBetterNeighbourFrame;
        private readonly int plus;
        private readonly int[,] tab;
        private readonly int tabuLen;
        private readonly List<(int, int, int)> tabuList = new List<(int, int, int)>();
        private int bestVal;
        private bool betterSolvFound;
        private DateTime maxTime;
        private int noBetterSolvFound;

        public Tabu(int[,] mat, int noBetSolvFrame, int plus_, int frame_, int tabuLen, int neighMode) : this(mat,
            plus_, frame_, tabuLen, neighMode)
        {
            noBetterNeighbourFrame = noBetSolvFrame;
            diversification = true;
        }

        public Tabu(int[,] mat, int plus_, int frame_, int _tabuLen, int neighMode_)
        {
            tab = mat;
            dimension = tab.GetLength(0);
            GenerateSolution(ref bestSolv);
            bestVal = CalculateVal(bestSolv);
            plus = plus_;
            frame = frame_;
            min = frame / 2;
            max = frame;
            maxTime = DateTime.Now.AddSeconds(180.0);
            tabuLen = _tabuLen;
            neighMode = neighMode_;
        }

        public void DoAlgorithm()
        {
            var rand = new Random();
            var neighbour = new int[dimension];
            var bestNeighbour = new int[dimension];
            var neighbourUnderTest = new int[dimension];
            var bestMove = (0, 0, dimension * 3);
            bestSolv.CopyTo(bestNeighbour, 0);
            var bestNeighbourVal = CalculateVal(bestNeighbour);
            TimeSpan timer;
            do
            {
                timer = maxTime - DateTime.Now;
                if ((int) timer.TotalSeconds == 120 || (int) timer.TotalSeconds == 60)
                {
                    Console.WriteLine(bestVal);
                    for (var i = 0; i < dimension; i++)
                        Console.Write(bestSolv[i] + " ");
                    Console.WriteLine();
                    maxTime = maxTime.AddSeconds(-1);
                }

                bestNeighbourVal = CalculateVal(bestNeighbour);
                bestNeighbour.CopyTo(neighbourUnderTest, 0);
                for (var n = 0; n < frame; n++)
                {
                    int j;
                    var i = rand.Next(dimension);
                    while (true)
                    {
                        j = rand.Next(dimension);
                        if (i != j)
                            break;
                    }

                    neighbour = neighMode == 0 ? Swap(i, j, neighbourUnderTest) :
                        neighMode == 1 ? Insert(i, j, neighbourUnderTest) : Invert(i, j, neighbourUnderTest);
                    var val = CalculateVal(neighbour);
                    if (CheckTabuList(i, j))
                    {
                        if (val < 1.3 * bestVal)
                        {
                            neighbour.CopyTo(bestNeighbour, 0);
                            bestNeighbourVal = val;
                            bestMove.Item1 = i;
                            bestMove.Item2 = j;
                            betterSolvFound = true;
                            if (n + plus < min)
                                n = frame - (min - n);
                            else if (n + plus > max)
                                break;
                            else
                                n = frame - plus;
                        }
                    }
                    else if (val < bestNeighbourVal)
                    {
                        neighbour.CopyTo(bestNeighbour, 0);
                        bestNeighbourVal = val;
                        bestMove.Item1 = i;
                        bestMove.Item2 = j;
                    }
                }

                if (bestNeighbourVal < bestVal)
                {
                    bestNeighbour.CopyTo(bestSolv, 0);
                    bestVal = bestNeighbourVal;
                    betterSolvFound = true;
                    noBetterSolvFound = 0;
                    Console.WriteLine(bestVal);
                    for (var i = 0; i < dimension; i++)
                        Console.Write(bestSolv[i] + " ");
                    Console.WriteLine();
                }

                for (var i = 0; i < tabuList.Count; i++)
                {
                    tabuList[i] = (tabuList[i].Item1, tabuList[i].Item2, tabuList[i].Item3 - 1);
                    if (tabuList[i].Item3 == 0)
                        tabuList.RemoveAt(i);
                }

                tabuList.Add(bestMove);
                if (tabuList.Count > tabuLen)
                    tabuList.RemoveAt(0);
                if (betterSolvFound == false && diversification)
                {
                    noBetterSolvFound++;
                    if (noBetterSolvFound > noBetterNeighbourFrame)
                    {
                        noBetterSolvFound = 0;
                        var temp = new int[dimension];
                        GenerateSolution(ref temp);
                        temp.CopyTo(bestNeighbour, 0);
                        var tempVal = CalculateVal(temp);
                        for (var i = 0; i < 10; i++)
                        {
                            GenerateSolution(ref temp);
                            var nextVal = CalculateVal(temp);
                            if (nextVal < tempVal)
                            {
                                temp.CopyTo(bestNeighbour, 0);
                                tempVal = nextVal;
                            }
                        }
                    }
                }

                betterSolvFound = false;
            } while (timer.TotalSeconds > 0);

            Console.WriteLine(bestVal);
            for (var i = 0; i < dimension; i++)
                Console.Write(bestSolv[i] + " ");
            Console.WriteLine();
            Console.WriteLine("-------------------");
        }

        private int CalculateVal(int[] currSolv)
        {
            var sum = 0;
            for (var i = 0; i < dimension; i++)
                if (i == 0)
                    sum += tab[currSolv[0], currSolv[i + 1]];
                else if (i == dimension - 1)
                    sum += tab[currSolv[i], currSolv[0]];
                else
                    sum += tab[currSolv[i], currSolv[i + 1]];
            return sum;
        }

        private bool CheckTabuList(int i, int j)
        {
            for (var k = 0; k < tabuList.Count; k++)
                if (tabuList[k].Item1 == i && tabuList[k].Item2 == j ||
                    tabuList[k].Item1 == j && tabuList[k].Item2 == i)
                    return true;
            return false;
        }

        private void RefSwap(int i, int j, int[] tabToSwap)
        {
            var x = tabToSwap[i];
            tabToSwap[i] = tabToSwap[j];
            tabToSwap[j] = x;
        }

        private int[] Swap(int i, int j, int[] tabToSwap)
        {
            var temp = new int[dimension];
            tabToSwap.CopyTo(temp, 0);
            var x = temp[i];
            temp[i] = temp[j];
            temp[j] = x;
            return temp;
        }

        private int[] Invert(int i, int j, int[] tabToInvert)
        {
            var temp = new int[dimension];
            tabToInvert.CopyTo(temp, 0);
            if (j < i)
            {
                var t = i;
                i = j;
                j = t;
            }

            var max = j - i;
            for (var it = 0; it <= max / 2; it++)
                RefSwap(i + it, j - it, temp);
            return temp;
        }

        private int[] Insert(int i, int j, int[] tabToInsert)
        {
            var temp = new int[dimension];
            tabToInsert.CopyTo(temp, 0);
            if (i > j)
                for (var k = j; k < i; k++)
                    RefSwap(k, k + 1, temp);
            else
                for (var k = j; k > i; k--)
                    RefSwap(k, k - 1, temp);
            return temp;
        }

        private void GenerateSolution(ref int[] tab)
        {
            var rand = new Random();
            //tab = new int[dimension];
            for (var i = 0; i < dimension; i++) tab[i] = -1;
            for (var i = 0; i < dimension; i++)
                while (true)
                {
                    var pos = rand.Next(dimension);
                    if (tab[pos] == -1)
                    {
                        tab[pos] = i;
                        break;
                    }
                }
        }
    }
}