using Gameplay.GameEngine;
using Gameplay.GameplayElements;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        /// <summary>
        /// All the stats
        /// </summary>
        enum MonStat
        {
            HP = 0,
            ATK = 1,
            DEF = 2,
            SPATK = 3,
            SPDEF = 4,
            SPEED = 5
        }
        /// <summary>
        /// Gets the index of a stat
        /// </summary>
        /// <param name="stat">Which stat</param>
        /// <returns>The index to help find elements in array</returns>
        int GetStatIndex(MonStat stat)
        {
            return (int)stat;
        }
        /// <summary>
        /// Gets the stat of a mon instance, gets mults and whatnot
        /// </summary>
        /// <param name="mon">Which mon</param>
        /// <param name="which">Which Stat</param>
        /// <returns>The mon's stat</returns>
        double GetMonStat(PokemonInstance mon, MonStat which)
        {
            int idx = GetStatIndex(which);
            PokemonSpecies speciesToCheck = mon.SpeciesOverride ?? mon.AssociatedEntity.Species; // Get the override unless the mon is just normal base
            double baseStat = speciesToCheck.Stats[idx];
            double statMult = mon.FixedStatMult[idx];
            // Todo other potential mults here due to status etc
            double result = which switch
            {
                MonStat.HP => 2 * baseStat + 110, // This is so stupid
                MonStat.ATK => baseStat, // Damage calc ones are just the normal ones
                MonStat.DEF => baseStat,
                MonStat.SPATK => baseStat,
                MonStat.SPDEF => baseStat,
                MonStat.SPEED => baseStat, // Speed is also the normal one because it means nothing and I can use it as damage calc in future or just operate with base
                _ => throw new Exception("Not a valid stat")
            };
            return result * statMult; // Multiply by mult and return
        }
    }
}
