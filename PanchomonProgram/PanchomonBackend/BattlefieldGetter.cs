using Gameplay.GameplayElements;
using Gameplay.GameplayElementsContainer;
using Newtonsoft.Json;

namespace PanchomonBackend
{
    public static class BattlefieldGetter
    {
        /// <summary>
        /// Generates a battlefield ID string to choose battlefield for a fight
        /// </summary>
        /// <param name="dirPath">Directory where to look for tourn json</param>
        /// <returns></returns>
        public static string GetBattlefieldIdString(string dirPath)
        {
            Console.WriteLine($"Where to fetch battlefield(s) from? either 'tournament' or: {string.Join(", ", [.. GameplayElementsContainer.GlobalData.Dungeons.Values.Select(d => d.Name)])}");
            string source = Console.ReadLine();
            List<Battlefield> validBattlefields = GetValidBattlefields(source, dirPath);
            Console.WriteLine($"Which one? {validBattlefields.Select(b => b.Name)}");
            string battlefield = Console.ReadLine();
            return $"{source},{battlefield}";
        }
        public static Battlefield GetBattlefield(string battlefieldIdString, string dirPath)
        {
            string source = battlefieldIdString.Split(",")[0];
            string battlefield = battlefieldIdString.Split(",")[1];
            List<Battlefield> validBattlefields = GetValidBattlefields(source, dirPath);
            return validBattlefields.Where(b => b.Name == battlefield).FirstOrDefault();
        }
        static List<Battlefield> GetValidBattlefields(string source, string dirPath)
        {
            if (source == "tournament")
            {
                return JsonConvert.DeserializeObject<List<Battlefield>>(File.ReadAllText(Path.Combine(dirPath, "tourn_battlefields.json")));
            }
            else
            {
                return GameplayElementsContainer.GlobalData.Dungeons[source].PossibleBattlefields;
            }
        }
    }
}
