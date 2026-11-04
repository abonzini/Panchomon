using Gameplay.GameEngine;
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
            Console.WriteLine($"Where to fetch battlefield(s) from? either 'fields' or: {string.Join(", ", [.. GameplayElementsContainer.GlobalData.Dungeons.Values.Select(d => d.Name)])}");
            string source = Console.ReadLine();
            List<Battlefield> validBattlefields = GetValidBattlefields(source, dirPath);
            Console.WriteLine($"Which one? {string.Join(", ", validBattlefields.Select(b => b.Name))}");
            string battlefield = Console.ReadLine();
            Console.WriteLine($"Any tags for construction? (use CSV if needed)");
            string tags = Console.ReadLine();
            return $"{source},{battlefield},{tags}";
        }
        public static TerrainGenerator GetTerrainGenerator(string battlefieldIdString, string dirPath)
        {
            string[] csv = battlefieldIdString.Split(",");
            string source = csv[0];
            string battlefield = csv[1];
            HashSet<string> tags = [];
            for (int i = 2; i < csv.Length; i++)
            {
                tags.Add(csv[i].Trim());
            }
            List<Battlefield> validBattlefields = GetValidBattlefields(source, dirPath);
            TerrainGenerator gen = validBattlefields.Where(b => b.Name == battlefield).FirstOrDefault().TerrainGenerator;
            gen.TagsToApply = tags;
            return gen;
        }
        static List<Battlefield> GetValidBattlefields(string source, string dirPath)
        {
            if (source == "fields")
            {
                return JsonConvert.DeserializeObject<List<Battlefield>>(File.ReadAllText(Path.Combine(dirPath, "fields", "tourn_battlefields.json")));
            }
            else
            {
                return GameplayElementsContainer.GlobalData.Dungeons[source].PossibleBattlefields;
            }
        }
    }
}
