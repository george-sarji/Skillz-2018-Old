using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Bot
{
    public static class GameExtension
    {
        public static int Steps(this Pirate pirate, MapObject mapObject)
        {
            return pirate.Distance(mapObject) / pirate.MaxSpeed+1;
        }

        public static int Clamp(this int num, int min, int max)
        {
            if(num <min)
                return min;
            if(num>max)
                return max;
            return num;
        }

        public static double Sqrt(this double num)
        {
            return System.Math.Sqrt(num);
        }

        public static int Max(params int[] numbers)
        {
            return numbers.OrderByDescending(num => num).First();
        }

        public static int Min(params int[] numbers)
        {
            return numbers.OrderBy(num => num).First();
        }

        public static int Power(this int num, int power)
        {
            return (int)System.Math.Pow(num, power);
        }

        public static double Power(this double num, int power)
        {
            return System.Math.Pow(num, power);
        }

        public static void Print(this string s)
        {
            if(InitializationBot.Debug)
                InitializationBot.game.Debug(s);
        }
    }

}