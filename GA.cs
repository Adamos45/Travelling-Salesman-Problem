using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PEA2
{
    internal class Ga
    {
        private readonly List<int[]> _parents;
        private readonly int _popSize;
        private readonly Random _rand = new Random();
        private readonly int[,] _tab;
        private readonly int _tabSideLen;
        private List<int[]> _population;
        private DateTime maxTime;
        private readonly double _parPer;
        private readonly double _tourPer;

        public Ga(int[,] tab, int popSize, double parPer, double tourPer)
        {
            _tab = tab;
            _popSize = popSize;
            _tabSideLen = tab.GetLength(0);
            _population = new List<int[]>(popSize);
            _parents = new List<int[]>();
            for (var i = 0; i < popSize; i++)
                _population.Add(GenerateSolution());
            maxTime = DateTime.Now.AddSeconds(180.0);
            _parPer = parPer;
            _tourPer = tourPer;
        }

        private int[] GenerateSolution()
        {
            var t = new int[_tabSideLen];
            for (var i = 0; i < _tabSideLen; i++) t[i] = -1;
            for (var i = 0; i < _tabSideLen; i++)
                while (true)
                {
                    var pos = _rand.Next(_tabSideLen);
                    if (t[pos] != -1) continue;
                    t[pos] = i;
                    break;
                }

            return t;
        }

        private int CalcSolVal(int[] sol)
        {
            var sum = 0;
            for (var i = 0; i < _tabSideLen; i++)
                if (i == 0)
                    sum += _tab[sol[0], sol[i + 1]];
                else if (i == _tabSideLen - 1)
                    sum += _tab[sol[i], sol[0]];
                else
                    sum += _tab[sol[i], sol[i + 1]];
            return sum;
        }

        private void ChooseParents()
        {
            _parents.Clear();
            var howmuchparents = _parPer*_popSize;
            var tourSize = _tourPer*_popSize;
            if (tourSize > _popSize)
                tourSize = _popSize;
            for (var i = 0; i < howmuchparents; i++)
            {
                var k = tourSize;
                var tourPopulation = new ArrayList();
                var bestChoiceVal = int.MaxValue;
                var bestChoice = -1;
                for (var j = 0; j < k; j++)
                {
                    var choice = _rand.Next(_popSize - 1);
                    if (tourPopulation.Contains(choice))
                    {
                        j--;
                    }
                    else
                    {
                        tourPopulation.Add(choice);
                        var choiceVal = CalcSolVal(_population[choice]);
                        if (choiceVal >= bestChoiceVal) continue;
                        bestChoiceVal = choiceVal;
                        bestChoice = choice;
                    }
                }

                _parents.Add(_population[bestChoice]);
                tourPopulation.Clear();
            }
        }

        private void Pmx()
        {
            if (_parents.Count < 2) return;

            var switchList = new List<(int, int)>();
            var a = _rand.Next(_tabSideLen);
            var b = _rand.Next(_tabSideLen);
            if (a > b)
            {
                var temp = a;
                a = b;
                b = temp;
            }

            var par1 = _rand.Next(_parents.Count);
            var par2 = _rand.Next(_parents.Count);

            var parent1 = _parents[par1];
            var parent2 = _parents[par2];

            var child1 = new int[_tabSideLen];
            var child2 = new int[_tabSideLen];
            parent1.CopyTo(child1, 0);
            parent2.CopyTo(child2, 0);

            for (var i = a; i <= b; i++)
            {
                var pair = (parent1[i], parent2[i]);
                child1[i] = pair.Item2;
                child2[i] = pair.Item1;

                switchList.Add(pair);
            }

            var counter = a;
            var start = 0;
            for (var z = 0; z < 2; z++)
            {
                for (var i = start; i < counter; i++)
                {
                    bool changed;
                    do
                    {
                        changed = false;
                        foreach (var k in switchList)
                        {
                            if (k.Item2 != child1[i]) continue;
                            child1[i] = k.Item1;
                            changed = true;
                        }
                    } while (changed);

                    do
                    {
                        changed = false;
                        foreach (var k in switchList)
                        {
                            if (k.Item1 != child2[i]) continue;
                            child2[i] = k.Item2;
                            changed = true;
                        }
                    } while (changed);
                }

                start = b + 1;
                counter = _tabSideLen;
            }

            var mutation = _rand.Next(10);
            if (mutation > 5)
            {
                a = _rand.Next(_tabSideLen);
                b = _rand.Next(_tabSideLen);

                Invert(a, b, ref child1);
            }

            mutation = _rand.Next(10);
            if (mutation > 5)
            {
                a = _rand.Next(_tabSideLen - 1);
                b = _rand.Next(_tabSideLen - 1);

                Invert(a, b, ref child2);
            }

            _population.Add(child1);
            _population.Add(child2);
        }

        private static void Invert(int i, int j, ref int[] tabToInvert)
        {
            if (j < i)
            {
                var t = i;
                i = j;
                j = t;
            }

            var max = j - i;
            for (var it = 0; it <= max / 2; it++)
            {
                var x = tabToInvert[i + it];
                tabToInvert[i + it] = tabToInvert[j - it];
                tabToInvert[j - it] = x;
            }
        }

        private void NextGen()
        {
            _population = _population.OrderBy(CalcSolVal).ToList();
            _population.RemoveRange(_popSize, _population.Count - _popSize);
        }

        public void DoAlgorithm()
        {
            TimeSpan timer;
            DateTime waitTime =
                DateTime.Now.AddSeconds(0.1 * _popSize + 0.1 * _parPer * _popSize + 0.1 * _tourPer * _popSize);
            var best = int.MaxValue;
            int[] path = default;
            do
            {
                timer = maxTime - DateTime.Now;
                var letNewPeople = waitTime - DateTime.Now;
                if ((int)timer.TotalSeconds == 120 || (int)timer.TotalSeconds == 60)
                {
                    //Console.WriteLine(best);
                    //for (var i = 0; i < _tabSideLen; i++)
                    //    Console.Write(path[i] + " ");
                    Console.WriteLine(best);
                    maxTime = maxTime.AddSeconds(-1);
                }
                ChooseParents();
                Pmx();
                NextGen();
                var temp = CalcSolVal(_population[0]);
                if (temp < best)
                {
                    best = temp;
                    path = _population[0];
                    waitTime =
                        DateTime.Now.AddSeconds(0.1 * _popSize + 0.1 * _parPer * _popSize + 0.1 * _tourPer * _popSize);
                }
                
                if (letNewPeople.TotalSeconds<=0)
                {
                    for (var k = 0; k < _popSize / 1.2; k++)
                        _population[_rand.Next(_popSize - 1)] = GenerateSolution();
                    waitTime=waitTime.AddSeconds(0.1 * _popSize + 0.1 * _parPer * _popSize + 0.1 * _tourPer * _popSize);
                }
            } while (timer.TotalSeconds > 0);
            Console.WriteLine(best);
            Console.WriteLine();
        }
    }
}