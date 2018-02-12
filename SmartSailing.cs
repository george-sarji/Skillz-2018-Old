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
            List<Location> candidates = new List<Location>();
            var bestOption = pirate.GetLocation();
            const int steps = 24;
            Location PirateLocation=pirate.GetLocation();
            if((pirate.Location.Distance(destination))-bestOption.Distance(destination)>=(pirate.MaxSpeed/2) && pirate.HasCapsule())
            {
                var LocationOfPush=TryPush.TryPushMyCapsule(pirate);
                if(PirateLocation != null)
                    PirateLocation=LocationOfPush;
            }
            for (int i = 0; i < steps; i++)
            {
                double angle = System.Math.PI * 2 * i / steps;
                double deltaX = pirate.MaxSpeed * System.Math.Cos(angle);
                double deltaY = pirate.MaxSpeed * System.Math.Sin(angle);
                Location option = new Location((int)(PirateLocation.Row - deltaY), (int)(PirateLocation.Col + deltaX));
                if (!IsInDanger(option, destination.GetLocation(), pirate) && option.InMap())
                {
                    candidates.Add(option);
                }

            }
            if (candidates.Any())
            {
                bestOption = candidates.OrderBy(option => option.Distance(destination)).First();
            }
            return bestOption;
        }

        public static bool IsInDanger(Location loc, Location destination, Pirate pirate)
        {
            return IsHittingAsteroid(loc)||IsInRangeOfEnemy(loc) || IsInWormholeDanger(loc, destination, pirate);
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

        public static bool IsInWormholeDanger(Location location, Location destination, Pirate pirate)
        {
            // Get the closest wormhole to the current locatio.
            var wormholes = InitializationBot.game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate<pirate.Steps(destination)/4).Where(wormhole => wormhole.InRange(location, wormhole.WormholeRange));
            var closestWormhole = wormholes.FirstOrDefault();
            if(closestWormhole==null)
                return false;
            else if(closestWormhole.Equals(GameExtension.GetBestWormhole(destination, pirate)))
                return false;
            return true;
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