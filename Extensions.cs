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
            return pirate.Distance(mapObject) / pirate.MaxSpeed + 1;
        }

        public static int Clamp(this int num, int min, int max)
        {
            if (num < min)
                return min;
            if (num > max)
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
            if (InitializationBot.Debug)
                InitializationBot.game.Debug(s);
        }

        public static Location IntersectionPoint(Location enemyLoc, Location myLoc, Location destLoc, int S1, int S2)
        {
            int Xa = enemyLoc.Col, Ya = enemyLoc.Row,
            Xb = destLoc.Col, Yb = destLoc.Row,
            Xc = myLoc.Col, Yc = myLoc.Row;
            double a = ((Xb - Xa).Power(2) + (Yb - Ya).Power(2)) / S2.Power(2) - (((Xb - Xa).Power(2) + (Yb - Ya).Power(2)) / S1.Power(2));
            double b = 2 * ((Xb - Xa) * (Xa - Xc) + (Yb - Ya) * (Ya - Yc)) / S2.Power(2);
            double c = ((Xa - Xc).Power(2) + (Ya - Yc).Power(2)) / S2.Power(2);
            double T1 = -b + (b.Power(2) - 4 * a * c).Sqrt();
            double T2 = -b - (b.Power(2) - 4 * a * c).Sqrt();
            if (T1 <= 1 && T1 >= 0)
                return new Location((int)(Ya + T1 * (Yb - Ya)), (int)(Xa + T1 * (Xb - Xa)));
            else if (T2 <= 1 && T2 >= 0)
                return new Location((int)(Ya + T2 * (Yb - Ya)), (int)(Xa + T2 * (Xb - Xa)));
            return null;
        }

        // Given an enemy pirate's location a, and destination b, and a friendly pirate's location c,
        // calculates and returns the optimal point d for the friendly pirate to intercept the path of the enemy pirate.
        public static Location Interception(Location a, Location b, Location c)
        {
            var game = InitializationBot.game;
            int numerator = (c.Row - a.Row).Power(2) + (c.Col - a.Col).Power(2);
            int denominator = 2 * ((b.Row - a.Row) * (c.Row - a.Row) + (b.Col - a.Col) * (c.Col - a.Col));
            double s = denominator == 0 ? 0 : (double)numerator / denominator;
            var d = new Location(a.Row + (int)(s * (b.Row - a.Row)), a.Col + (int)(s * (b.Col - a.Col)));
            if (IsOnTheWay(a, b, c, game.PushRange) || c.Distance(d) < game.PirateMaxSpeed)
            {
                return a.Towards(b, game.PirateMaxSpeed);
            }
            return d;
        }
        // Returns true if the point c is within buffer distance to the line from a and b.
        private static bool IsOnTheWay(Location a, Location b, Location c, int buffer)
        {
            return b.Distance(c) <= a.Distance(c) && DistanceLP(a, b, c) <= buffer;
        }

        private static int DistanceLP(Location a, Location b, Location c)
        {
            int numerator = System.Math.Abs((b.Col - a.Col) * c.Row - (b.Row - a.Row) * c.Col + b.Row * a.Col - b.Col * a.Row);
            double denominator = a.Distance(b);
            return denominator == 0 ? 0 : (int)System.Math.Round(numerator / denominator);
        }

        public static int NumberOfAvailableEnemyPushers(Pirate pirate)
        {
            return InitializationBot.game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(pirate)).Count();
        }

        public static Location MidPoint(Pirate pirate1, Pirate pirate2)
        {
            int x1 = pirate1.Location.Col, x2 = pirate2.Location.Col;
            int y1 = pirate1.Location.Row, y2 = pirate2.Location.Row;
            return new Location((y1 + y2) / 2, (x1 + x2) / 2);
        }

        private static int DistanceThroughWormhole(Location from, MapObject to, Wormhole wormhole, IEnumerable<Wormhole> wormholes)
        {
            return from.Distance(wormhole) +
                   ClosestDistance(wormhole.Partner.Location, to,
                                   wormholes.Where(w => w.Id != wormhole.Id && w.Id != wormhole.Partner.Id));
        }

        private static int ClosestDistance(Location from, MapObject to, IEnumerable<Wormhole> wormholes)
        {
            if (wormholes.Any())
            {
                int distanceWithoutWormholes = from.Distance(to);
                int distanceWithWormholes = wormholes
                    .Select(wormhole => DistanceThroughWormhole(from, to, wormhole, wormholes))
                    .Min();
                return System.Math.Min(distanceWithoutWormholes, distanceWithWormholes);
            }
            return from.Distance(to);
        }


        public static Wormhole GetBestWormhole(IEnumerable<Wormhole> wormholes, Location destination, Pirate pirate)
        {
            var wormholeDistances = new Dictionary<Wormhole, int>();
            foreach (var wormhole in wormholes)
            {
                //    Assign the closest distance for the wormhole
                wormholeDistances.Add(wormhole, DistanceThroughWormhole(pirate.Location, destination, wormhole, wormholes));
            }
            //    Get the minimum
            var bestWormhole = wormholeDistances.OrderBy(map => map.Value).FirstOrDefault();
            if (bestWormhole.Key != null)
            {
                // Check the regular distance.
                var normalDistance = pirate.Distance(destination);
                if (bestWormhole.Value < normalDistance)
                    return bestWormhole.Key;
            }
            return null;
        }

        public static int WormholeLocationScore(Location location, Location partner, Pirate pirate)
        {
            // Evaluates how good a Wormhole's location is by its distance from the best mothership and capsule from ours and the enemy's.
            // The lower the score that gets reported, the better. Use to compare before and after pushing results.
            // int enemyScore = 0, myScore = 0;
            // var bestEnemyMothership = InitializationBot.game.GetEnemyMotherships()
            // .OrderBy(enemy => enemy.Distance(location) * Sqrt(enemy.ValueMultiplier)).FirstOrDefault();
            // if (bestEnemyMothership != null)
            // {
            //     if (location.Distance(bestEnemyMothership) < partner.Distance(bestEnemyMothership))
            //         enemyScore = (int)(bestEnemyMothership.Distance(location) * Sqrt(bestEnemyMothership.ValueMultiplier));
            //     else
            //     {
            //         var bestEnemyCapsule = InitializationBot.game.GetEnemyCapsules()
            //         .OrderBy(capsule => capsule.InitialLocation.Distance(location)).FirstOrDefault();
            //         enemyScore = bestEnemyCapsule.Distance(location);
            //     }
            // }
            // else enemyScore = 0;
            // var bestMothership = InitializationBot.game.GetMyMotherships()
            // .OrderBy(mothership => mothership.Distance(location) * Sqrt(mothership.ValueMultiplier)).FirstOrDefault();
            // if (bestMothership != null)
            // {
            //     if (location.Distance(bestMothership) < partner.Distance(bestMothership))
            //         myScore = (int)(bestMothership.Distance(location) * Sqrt(bestMothership.ValueMultiplier));
            //     else
            //     {
            //         var bestCapsule = InitializationBot.game.GetMyCapsules()
            //         .OrderBy(capsule => capsule.InitialLocation.Distance(location)).FirstOrDefault();
            //         myScore = bestCapsule.Distance(location);
            //     }
            // }
            // else myScore = 0;
            // return myScore - enemyScore;
            int score=0;
            var bestMothership = InitializationBot.game
                                .GetMyMotherships()
                                .OrderBy(mothership => mothership.Distance(location))
                                .FirstOrDefault();
            return score;
            

        }

        public static Location WorthPushingWormhole(Wormhole wormhole, Pirate pirate)
        {
            // Checks if the wormhole can be pushed to a better location, and if is it returns the new location.
            if (wormhole == null) return null;
            List<Location> candidates = new List<Location>();
            var bestOption = wormhole.GetLocation();
            const int steps = 24;
            for (int i = 0; i < steps; i++)
            {
                double angle = System.Math.PI * 2 * i / steps;
                double deltaX = InitializationBot.game.PushRange * System.Math.Cos(angle);
                double deltaY = InitializationBot.game.PushRange * System.Math.Sin(angle);
                Location option1 = new Location((int)(wormhole.Location.Row - deltaY), (int)(wormhole.Location.Col + deltaX));
                Location option2 = new Location((int)(wormhole.Location.Row - (deltaY / 2)), (int)(wormhole.Location.Col + (deltaX / 2)));
                //InitializationBot.game.Debug(option);
                //InitializationBot.game.Debug(WormholeLocationScore(option, wormhole.Partner.Location));
                if (wormhole != null && WormholeLocationScore(option1, wormhole.Partner.Location, pirate) < WormholeLocationScore(wormhole.Location, wormhole.Partner.Location, pirate) - 150)
                {
                    candidates.Add(option1);
                }
                if (wormhole != null && WormholeLocationScore(option2, wormhole.Partner.Location, pirate) < WormholeLocationScore(wormhole.Location, wormhole.Partner.Location, pirate) - 150)
                {
                    candidates.Add(option2);
                }
            }
            if (candidates.Any())
            {//InitializationBot.game.Debug(WormholeLocationScore(candidates.OrderBy(option => WormholeLocationScore(option, wormhole.Partner.Location)).FirstOrDefault(),wormhole.Partner.Location));
                return candidates.OrderBy(option => WormholeLocationScore(option, wormhole.Partner.Location, pirate)).FirstOrDefault();
            }
            return wormhole.Location;
        }


    }

}