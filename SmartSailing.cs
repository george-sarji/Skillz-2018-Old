using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class SmartSailing : InitializationBot
    {
        public static Location SmartSail(Pirate pirate, MapObject destination)
        {
            // List<Location> best = new List<Location>();//Location Available for sailing who contain a safe route
            // best.Add(pirate.Location);
            // for (int i = 1; i <= 24; i++)
            // {
            //     int alpha = (int)((2*System.Math.PI * i) / 24);//turn from radians to degrees
            //     Location current = new Location((int)(pirate.Location.Row - pirate.MaxSpeed * System.Math.Sin(alpha))
            //     , (int)(pirate.Location.Col + pirate.MaxSpeed * System.Math.Cos(alpha)));
            //     if (!current.InMap())//check if in map
            //         continue;
            //     if(!IsHittingAsteroid(current) && !IsInRangeOfEnemy(current))//check if it is a safe route
            //         best.Add(current);

            // }
            //  return best.OrderBy(location => location.Distance(destination)).FirstOrDefault();// get the closest and safest route
            Location best = pirate.Location;
            int col = 0, row = 0;
            for (row = pirate.GetLocation().Row - pirate.MaxSpeed; row < pirate.GetLocation().Row + pirate.MaxSpeed; row += 5)// go over a couple of location and get the best
            {
                for (col = pirate.GetLocation().Col - pirate.MaxSpeed; col < pirate.GetLocation().Col + pirate.MaxSpeed; col += 5)
                {
                    Location current = new Location(row, col);
                    if (!current.InMap())
                        continue;
                    if (current.Distance(destination) < pirate.Distance(destination) && current.Distance(destination) >= pirate.Distance(destination) - pirate.MaxSpeed && !IsInDanger(current))
                    {
                        if ((best.Distance(destination) > current.Distance(destination)) || (best == pirate.Location))
                            best = current;
                    }
                }
            }
            return best;
            
           
        }

        public static bool IsInDanger(Location loc)
        {
            return IsHittingAsteroid(loc)&&IsInRangeOfEnemy(loc)&&IsInWormHoleRange(loc);
        }

        public static bool IsInWormHoleRange(Location loc)
        {
            return InitializationBot.activeWormholes.
            Where(wormhole => wormhole.InRange(loc,wormhole.WormholeRange)).ToList()
            .Count>0;
        }

        public static bool IsHittingAsteroid(Location loc)
        {
            bool hitting = false;
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                if (loc.InRange(asteroid.Location.Add(asteroid.Direction), asteroid.Size))
                    hitting = true;
            }
            return hitting;
        }
        public static bool IsInRangeOfEnemy(Location loc)
        {
            int count = 0;
            foreach (Pirate pirate in game.GetEnemyLivingPirates())
            {
                if (pirate.InRange(loc, pirate.PushRange + pirate.MaxSpeed)&& pirate.PushReloadTurns<GameExtension.Steps(pirate,loc))
                {
                    count++;
                }
            }
            return count >= game.NumPushesForCapsuleLoss;
        }
    }
}