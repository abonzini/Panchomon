using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{

    /// <summary>
    /// Contains all the data of a trainers in battle
    /// </summary>
    public class TrainerInstance : SimulationElement
    {
        public TrainerEntity AssociatedEntity = null; /// Needs to have an associated entity for name, pic, etc
        public Side Team = Side.FIELD; /// Which team they belong and which Side the spawn is. Team 0 is for chaotic agents, some attacks and neutral players (if team 0 wins, it's a tie)
        public int MaxMonsInField = GameplayElementsContainer.GameplayElementsContainer.DEFAULT_BATTLE_NMONS; /// How many mons at the same team they can have (Normally 3 in 3v3 explorations but tag trainers may do 1, in some cases even more than 3, starting base needs to be adaptable)
        public List<PokemonInstance> PokemonInTeam = new List<PokemonInstance>(); /// The mons they have, may be KOd
        public bool AutoDistributeMons = true; /// Mons are distributed in a balanced manner on a starting area unless disabled (in case of Unowns...)
        public override string Name { get => AssociatedEntity.Name; set => base.Name = value; } // Use associated entity's name
        public override string ToString()
        {
            return Name;
        }
    }
}
