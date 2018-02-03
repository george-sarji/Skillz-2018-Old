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
            Location best = pirate.Location;
            for (int i = 0; i < 24; i++)
            {
                int alpha = (int)((System.Math.PI * i) / 24);
                Location current = new Location((int)(pirate.Location.Row - pirate.MaxSpeed * System.Math.Sin(alpha)), (int)(pirate.Location.Col + pirate.MaxSpeed * System.Math.Cos(alpha)));
                if (!current.InMap())
                    continue;
                if (current.Distance(destination) < pirate.Distance(destination) && current.Distance(destination) >= pirate.Distance(destination) - pirate.MaxSpeed && !IsInRange(current) && !IsHittingAsteroid(current))
                {
                    if ((best != pirate.Location && best.Distance(destination) > current.Distance(destination)) || (best == pirate.Location))
                        best = current;
                }
            }
            return best;
        }

        public static bool IsHittingAsteroid(Location loc)
        {
            bool hitting = false;
            foreach (Asteroid asteroid in game.GetLivingAsteroids())
            {
                if (loc.InRange(asteroid, asteroid.Size))
                    hitting = true;
            }
            return hitting;
        }
        public static bool IsInRange(Location loc)
        {
            int count = 0;
            foreach (Pirate pirate in game.GetEnemyLivingPirates())
            {
                if (pirate.InRange(loc, pirate.PushRange + pirate.MaxSpeed))
                {
                    count++;
                }
            }
            return count >= game.NumPushesForCapsuleLoss;
        }
    }
}