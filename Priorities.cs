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

        public static int StepsScaled(int steps)
        {
            double scale = ((double)(game.Cols.Power(2)+game.Rows.Power(2))).Sqrt();
            return (int)(steps*scale*9.0);
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
    }
}