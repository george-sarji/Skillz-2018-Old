using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace MyBot
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

        public static double Sqrt(this int num)
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

        public static double Power(this int num, int power)
        {
            return System.Math.Pow(num, power);
        }
    }

}