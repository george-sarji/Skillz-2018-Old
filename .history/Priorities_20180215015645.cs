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
        public static int ScaleNumber(double num, int turns, double scale)
        {
            if (turns == 0)
                return (int)(num * 100 / scale);
            return (int)(num * 100 * turns / scale);
        }
        public static void GeneratePriority(MapObject mapObject)
        {
            int Priority = 0;
            Priority += NumberOfEnemies(mapObject);
            GeneralPriority[mapObject] = Priority;
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

        public static int GetWormholeLocationScore(Wormhole wormhole, Location wormholeLocation, Location partner, Pirate pirate, bool ConsiderPirate)
        {
            int score = 0,distance;
            Mothership bestMothership = myMotherships//Closest Mothership to wormhole
                                .OrderBy(mothership => mothership.Distance(wormholeLocation))
                                .FirstOrDefault();
            Capsule bestCapsule = myCapsules//Closest Capsule to 
                                .OrderBy(capsule => capsule.Distance(partner))
                                .FirstOrDefault();
            
            int distanceFromMothershipToCapsule = GameExtension.WormholePossibleLocationDistance(bestMothership.Location, bestCapsule.Location, wormholeLocation, NewWormholeLocation[wormhole.Partner]);
            bestMothership = myMotherships//Closest Mothership to wormhole
                                .OrderBy(mothership => mothership.Distance(partner))
                                .FirstOrDefault();
            bestCapsule = myCapsules//Closest Capsule to 
                                .OrderBy(capsule => capsule.Distance(wormhole))
                                .FirstOrDefault();
            int distanceFromCapsuleToMothership = GameExtension.WormholePossibleLocationDistance(bestCapsule.Location, bestMothership.Location, wormholeLocation, NewWormholeLocation[wormhole.Partner]);
            if(distanceFromCapsuleToMothership < distanceFromMothershipToCapsule)
                distance=distanceFromCapsuleToMothership;
            else
                distance=distanceFromMothershipToCapsule
            if (bestMothership != null && bestCapsule != null)
                score += ScaleNumber(distance, wormhole.TurnsToReactivate, scale);
            if (ConsiderPirate)
                score += ScaleNumber(pirate.Distance(wormholeLocation), wormhole.TurnsToReactivate, scale);
            score += ScaleNumber((double)NumOfAssignedPiratesToWormhole[wormhole], wormhole.TurnsToReactivate, 1);
            return score;
        }

        public static Wormhole GetBestWormhole(Pirate pirate)
        {
            Dictionary<Wormhole, int> wormholesScore = new Dictionary<Wormhole, int>();
            foreach (var wormhole in allWormholes)
            {
                wormholesScore.Add(wormhole, GetWormholeLocationScore(wormhole, wormhole.Location, NewWormholeLocation[wormhole], pirate, true));//Add all wormholes with their Scores according to the pirate
            }
            PrintWormhole(wormholesScore, pirate);
            return wormholesScore.OrderBy(x => x.Value)
                    .FirstOrDefault().Key;//Order the wormholes by the score the lowest score is the best wormhole
        }

        public static Location GetPushLocation(Wormhole wormhole, Pirate pirate)
        {
            // Checks if the wormhole can be pushed to a better location, and if is it returns the new location.
            List<Location> candidates = new List<Location>();
            candidates.Add(wormhole.GetLocation());
            const int steps = 24;
            for (int i = 0; i < steps; i++)
            {
                double angle = System.Math.PI * 2 * i / steps;
                double deltaX = pirate.PushDistance * System.Math.Sin(angle);
                double deltaY = pirate.PushDistance * System.Math.Cos(angle);
                Location option = wormhole.Location.Add(new Location(-(int)deltaX, (int)deltaY));
                if (!option.InMap())
                {
                    ("reached").Print();
                    continue;
                }
                candidates.Add(option);

            }
            var PushLocation = candidates
                                .OrderBy(option => GetWormholeLocationScore(wormhole, option, NewWormholeLocation[wormhole.Partner], pirate, false))
                                .FirstOrDefault();
            NewWormholeLocation[wormhole] = PushLocation;
            return PushLocation;
        }
    }
}