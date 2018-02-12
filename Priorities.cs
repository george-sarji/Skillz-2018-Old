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
            int Priority=0;
            Priority += NumberOfEnemies(mapObject);
            GeneralPriority[mapObject]=Priority;
        }

        public static int StepsScaled(int Distance, int speed)
        {
            int steps = Distance/speed;
            double scale = ((double)(game.Cols.Power(2)+game.Rows.Power(2))).Sqrt();
            return (int)(scale/steps);
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

        public static int WormholeLocationScore(Location wormhole,Location partner, Pirate pirate)
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
                                .OrderBy(mothership => mothership.Distance(wormhole))
                                .FirstOrDefault();
            var bestCapsule = InitializationBot.game
                                .GetMyCapsules()
                                .OrderBy(capsule => capsule.Distance(partner))
                                .FirstOrDefault();
            score += StepsScaled(bestMothership.Distance(wormhole),pirate.MaxSpeed);
            score += StepsScaled(bestCapsule.Distance(partner),pirate.MaxSpeed);
            
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
                double deltaX = pirate.PushDistance * System.Math.Cos(angle);
                double deltaY = pirate.PushDistance * System.Math.Sin(angle);
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