using System;
using System.Collections.Generic;
using System.Linq;

namespace MahJong
{
    internal class Program
    {
        private const int maxKan = 5;

        private static Dictionary<Tiles, int> mahjongs = new Dictionary<Tiles, int>();
        private static List<Tiles> eyes = new List<Tiles>();
        private static List<Tiles> pongs = new List<Tiles>();
        private static List<Tiles> chows = new List<Tiles>();
        private static Random rng = new Random();

        private static void Main(string[] args)
        {
            var s = string.Empty;
            do
            {
                Init();
                Console.WriteLine("目前牌型：");
                Tiles[] readyHands = GenerateReadyHands();
                Show(readyHands);
                Console.WriteLine("聽的牌型：");
                Tiles[] ans = GetWaitingTiles(readyHands);
                Show(ans);
                s = Console.ReadLine();
            } while (s != "1");
        }

        private static void Init()
        {
            mahjongs.Clear();
            eyes.Clear();
            pongs.Clear();
            chows.Clear();
            foreach (object v in Enum.GetValues(typeof(Tiles)))
            {
                Tiles t = (Tiles)v;
                mahjongs.Add(t, 4);
                eyes.Add(t);
                pongs.Add(t);
                switch (t)
                {
                    case Tiles.Wan8:
                    case Tiles.Wan9:
                    case Tiles.Dot8:
                    case Tiles.Dot9:
                    case Tiles.Bamboo8:
                    case Tiles.Bamboo9:
                    case Tiles.East:
                    case Tiles.South:
                    case Tiles.West:
                    case Tiles.North:
                    case Tiles.Red:
                    case Tiles.Green:
                    case Tiles.White:
                        break;

                    default:
                        chows.Add(t);
                        break;
                }
            }
        }

        private static void Show(Tiles[] tiles)
        {
            Dictionary<int, string> numberName = new Dictionary<int, string>()
            {
                { 1, "一" },{ 2, "二" },{ 3, "三" },{ 4, "四" },{ 5, "五" },{ 6, "六" },{ 7, "七" },{ 8, "八" },{ 9, "九" },
            };

            Dictionary<int, string> wordName = new Dictionary<int, string>()
            {
                { 1, "東" },{ 2, "南" },{ 3, "西" },{ 4, "北" },{ 5, "紅" },{ 6, "青" },{ 7, "白" }
            };
            for (int i = 0; i < tiles.Length; i++)
            {
                int t = (int)tiles[i];
                switch (t % 10)
                {
                    case 0:
                        Console.Write(wordName[t / 1000]);
                        break;

                    default:
                        Console.Write(numberName[t % 10]);
                        break;
                }
            }
            Console.WriteLine();
            for (int i = 0; i < tiles.Length; i++)
            {
                int t = (int)tiles[i];
                if (t < 10)
                    Console.Write("萬");
                else if (t < 100)
                    Console.Write("筒");
                else if (t < 1000)
                    Console.Write("條");
                else if (t < 5000)
                    Console.Write("風");
                else if (t == 5000)
                    Console.Write("中");
                else if (t == 6000)
                    Console.Write("發");
                else
                    Console.Write("板");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 產生手牌
        /// </summary>
        /// <returns></returns>
        private static Tiles[] GenerateReadyHands()
        {
            int curChow = rng.Next(0, maxKan);
            int curPong = maxKan - curChow;
            List<Tiles> hands = new List<Tiles>();
            int r = 0, mj = 0;
            while (curChow > 0)
            {
                r = rng.Next(0, chows.Count - 1);
                mj = (int)chows[r];
                for (int i = 0; i < 3; i++)
                {
                    Tiles tC = (Tiles)(mj + i);
                    hands.Add(tC);
                    mahjongs[tC]--;
                }
                curChow--;
                chows.RemoveAt(r);
            }

            foreach (var kvp in mahjongs)
            {
                if (kvp.Value < 3)
                    pongs.Remove(kvp.Key);
            }

            while (curPong > 0)
            {
                r = rng.Next(0, pongs.Count - 1);
                mj = (int)pongs[r];
                Tiles tP = (Tiles)(mj);

                for (int i = 0; i < 3; i++)
                    hands.Add(tP);

                mahjongs[tP] -= 3;
                curPong--;
                pongs.RemoveAt(r);
            }

            foreach (var kvp in mahjongs)
            {
                if (kvp.Value < 2)
                    eyes.Remove(kvp.Key);
            }

            r = rng.Next(0, eyes.Count - 1);
            mj = (int)eyes[r];
            Tiles tE = (Tiles)(mj);

            for (int i = 0; i < 2; i++)
                hands.Add(tE);

            mahjongs[tE] -= 2;
            eyes.RemoveAt(r);

            hands.Sort();
            r = rng.Next(0, hands.Count - 1);
            hands.RemoveAt(r);
            return hands.ToArray();
        }

        private static Tiles[] GetWaitingTiles(Tiles[] readyHands)
        {
            var remainTiles = mahjongs.Where(m => m.Value > 0).Select(m => m.Key);
            List<Tiles> tmp = new List<Tiles>();
            foreach (var rt in remainTiles)
            {
                List<Tiles> checkHands = new List<Tiles>() { rt };
                checkHands.AddRange(readyHands);
                checkHands.Sort();
                List<int> eyeIndex = new List<int>();
                for (int i = 0; i < checkHands.Count - 1; i++)
                {
                    if (checkHands[i] == checkHands[i + 1])
                    {
                        if (!eyeIndex.Any(e => checkHands[e] == checkHands[i]))
                            eyeIndex.Add(i);
                    }
                }

                foreach (var ei in eyeIndex)
                {
                    var orderValues = checkHands.Select(o => (int)o).ToList();
                    orderValues[ei] = orderValues[ei + 1] = 0;
                    if (orderValues.Sum() % 3 > 0) continue;
                    if (IsHu(orderValues))
                    {
                        tmp.Add(rt);
                        break;
                    }
                }
            }

            return tmp.Distinct().OrderBy(t => t).ToArray();
        }

        private static bool IsHu(List<int> remainValues)
        {
            for (int i = 0; i < remainValues.Count; i++)
            {
                bool hasKan = false;
                if (remainValues[i] == 0) continue;
                if (i + 2 < remainValues.Count)
                {
                    if (remainValues[i + 2] == remainValues[i])
                    {
                        remainValues[i] = remainValues[i + 1] = remainValues[i + 2] = 0;
                        hasKan = true;
                    }
                }
                if (!hasKan)
                {
                    var nextI = remainValues.FindIndex(fi => fi == remainValues[i] + 1);
                    var next2I = remainValues.FindIndex(fi => fi == remainValues[i] + 2);
                    if (nextI != -1 && next2I != -1)
                    {
                        remainValues[i] = remainValues[nextI] = remainValues[next2I] = 0;
                    }
                }
            }
            return !remainValues.Any(s => s > 0);
        }
    }
}