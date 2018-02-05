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

        public static Location IntersectionPoint(Location enemyLoc, Location myLoc, Location destLoc, int S1, int S2)
        {
            int Xa = enemyLoc.Col, Ya = enemyLoc.Row,
            Xb = destLoc.Col, Yb = destLoc.Row,
            Xc = myLoc.Col, Yc = myLoc.Row;
            double a = ((Xb-Xa).Power(2)+(Yb-Ya).Power(2))/S2.Power(2)-(((Xb-Xa).Power(2)+(Yb-Ya).Power(2))/S1.Power(2));
            double b = 2*((Xb-Xa)*(Xa-Xc)+(Yb-Ya)*(Ya-Yc))/S2.Power(2);
            double c = ((Xa-Xc).Power(2)+(Ya-Yc).Power(2))/S2.Power(2);
            double T1 = -b+(b.Power(2)-4*a*c).Sqrt();
            double T2 = -b-(b.Power(2)-4*a*c).Sqrt();
            if(T1<=1&&T1>=0)
                return new Location((int)(Ya+T1*(Yb-Ya)),(int)(Xa+T1*(Xb-Xa)));
            else if(T2<=1&&T2>=0)
                return new Location((int)(Ya+T2*(Yb-Ya)),(int)(Xa+T2*(Xb-Xa)));
            return null;
        }
    }

}