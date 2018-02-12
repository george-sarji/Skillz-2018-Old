using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class Priorities : InitializationBot
    {
        public static void GenerateGeneralPriority()
        {
            foreach (MapObject mapobject in GeneralPriority.Keys)
            {
                GeneratePriority(mapobject);
            }
        }
        public static void GeneratePriority(MapObject mapObject)
        {
            int Priority = 0;
            Priority += NumberOfEnemies(mapObject);
            GeneralPriority[mapObject] = Priority;
        }

        public static int StepsScaled(int Distance, int speed)
        {
            int steps = Distance / speed;
            double scale = ((double)(game.Cols.Power(2) + game.Rows.Power(2))).Sqrt();
            return (int)(scale / steps);
        }

        public static int NumberOfEnemies(MapObject mapObject)  // Returns number of enemies in range of a mapobject fix it
        {
            if (mapObject is Mothership)
            {
                return game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Mothership)mapObject).UnloadRange)));  // Returns the number of enemies on a mothership
            }
            else if (mapObject is Pirate)
            {
                int number = game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Pirate)mapObject).PushRange)));  // Returns the number of enemies in range of a pirate
                if (((Pirate)mapObject).Owner == game.GetMyself())
                    number--;
                return number;
            }
            else if (mapObject is Capsule)
            {
                return game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Capsule)mapObject).PickupRange)));  // Returns the number of enemies in the pickup range of a capsule
            }
            return 0;
        }

        public static int GetWormholeLocationScore(Location wormhole, Location partner, Pirate pirate)
        {
            int score = 0;
            Mothership bestMothership = myMotherships//Closest Mothership to wormhole
                                .OrderBy(mothership => mothership.Distance(wormhole))
                                .FirstOrDefault();
            Capsule bestCapsule = myCapsules//Closest Capsule to 
                                .OrderBy(capsule => capsule.Distance(partner))
                                .FirstOrDefault();
            if (bestMothership != null)
                score += StepsScaled(bestMothership.Distance(wormhole), pirate.MaxSpeed);
            if (bestCapsule != null)
                score += StepsScaled(bestCapsule.Distance(partner), pirate.MaxSpeed);

            return score;


        }

        public static Wormhole GetBestWormhole(Pirate pirate)
        {
            Dictionary<Wormhole, int> wormholesScore = new Dictionary<Wormhole, int>();
            foreach (var wormhole in allWormholes)
            {
                wormholesScore.Add(wormhole, GetWormholeLocationScore(wormhole.Location, wormhole.Partner.Location, pirate));//Add all wormholes with their Scores according to the pirate
            }
            return wormholesScore.OrderBy(x => x.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
                    .FirstOrDefault().Key;//Order the wormholes by the score the lowest score is the best wormhole
        }

        public static Location GetPushLocation(Wormhole wormhole, Pirate pirate)
        {
            // Checks if the wormhole can be pushed to a better location, and if is it returns the new location.
            List<Location> candidates = new List<Location>();
            var bestOption = wormhole.GetLocation();
            const int steps = 24;
            for (int i = 0; i < steps; i++)
            {
                double angle = System.Math.PI * 2 * i / steps;
                double deltaX = pirate.PushDistance * System.Math.Cos(angle);
                double deltaY = pirate.PushDistance * System.Math.Sin(angle);
                Location option1 = new Location((int)(wormhole.Location.Row - deltaY), (int)(wormhole.Location.Col + deltaX));
                //InitializationBot.game.Debug(option);
                //InitializationBot.game.Debug(WormholeLocationScore(option, wormhole.Partner.Location));
                candidates.Add(option1);

            }
            if (candidates.Any())
            {//InitializationBot.game.Debug(WormholeLocationScore(candidates.OrderBy(option => WormholeLocationScore(option, wormhole.Partner.Location)).FirstOrDefault(),wormhole.Partner.Location));
                return candidates.OrderBy(option => GetWormholeLocationScore(option, wormhole.Partner.Location, pirate)).FirstOrDefault();
            }
            return wormhole.Location;
        }
    }
}