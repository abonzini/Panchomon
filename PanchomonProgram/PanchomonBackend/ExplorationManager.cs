using Gameplay.GameEngine;
using Gameplay.GameplayElements;
using Gameplay.GameplayElementsContainer;
using Utilities;

namespace PanchomonBackend
{
    class ExplorationPrizeList
    {
        public Dictionary<string, int> ItemPrizes = [];
        public Dictionary<int, Dictionary<string, int>> MonsFound = []; // Mons divided by rank
    }
    /// <summary>
    /// Contains all the dynamic ongoing dungeon things and useful functions
    /// </summary>
    class ExplorationContext
    {
        List<(Sandwich, int)> _sandwichEffects = [];
        public MessageLogger ExplorationMessage = new MessageLogger();
        public ExplorationPrizeList Prizes = new ExplorationPrizeList();
        public Dungeon DungeonData;
        public TrainerEntity TrainerEntity;
        public TrainerInstance TrainerInstance;
        public TrainerInstance FollowerInstance = null; // Follower if any
        public Random Rng;
        public int CurrentFloor;
        public HashSet<PokemonSpecies> EncounterTable = [];
        public List<ItemDrop> DropTable = [];
        /// <summary>
        /// Adds a sandwich effect to the current context
        /// </summary>
        /// <param name="sandwich"></param>
        public void AddSandwichEffect(Sandwich sandwich)
        {
            _sandwichEffects.Add((sandwich, sandwich.Duration));
        }
        // The sandwiches with more duration are better because they last for bigger battles so the variance needs to be retought and be more negative, shouldnt keep the mean as is, but instead move the mean around
        /// <summary>
        /// Does a random pull with the current game effects to try and get an extra pokemon
        /// </summary>
        /// <returns>How many extra pokemon are found in this encounter. Can be negative</returns>
        public int GetExtraWildPokemon()
        {
            // Lvl 5 will add 5 mons, etc. Result will be an int so need to round
            double extraMonCount = 0; // Will be double to stack many double effects
            List<Sandwich> validSandwichEffects = [.. _sandwichEffects.Where(s => s.Item1.Effect == SandwichEffectType.ENEMY_NUMBER).Select(s => s.Item1)]; // Get the valid sandwiches
            foreach (Sandwich sando in validSandwichEffects)
            {
                double max = sando.Level;
                double min = max - 0.5 * max * (sando.Duration - 1); // Reaches -MAX at duration 5
                double randomFloat = min + ((max - min) * Rng.NextDouble());
                extraMonCount += randomFloat;
            }
            return (int)Math.Round(extraMonCount); // Always round to closest integer
        }
        /// <summary>
        /// Does a random pull with how much will be the item multiplier
        /// </summary>
        /// <returns>Item multiplier</returns>
        public double GetItemMult()
        {
            // Similar but the additive nature is to a mult which will need to be rounded
            double itemMult = 1; // Base item multiplier
            List<Sandwich> validSandwichEffects = [.. _sandwichEffects.Where(s => s.Item1.Effect == SandwichEffectType.ITEM_DROP).Select(s => s.Item1)]; // Get the valid sandwiches
            foreach (Sandwich sando in validSandwichEffects)
            {
                double max = sando.Level;
                double min = max - 0.5 * max * (sando.Duration - 1); // Reaches -MAX at duration 5
                double randomFloat = min + ((max - min) * Rng.NextDouble());
                itemMult += randomFloat;
            }
            return itemMult;
        }
        /// <summary>
        /// Does a random pull with how much will be the extra healing in int percentage
        /// </summary>
        /// <returns>Percentage healing</returns>
        public double GetExtraHealing()
        {
            // This sando heals 5% to 25% at lvl 5. Max is always that and variance just opens amplitude below as the duration approaches 5. Mean value actually decreases but because healing a potential of 25% is the cumul part when duration is there
            double totalHealingPercentage = 0; // Will be double to stack many double effects
            List<Sandwich> validSandwichEffects = [.. _sandwichEffects.Where(s => s.Item1.Effect == SandwichEffectType.POST_HEALING).Select(s => s.Item1)]; // Get the valid sandwiches
            foreach (Sandwich sando in validSandwichEffects)
            {
                double max = 0.05 * sando.Level; // Max healing
                double min = max - 0.5 * max * (sando.Duration - 1); // Reaches -MAX at duration 5
                double randomFloat = min + ((max - min) * Rng.NextDouble());
                totalHealingPercentage += randomFloat;
            }
            return totalHealingPercentage;
        }
        /// <summary>
        /// Does a random pull with how much will be the shiny chance for a fight
        /// </summary>
        /// <returns>Shiny chance</returns>
        public int GetShinyChance()
        {
            // This one is kind of inverse. Being shiny chance of 1 in 500. A lvl 5 will be 50 times more likely (i.e. 1 in 10) and the others are all the way inbetween
            // This moves the mean, and more sandwiches then will jus oscilate similarly around the new mean
            double shinyChance = GameplayElementsContainer.SHINY_CHANCE; // Base shiny chance (1 in)
            List<Sandwich> validSandwichEffects = [.. _sandwichEffects.Where(s => s.Item1.Effect == SandwichEffectType.SHINY_CHANCE).Select(s => s.Item1)]; // Get the valid sandwiches
            if (validSandwichEffects.Count > 0)
            {
                foreach (Sandwich sando in validSandwichEffects)
                {
                    double bestCaseShinyMean = shinyChance / 50; // 50 times more likely is the hardcoded best chance
                    double shinyVariance = (bestCaseShinyMean - shinyChance) * (sando.Level - 1) / 4; // Meaning a lvl 5 will reach this and lvl 1 etc not so much
                    double min = bestCaseShinyMean - shinyVariance; // This is the good shit, better shiny chances
                    double max = bestCaseShinyMean + shinyVariance * (sando.Duration - 1) / 4;
                    // Kind of, I guess...
                    // So now I roll
                    shinyChance = min + ((max - min) * Rng.NextDouble()); // New mean, next rolls will be around here, may be insanely good or drift worse and worse (realistically it should usually be just 1 sandwich anyway)
                }
            }
            int resultingChance = (int)shinyChance; // Get the chance 1/X, round to closest integer
            if (resultingChance < 1) resultingChance = 1; // Chance needs to keep positive otherwise the roll goes to fuck
            return resultingChance;
        }
        /// <summary>
        /// Does a random pull of extra level gain
        /// </summary>
        /// <returns>The lvl gain for a mon</returns>
        public double GetExtraStatMult()
        {
            // Similarly to healing and stuff, each levels increases stats by 5% (so a mult of 1.05 to 1.25 but this mult should be additive i'd say?)
            double additionalStatMult = 1; // Will be double to stack many double effects
            List<Sandwich> validSandwichEffects = [.. _sandwichEffects.Where(s => s.Item1.Effect == SandwichEffectType.STAT_MULT).Select(s => s.Item1)]; // Get the valid sandwiches
            foreach (Sandwich sando in validSandwichEffects)
            {
                double max = 0.05 * sando.Level; // Max stat mult possible
                double min = max - 0.5 * max * (sando.Duration - 1); // Reaches -MAX at duration 5
                double randomFloat = min + ((max - min) * Rng.NextDouble());
                additionalStatMult += randomFloat;
            }
            return additionalStatMult;
        }
        /// <summary>
        /// Uses the effect of a sandwich type
        /// </summary>
        /// <param name="effect">The effect that will be used</param>
        /// <param name="expirationAction">Container for expiration messages</param>
        /// <returns>A list of messages of the sandwiches that have been consumed</returns>
        public void UseSandwichEffect(SandwichEffectType effect, Action<string> expirationAction)
        {
            List<(Sandwich, int)> expiredSandwiches = [.. _sandwichEffects.Where(s => s.Item1.Effect == effect && s.Item2 == 1)]; // Get the valid sandwiches that will be exausted here
            foreach (string sando in expiredSandwiches.Select(s => s.Item1.Name)) // print the expired ones
            {
                expirationAction($"The {sando} effect has worn off");
            }
            _sandwichEffects = [.. _sandwichEffects.Except(expiredSandwiches)]; // Remove expired
            for (int i = 0; i < _sandwichEffects.Count; i++)
            {
                (Sandwich, int) item = _sandwichEffects[i];
                if (item.Item1.Effect == effect)
                {
                    item.Item2--;
                    _sandwichEffects[i] = item; // Reduce duration by 1
                }
            }
        }
    }
    public class ExplorationManager
    {
        // Private data to retrieve stuff
        readonly ExplorationContext _context = new ExplorationContext();
        // Public data (saved)
        public string Dungeon { get; set; } = "";
        public int ExplorationSeed { get; set; }
        public (string, int, int) TrainerAndSeeds { get; set; } // Trainer seed, mon seed
        public string DirectoryPath { get; set; } = "";
        public bool UseExplorationTicket { get; set; } = false;
        public string ExplFavour { get; set; } = "";
        /// <summary>
        /// Sets up exploration basics to later run it
        /// </summary>
        /// <param name="directoryPath">Path where stuff will be put</param>
        public void InitializeExploration()
        {
            Random rng = GameplayElementsContainer.GlobalData.CommonRng;
            // First ask organizer to choose dungeon
            List<string> dungeonOptions = [.. GameplayElementsContainer.GlobalData.Dungeons.Keys];
            Console.WriteLine($"Creating a brand new exploration, which dungeon? (something else for random)\n{string.Join(", ", dungeonOptions)}");
            Dungeon = Console.ReadLine();
            if (!GameplayElementsContainer.GlobalData.Dungeons.ContainsKey(Dungeon)) // If invalid, pick a random dungeon then
            {
                Dungeon = GeneralUtilities.GetRandomPick(dungeonOptions, rng);
            }
            Console.WriteLine(Dungeon);
            // Then which player
            List<string> validTrainers = [.. GameplayElementsContainer.GlobalData.Trainers.Keys];
            Console.WriteLine($"Which trainer will explore?\n{string.Join(", ", validTrainers)}");
            TrainerEntity theTrainer = GameplayElementsContainer.GlobalData.Trainers[Console.ReadLine()];
            TrainerAndSeeds = (theTrainer.Name, rng.Next(), rng.Next());
            ExplorationSeed = rng.Next();
            // Some extra trainer stuff
            KeyItem explTicketItem = GameplayElementsContainer.GlobalData.KeyItems["Exploration Ticket"];
            if (theTrainer.KeyItems.ContainsKey(explTicketItem)) // Fetch the expl ticket key item
            {
                Console.WriteLine("Use ticket? y/N");
                UseExplorationTicket = Console.ReadLine().ToLower() == "y";
            }
            if (theTrainer.Favours.Count > 0)
            {
                Console.WriteLine($"Use Favour? {string.Join(", ", theTrainer.Favours.Keys.Select(t => t.Name))}");
                ExplFavour = Console.ReadLine();
            }
        }
        /// <summary>
        /// Begins an exploration, starts a simulation
        /// </summary>
        public void ExecuteExploration()
        {
            _context.Rng = new Random(ExplorationSeed);
            _context.ExplorationMessage.Clear(); // This is a new event, so I will clear whatever thet was there before
            // Just to help debug
            Console.Clear();
            // Initialize from beginning
            _context.DungeonData = GameplayElementsContainer.GlobalData.Dungeons[Dungeon];
            _context.TrainerEntity = GameplayElementsContainer.GlobalData.Trainers[TrainerAndSeeds.Item1]; // Find the trainer data
            // Initialize all trainer/mon instances. Will be stored in context
            if (_context.TrainerEntity.AutoTeam)
            {
                Random trainerRng = new Random(TrainerAndSeeds.Item2); // Only case where this is used really. Shuffles the Pokemon around, only remaining RNG stuff
                GeneralUtilities.ShuffleList(_context.TrainerEntity.Pokemon, trainerRng);
            }
            int monSeed = TrainerAndSeeds.Item3;
            Random pokemonRng = new Random(monSeed);
            _context.TrainerInstance = new TrainerInstance()
            {
                Name = _context.TrainerEntity.Name,
                Team = Side.BOTTOM,
                MaxMonsInField = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                // No limit for max mon used, will start with 3-10 and then who knows
                ActiveJewelry = _context.TrainerEntity.EquippedJewelry,
                PokemonInTeam = [.. _context.TrainerEntity.Pokemon.Select(p => new PokemonInstance(p, new Random(pokemonRng.Next())))]
            };
            // Check expl ticket
            if (UseExplorationTicket) // Remove ticket and also different message
            {
                KeyItem explTicketItem = GameplayElementsContainer.GlobalData.KeyItems["Exploration Ticket"];
                GeneralUtilities.AddtemToCountDictionary(_context.TrainerEntity.KeyItems, explTicketItem, -1); // Remove a ticket
                _context.ExplorationMessage.EventTitle = $"<@{_context.TrainerEntity.DiscordId}> Meanwhile, {_context.TrainerEntity.Name} used their Exploration Ticket for a free trip to {Dungeon}.";
            }
            else
            {
                _context.ExplorationMessage.EventTitle = $"<@{_context.TrainerEntity.DiscordId}> Meanwhile, {_context.TrainerEntity.Name} went on to explore the {Dungeon}.";
            }
            // Check if trainer will use an exploration assistance favour
            if (_context.TrainerEntity.Favours.Count > 0)
            {
                NpcTrainer favourTrainer = _context.TrainerEntity.Favours.Keys.Where(t => t.Name == ExplFavour).FirstOrDefault();
                if (favourTrainer != null)
                {
                    Console.WriteLine($"{favourTrainer.Name} selected");
                    GeneralUtilities.AddtemToCountDictionary(_context.TrainerEntity.Favours, favourTrainer, -1); // Take favour
                    TrainerEntity followerEntity = GameplayElementsContainer.GlobalData.CreateNpcEntity(favourTrainer, _context.TrainerEntity.Pokemon.Count, _context.Rng.Next(), 1, GameplayElementsContainer.SHINY_CHANCE, []);
                    _context.FollowerInstance = new TrainerInstance()
                    {
                        Name = followerEntity.Name,
                        Team = Side.BOTTOM, // Same team as player
                        MaxMonsInField = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                        // No limit for max mon used, will start with 3-10 and then who knows
                        ActiveJewelry = followerEntity.EquippedJewelry,
                        PokemonInTeam = [.. followerEntity.Pokemon.Select(p => new PokemonInstance(p, new Random(_context.Rng.Next())))] // Instance the mons too
                    };
                }
            }
            // Get the mon table for this dungeon (weather and ALL), need to also use the wild alternatives!
            HashSet<string> allWeatherMons = [.. _context.DungeonData.WeatherMons[_context.DungeonData.CurrentWeather.Name], .. _context.DungeonData.WeatherMons["ALL"]];
            foreach (string weatherMon in allWeatherMons)
            {
                PokemonSpecies monSpecies = GameplayElementsContainer.GlobalData.Dex[weatherMon];
                _context.EncounterTable.Add(monSpecies);
                _context.EncounterTable.UnionWith(monSpecies.WildAlternatives); // Also add the variants
            }
            // Also get item drop table for lookup
            _context.DropTable = [.. _context.DungeonData.CommonDrops, .. _context.DungeonData.RareDrops];
            // Exploration loop begin
            List<Encounter> pastEncounters = [];
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine($"Beginning of {_context.TrainerEntity.Name}'s exploration in {Dungeon}"));
            for (_context.CurrentFloor = 0; _context.CurrentFloor < GameplayElementsContainer.DUNGEON_NUM_FLOORS; _context.CurrentFloor++) // Begin the iteration of all floors
            {
                bool isFinalFloor = (_context.CurrentFloor == GameplayElementsContainer.DUNGEON_NUM_FLOORS - 1);
                for (int room = 0; room < GameplayElementsContainer.DUNGEON_ROOMS_PER_FLOOR; room++) // Begin the iteration of floor depth
                {
                    // Choose potential rooms to pick from
                    List<Encounter> possibleEncountersThisRoom;
                    if (room == GameplayElementsContainer.DUNGEON_ENCOUNTER_ROOM)
                    {
                        // Need to generate a procedural wild battle encounter
                        possibleEncountersThisRoom = [
                            new Encounter() // This encounter is simple because there's no scripted overrides just a Pokemon battle with some shit going on
                            {
                                Type = EncounterType.BATTLE_AND_PRIZE,
                                Category = EncounterCategory.DANGER,
                                PreEncounterString = "Suddenly, wild Pokemon attack!",
                                EncounterSuccessString = "You defeated the wild Pokemon and gathered some items that were dropped: $2", // Prints the prizes
                                EncounterFailureString = "The Pokemon in this area are quite tough...",
                                PrizeType = EncounterItemType.EQUIPPED_ITEMS,
                                UseEnemyCountSandwich = true,
                                Enemies = [
                                    new EnemyParams() // Standard enemy params besides these (3 pokemon but variabel length and catchable
                                    {
                                        Type = EncounterEnemyType.CURRENT_FLOOR,
                                        EquipType = EncounterItemType.COMMON_ITEM_POOL,
                                        EquipChance = _context.DungeonData.GeneralItemDropChance,
                                        Number = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                                        MaxSimultaneousMons = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                                        Catchable = true
                                    }
                                ]
                            }
                        ];
                    }
                    else if (isFinalFloor && room == GameplayElementsContainer.DUNGEON_FINAL_BOSS_ROOM)
                    {
                        possibleEncountersThisRoom = [_context.DungeonData.FinalBossEncounter]; // Simply the final boss encounter to choose from
                    }
                    else if (!isFinalFloor && room == GameplayElementsContainer.DUNGEON_BOSS_ROOM)
                    {
                        possibleEncountersThisRoom = [GeneralUtilities.GetRandomPick(_context.DungeonData.BossEncounters, _context.Rng)]; // Choose one random boss battle
                    }
                    else
                    {
                        // Otherwise, random pick of encounters, given their absolute weights and then a bit of shenanigans to softmax weights with some rng, sorts the resulting list (bit of black magic ngl)
                        possibleEncountersThisRoom = [.. _context.DungeonData.EncounterPool.OrderBy(e => GeneralUtilities.GetSortableRandomWeight(GetRoomEncounterWeight(e, room, pastEncounters), _context.Rng))];
                        int numberOfRooms = GeneralUtilities.GetRandomDecayingInt(1, GameplayElementsContainer.MAX_SIMULTANEOUS_ROOM_PICKS, 2, _context.Rng); // Choose between 1-5 rooms
                        possibleEncountersThisRoom = possibleEncountersThisRoom[..numberOfRooms]; // Splice only the first n
                    }
                    Encounter roomEncounter = GeneralUtilities.GetRandomPick(possibleEncountersThisRoom, _context.Rng); // Feels wasteful but now choose a random one of these encounters (all this was for animation sake)
                    // Now, attempt to do the room's encounter
                    ExecuteEncounter(roomEncounter);
                    pastEncounters.Add(roomEncounter); // This encounter happened
                    // Checking for the case where trainer or their friend has no more usable Pokemon
                    if (!_context.TrainerInstance.PokemonInTeam.Any(p => p.Availability == BattleAvailability.READY)
                        && !_context.FollowerInstance.PokemonInTeam.Any(p => p.Availability == BattleAvailability.READY))
                    {
                        Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine($"Trainer blacked out..."));
                        goto DungeonEnd; // Just go directly to end
                    }
                }
            }
        DungeonEnd: // Best way to break from a 2 damn nested loops I think
            ;
            // Exploration end part
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine("Exploration end."));
            GameplayElementsContainer.ConsumeTrainersItems(_context.TrainerEntity); // Consume all items for all mons
            GameplayElementsContainer.ConsumeTrainersJewelry(_context.TrainerEntity); // Consume jew too
            _context.TrainerEntity.SaveTrainerCsv(DirectoryPath);
            SaveExplorationOutcome(); // Replace with message stuff
        }
        /// <summary>
        /// Calculates the weight of each encounter room given a couple of extra params
        /// </summary>
        /// <param name="encounter">Encounter to check</param>
        /// <param name="room">Current room within floor</param>
        /// <param name="previousEncounters">List of previous encounters for history purposes</param>
        /// <returns></returns>
        double GetRoomEncounterWeight(Encounter encounter, int room, List<Encounter> previousEncounters)
        {
            double weight = encounter.BaseWeight; // Start with base weight (i.e. rarity)
            weight *= Math.Pow(0.1, previousEncounters.Count(x => x == encounter)); // Each time an encounter already happened, it's 90% less rare, prioritises events that hadn't happened yet
            if (previousEncounters.Count > 0 && previousEncounters.Last().Category == encounter.Category) weight *= 0.5; // Slightly reduces chance of multiple categories in a row
            // Danger chance multiplier depending on the difficulty level of the dungeon
            if (_context.DungeonData.Difficulty >= 5)
            {
                weight *= encounter.Category switch // Good things less likely, bad things more likely but also more prizes
                {
                    EncounterCategory.DANGER => 1.5,
                    EncounterCategory.REFRESH => 0.75,
                    EncounterCategory.PRIZE => 1.2,
                    _ => 1
                };
            }
            else if (_context.DungeonData.Difficulty >= 3)
            {
                weight *= encounter.Category switch // Good things less likely, bad things more likely but also more prizes (less so)
                {
                    EncounterCategory.DANGER => 1.2,
                    EncounterCategory.REFRESH => 0.85,
                    EncounterCategory.PRIZE => 1.1,
                    _ => 1
                };
            }
            else
            {
                weight *= 1; // Otherwise these low diffs dont multiply chances
            }
            // Room 4 (5th room) always try to prioritize a treasure or a healing (give insanely low score to anything else)
            if (room == 4 && (encounter.Category != EncounterCategory.PRIZE || encounter.Category != EncounterCategory.REFRESH))
            {
                weight *= 0.01;
            }
            // Finally, boost certain specific rooms or not depending on additional stuff (more specifics)
            switch (encounter.Type)
            {
                case EncounterType.CAMPING:
                    weight *= (_context.TrainerEntity.Sandwiches.Count > 0) ? 1.2 : 0.8; // Less chance of eating sandwich if there's not in the inventory
                    break;
                case EncounterType.EVO:
                    weight *= (_context.TrainerEntity.EvoPlates.Count > 0 && _context.TrainerEntity.Pokemon.Any(p => p.Species.Evos.Count > 0)) ? 1.2 : 0.8; // Less chance of eating evo event if no mon can use it (or no plates)
                    break;
                case EncounterType.HP_HEAL:
                    weight *= 1 + _context.TrainerInstance.PokemonInTeam.Count(p => p.HpPercent < 0.66) * 0.05; // Add a 5% for each mon below 66%
                    break;
                case EncounterType.FULL_RESTORE:
                    weight *= _context.TrainerInstance.PokemonInTeam.Any(p => p.HpPercent < 0.25) ? 1.2 : 0.8; // If mon heavily hurt there's more chance
                    break;
                case EncounterType.REVIVE:
                    weight *= _context.TrainerInstance.PokemonInTeam.Any(p => p.Availability == BattleAvailability.FAINT) ? 1.2 : 0.8; // If mon dead
                    break;
                case EncounterType.STATUS_CURE:
                    weight *= _context.TrainerInstance.PokemonInTeam.Any(p => p.NonVolatileStatusConditions.Count > 0) ? 1.2 : 0.8; // If any mon status'd
                    break;
                // These events are always base chance
                case EncounterType.BATTLE_AND_PRIZE:
                case EncounterType.JOINER:
                case EncounterType.DAMAGE_TRAP:
                case EncounterType.STATUS_TRAP:
                case EncounterType.KILL_TRAP:
                case EncounterType.NPC_BATTLE:
                case EncounterType.UNOWN:
                    break;
                default:
                    throw new Exception("Unimplemented event type in exploration");
            }
            return weight;
        }
        /// <summary>
        /// Executes an encounter of the many possible in dungeon
        /// </summary>
        /// <param name="encounter">Encounter to simulate</param>
        void ExecuteEncounter(Encounter encounter)
        {
            // Success or failure, they assemble the string, and then the string is printed in the end once loaded
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine($"{encounter.Type} - {encounter.PreEncounterString}"));
            // Now execute encounter
            List<PokemonInstance> targetMons; // Aux list
            switch (encounter.Type)
            {
                case EncounterType.BATTLE_AND_PRIZE:
                    ResolveBattleAndPrizeEncounter(encounter); // Works kinda for treasures and other stuff too lol
                    break;
                case EncounterType.CAMPING:
                    {
                        Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.PreEncounterString)); // TODO: Also animation later on ig
                        if (_context.TrainerEntity.Sandwiches.Count > 0) // Sandwich to be eaten
                        {
                            string nextString = encounter.EncounterSuccessString.Replace("$1", _context.TrainerEntity.Sandwiches[0].Name); // Will eat first sando in list
                            _context.AddSandwichEffect(_context.TrainerEntity.Sandwiches[0]);
                            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(nextString)); // TODO: Also animation later on ig
                            _context.TrainerEntity.Sandwiches.RemoveAt(0); // Consume sandwich
                        }
                        else // Otherwise, you fail to eat anything ig
                        {
                            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.EncounterFailureString)); // TODO: Also animation later on ig
                        }
                    }
                    break;
                case EncounterType.EVO:
                    Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.PreEncounterString)); // TODO: Also animation later on ig
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(p => p.AssociatedEntity.Species.Evos.Count > 0)];
                    if (targetMons.Count > 0)
                    {
                        foreach (PokemonInstance mon in targetMons)
                        {
                            // Bit of a long one but basically, get all the evos that can consume any plate from the trainer in order to evolve
                            List<PokemonSpecies> validEvos = [.. mon.AssociatedEntity.Species.Evos.Where(e => _context.TrainerEntity.EvoPlates.Keys.Any(p => p.Type == e.Types.Item1 || p.Type == e.Types.Item2))];
                            if (validEvos.Count > 0)
                            {
                                // Mon has a valid evo! Will evolve...
                                string preName = mon.AssociatedEntity.ToString();
                                PokemonSpecies chosenEvo = GeneralUtilities.GetRandomPick(validEvos, _context.Rng); // Get a random one
                                EvoPlate usedPlate = GeneralUtilities.GetRandomPick([.. _context.TrainerEntity.EvoPlates.Keys.Where(p => p.Type == chosenEvo.Types.Item1 || p.Type == chosenEvo.Types.Item2)], _context.Rng); // Get a random plate needed
                                _context.TrainerEntity.EvolveProcess(mon.AssociatedEntity, usedPlate, _context.Rng); // Atp this mon should have evolved
                                Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine($"{preName} has evolved into {mon.Name} using the {usedPlate.Name}!")); // TODO: Also animation later on ig
                            }
                        }
                        Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.EncounterSuccessString)); // TODO: Also animation later on ig
                    }
                    else // Nothing to evolve
                    {
                        Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.EncounterFailureString)); // TODO: Also animation later on ig
                    }
                    break;
                case EncounterType.HP_HEAL:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY && m.HpPercent < 1)];
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.HpPercent += 0.3;
                        mon.HpPercent = Math.Clamp(mon.HpPercent, 0, 1);
                    });
                    break;
                case EncounterType.FULL_RESTORE:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY && m.HpPercent < 1).OrderBy(m => m.HpPercent)];
                    targetMons = targetMons.Count > 0 ? [targetMons[0]] : []; // Get the first element only (or keep an empty list otherwise)
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.HpPercent = 1;
                    });
                    break;
                case EncounterType.REVIVE:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.FAINT)];
                    targetMons = targetMons.Count > 0 ? [targetMons[0]] : []; // Get the first element only (or keep an empty list otherwise)
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.HpPercent = 1;
                        mon.Availability = BattleAvailability.READY;
                    });
                    break;
                case EncounterType.STATUS_CURE:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY && m.NonVolatileStatusConditions.Count > 0)];
                    targetMons = targetMons.Count > 0 ? [targetMons[0]] : []; // Get the first element only (or keep an empty list otherwise)
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.NonVolatileStatusConditions.Clear();
                    });
                    break;
                case EncounterType.DAMAGE_TRAP:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY)];
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.HpPercent -= 0.25;
                        if (mon.HpPercent < 0)
                        {
                            mon.HpPercent = 0;
                            mon.Availability = BattleAvailability.FAINT;
                        }
                    });
                    break;
                case EncounterType.STATUS_TRAP:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY)];
                    targetMons = targetMons.Count > 0 ? [GeneralUtilities.GetRandomPick(targetMons, _context.Rng)] : []; // Get a random one of these
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.NonVolatileStatusConditions.Add(GameplayElementsContainer.GlobalData.NvStatuses[encounter.Params[0]]);
                    });
                    break;
                case EncounterType.KILL_TRAP:
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY)];
                    targetMons = targetMons.Count > 0 ? [GeneralUtilities.GetRandomPick(targetMons, _context.Rng)] : []; // Get a random one of these
                    ResolveActionOnMonsEncounter(encounter, targetMons, mon =>
                    {
                        mon.HpPercent = 0;
                        mon.Availability = BattleAvailability.FAINT;
                    });
                    break;
                case EncounterType.JOINER:
                    // Get a mon from this floor, create a very basic instance, add to party and prizes
                    {
                        PokemonSpecies randomSpecies = GeneralUtilities.GetRandomPick([.. _context.EncounterTable.Where(m => m.Rank == Math.Clamp(_context.CurrentFloor + 1, 1, 3))], _context.Rng); // Find a random mon
                        PokemonEntity newMon = new PokemonEntity()
                        {
                            Species = randomSpecies,
                            PokeBall = GameplayElementsContainer.GlobalData.PokeBalls["Poke Ball"], // All npc mons have a pokeball unless specified otherwise
                            IsShiny = _context.Rng.Next(0, _context.GetShinyChance()) == 0 // Depending on shiny chance ofc
                        };
                        GameplayElementsContainer.GlobalData.RandomizePokemon(newMon, _context.Rng); // Randomize the rest for this mon
                        PokemonInstance joinerInstance = new PokemonInstance(newMon, _context.Rng); // Creates rest of instance
                        // Finally, add the joiner to the team AND the catchtable
                        _context.TrainerInstance.PokemonInTeam.Add(joinerInstance);
                        AddMonToCatchTable(joinerInstance, 0);
                        _context.UseSandwichEffect(SandwichEffectType.SHINY_CHANCE, s => { });
                    }
                    break;
                case EncounterType.NPC_BATTLE:
                    // Npc rank will be diff dependant, so choose a combo of trainer ranks which will be used for filtering
                    HashSet<TrainerRank> validRanks = new HashSet<TrainerRank>();
                    if (_context.DungeonData.Difficulty <= 4) validRanks.Add(TrainerRank.UNRANKED);
                    if (_context.DungeonData.Difficulty >= 4 && _context.DungeonData.Difficulty <= 5) validRanks.Add(TrainerRank.GYM);
                    if (_context.DungeonData.Difficulty >= 4 && _context.DungeonData.Difficulty <= 6) validRanks.Add(TrainerRank.ELITE4);
                    if (_context.DungeonData.Difficulty >= 5) validRanks.Add(TrainerRank.CHAMPION);
                    // Pick a random one, keep name
                    string chosenNpc = GeneralUtilities.GetRandomPick([.. GameplayElementsContainer.GlobalData.AllNpcTrainers.Where(n => validRanks.Contains(n.Value.TrainerRank))], _context.Rng).Key;
                    chosenNpc = "Brock"; // TODO: Remove this in production, just that for now brock is the only implemented trainer
                    Encounter npcBattleEncounter =
                            new Encounter()
                            {
                                Type = EncounterType.BATTLE_AND_PRIZE,
                                Category = EncounterCategory.NEUTRAL,
                                PreEncounterString = encounter.PreEncounterString,
                                EncounterSuccessString = encounter.EncounterSuccessString,
                                EncounterFailureString = encounter.EncounterFailureString,
                                Params = [chosenNpc, chosenNpc, chosenNpc], // Will equip its own items but then reward the favour (item with name)
                                PrizeType = EncounterItemType.SPECIFIC_ITEM,
                                WeatherOverrides = encounter.WeatherOverrides,
                                UseFollower = false,
                                Enemies = [
                                    new EnemyParams()
                                    {
                                        Type = EncounterEnemyType.SPECIFIC_TRAINER,
                                        EquipType = EncounterItemType.TRAINER,
                                        EquipChance = GameplayElementsContainer.NPC_ITEM_CHANCE,
                                        Number = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                                        MaxSimultaneousMons = GameplayElementsContainer.DEFAULT_BATTLE_NMONS,
                                        Catchable = false,
                                        ApplyDifficultyMods = false
                                    }
                                ]
                            };
                    // Finally, need to do some shenanigans with the trainer, including healing to full, using 3 mons max (reverted after!) and reviving mon if needed
                    _context.TrainerInstance.MaxMonsUsed = GameplayElementsContainer.DEFAULT_BATTLE_NMONS;
                    // Revive mons until there's at least 3 alive
                    while (_context.TrainerInstance.PokemonInTeam.Count(m => m.Availability == BattleAvailability.READY) < GameplayElementsContainer.DEFAULT_BATTLE_NMONS)
                    {
                        targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability != BattleAvailability.READY)];
                        PokemonInstance monToRevive = GeneralUtilities.GetRandomPick(targetMons, _context.Rng);
                        monToRevive.HpPercent = 1;
                        monToRevive.Availability = BattleAvailability.READY;
                    }
                    // Then heal top 3 mons to full hp
                    targetMons = [.. _context.TrainerInstance.PokemonInTeam.Where(m => m.Availability == BattleAvailability.READY)]; // Get alive mons
                    for (int i = 0; i < GameplayElementsContainer.DEFAULT_BATTLE_NMONS; i++)
                    {
                        targetMons[i].HpPercent = 1; // Done!
                        targetMons[i].NonVolatileStatusConditions.Clear();
                    }
                    // Now run it
                    ResolveBattleAndPrizeEncounter(npcBattleEncounter);
                    _context.TrainerInstance.MaxMonsUsed = int.MaxValue; // Revert to trainer emptying their whole team in battles
                    break;
                case EncounterType.UNOWN:
                    string unownWord = GeneralUtilities.GetRandomPick([.. GameplayElementsContainer.GlobalData.UnownLookup.Keys], _context.Rng); // Find a random unown word
                    Encounter unownBattleEncounter =
                            new Encounter()
                            {
                                Type = EncounterType.BATTLE_AND_PRIZE,
                                Category = EncounterCategory.NEUTRAL,
                                PreEncounterString = encounter.PreEncounterString,
                                EncounterSuccessString = encounter.EncounterSuccessString,
                                EncounterFailureString = encounter.EncounterFailureString,
                                Params = [unownWord, unownWord],
                                PrizeType = EncounterItemType.UNOWN,
                                WeatherOverrides = encounter.WeatherOverrides,
                                Enemies = [
                                    new EnemyParams()
                                    {
                                        Type = EncounterEnemyType.UNOWN,
                                        Number = int.MaxValue, // All unowns at the same time
                                        MaxSimultaneousMons = int.MaxValue,
                                        StatMult = [1, 0.75, 0.75, 0.75, 0.75, 1], // Unowns will be slightly weaker to avoid curbstomp
                                        AutoDistribute = false
                                    }
                                ]
                            };
                    ResolveBattleAndPrizeEncounter(unownBattleEncounter);
                    break;
                default:
                    throw new Exception($"This encounter type {encounter.Type} is not known");
            }
        }
        /// <summary>
        /// Function that digs until the end to find every single relative of the desired Pokemon Species (i.e. it finds all alternates and prevos)
        /// </summary>
        /// <param name="speciesCollection">The species to look for family</param>
        /// <returns>The complete family tree but no evos!</returns>
        static HashSet<PokemonSpecies> GetFamilyHash(IEnumerable<PokemonSpecies> speciesCollection)
        {
            HashSet<PokemonSpecies> resultingSpecies = [];
            foreach (PokemonSpecies species in speciesCollection)
            {
                resultingSpecies.Add(species);
                foreach (PokemonSpecies alt in species.WildAlternatives)
                {
                    resultingSpecies.UnionWith(GetFamilyHash([alt]));
                }
                if (species.Prevo != null)
                {
                    resultingSpecies.UnionWith(GetFamilyHash([species.Prevo]));
                }
            }
            return resultingSpecies;
        }
        /// <summary>
        /// Resolves an encounter scripted battle. Similar to the wild battle one but this is way more custom. Doesn't use enemy quant sandwiches tho
        /// </summary>
        /// <param name="encounter"></param>
        void ResolveBattleAndPrizeEncounter(Encounter encounter)
        {
            // Automatic string reading-writing
            int paramsIndex = 0; // Index that will navigate through scripted battle's params
            List<string> proceduralStrings = []; // Will be assembled in some cases like when a random boss is found or sth. Also adds the params
            // The way the proc string will work is
            //  1 - The selected trainers, this is to name them in $1 (e.g. "Brock wants to battle!")
            //  2-n - The groups of mons, one by one. This is done to isolate Boss + Mobs
            // last - The prize(s) gathered. If the encounter designer knows what to look for, they can extract all corresponding strings
            // Enemy containers
            List<TrainerInstance> allEnemyInstances = []; // Contains all generated enemy instances that will participate in this fight
            // Potential prize tables, if the event allows it, both pokemon and items but only filled if catchable
            List<PokemonInstance> catchableEnemyMons = [];
            List<string> potentialPrizes = [];
            // Assemble all sides, enemy sides
            HashSet<PokemonSpecies> speciesUsed = []; // Species that have been used by these enemies
            foreach (EnemyParams enemy in encounter.Enemies) // Instance, one by one
            {
                List<string> itemsUsed = []; // Items that have been used by enemy, there can be duplicates
                if (enemy.Type == EncounterEnemyType.SPECIFIC_TRAINER)
                {
                    proceduralStrings.Add(encounter.Params[paramsIndex]); // If a specific trainer, add it here too as otherwise it won't be added to procedural strings to consumptions
                }
                // Assemble what the enemy can be, and what can it have
                HashSet<PokemonSpecies> availablePokemonSpecies = enemy.Type switch
                {
                    EncounterEnemyType.SPECIFIC_POKEMON => [.. encounter.Params[paramsIndex++].Split(",").Select(p => GameplayElementsContainer.GlobalData.Dex[p.Trim()])], // Gets (all the) specific pokemon in the params, separated by commas
                    EncounterEnemyType.NEXT_FLOOR => [.. _context.EncounterTable.Where(m => m.Rank == Math.Clamp(_context.CurrentFloor + 2, 1, 3))], // Make sure floor not out of bounds!
                    EncounterEnemyType.CURRENT_FLOOR => [.. _context.EncounterTable.Where(m => m.Rank == Math.Clamp(_context.CurrentFloor + 1, 1, 3))], // Make sure floor not out of bounds!
                    EncounterEnemyType.PREVIOUS_FLOOR => [.. _context.EncounterTable.Where(m => m.Rank == Math.Clamp(_context.CurrentFloor + 0, 1, 3))], // Make sure floor not out of bounds!
                    EncounterEnemyType.FIRST_FLOOR => [.. _context.EncounterTable.Where(m => m.Rank == 1)],
                    EncounterEnemyType.FAMILY => [.. GetFamilyHash(speciesUsed)],
                    EncounterEnemyType.SPECIFIC_TRAINER => [], // Yeah this needs to be resolved later on
                    EncounterEnemyType.NONE => throw new Exception("Enemy was not defined properly and still has NONE as type"),
                    _ => throw new Exception("Unknown scripted enemy type"),
                };
                List<string> availableEquippableItems = enemy.EquipType switch
                {
                    EncounterItemType.NONE => [],
                    EncounterItemType.SPECIFIC_ITEM => [.. encounter.Params[paramsIndex++].Split(",")], // Just get it from params ig
                    EncounterItemType.COMMON_ITEM_POOL => [.. _context.DungeonData.CommonDrops.Select(d => d.Name)],
                    EncounterItemType.RARE_ITEM_POOL => [.. _context.DungeonData.RareDrops.Select(d => d.Name)],
                    EncounterItemType.ALL_ITEM_POOLS => [.. _context.DungeonData.CommonDrops.Select(d => d.Name), .. _context.DungeonData.RareDrops.Select(d => d.Name)],
                    EncounterItemType.TRAINER => [.. GameplayElementsContainer.GlobalData.AllNpcTrainers[encounter.Params[paramsIndex++]].AvailableItems],
                    EncounterItemType.UNOWN or EncounterItemType.DISKS or EncounterItemType.IMP or EncounterItemType.EVO_PLATE or EncounterItemType.SANDWICH or EncounterItemType.ENEMIES or EncounterItemType.EQUIPPED_ITEMS
                        => throw new Exception("These item types are not a valid descriptor of what items a mon can equip. These are only for prizes. Try again"),
                    _ => throw new Exception("Unknown scripted item type"),
                };
                // Then, time to assemble the enemy "NPC trainer"
                NpcTrainer enemyTrainer;
                if (enemy.Type == EncounterEnemyType.SPECIFIC_TRAINER) // Fetch the NPC trainer directly from NPC list
                {
                    enemyTrainer = GameplayElementsContainer.GlobalData.AllNpcTrainers[encounter.Params[paramsIndex++]];
                }
                else // Otherwise generate the new NPC procedurally
                {
                    enemyTrainer = new NpcTrainer()
                    {
                        // No name or picture, would be left blank in renderer
                        TrainerRank = TrainerRank.WILD_MONS,
                        FullyLoadedData = true, // It's generated here...
                        AvailablePokemon = [.. availablePokemonSpecies],
                        AvailableItems = availableEquippableItems // Adds the potential items by name, the actual drop amount and prize type would be considered later (if the battle is won!)
                    };
                }
                // How many mons will appear?
                int nMons = enemy.Number;
                if (encounter.UseEnemyCountSandwich) // Check if the sandwich can alter the count
                {
                    nMons += _context.GetExtraWildPokemon();
                    if (nMons < 1) // Always at least one enemy...
                    {
                        nMons = 1;
                    }
                }
                // Next, the trainer entity, i.e. from NPC data into an actual entity
                TrainerEntity enemyTrainerEntity = GameplayElementsContainer.GlobalData.CreateNpcEntity(enemyTrainer, nMons, _context.Rng.Next(), enemy.EquipChance, _context.GetShinyChance(), itemsUsed); // For now the shiny chance is identical to all mons, scripted or not
                // Finally, the actual instance for game sim, both the trainer AND the mons
                // Create instance of all mons
                TrainerInstance enemyTrainerInstance = new TrainerInstance()
                {
                    Name = enemyTrainerEntity.Name,
                    Team = Side.TOP,
                    MaxMonsInField = enemy.MaxSimultaneousMons,
                    // No limit for max mon used, will start with 3-10 and then who knows
                    ActiveJewelry = enemyTrainerEntity.EquippedJewelry,
                    // Will add mons one by one later due to potential customizations
                };
                foreach (PokemonEntity enemyMon in enemyTrainerEntity.Pokemon) // Instance one by one
                {
                    PokemonInstance nextMonInstance = new PokemonInstance(enemyMon, _context.Rng);
                    if (enemy.ApplyDifficultyMods)
                    {
                        nextMonInstance.BaseStatMult = [.. enemy.StatMult.Select(i => i * GetDifficultyStatMult())]; // Its stats do need to be multiplied depending on danger level and the event's mult
                    }
                    enemyTrainerInstance.PokemonInTeam.Add(nextMonInstance);
                }
                // Now that the trainer has been assembled, if catchable, need to put into the lists
                speciesUsed.UnionWith(enemyTrainerEntity.Pokemon.Select(p => p.Species)); // Add to species used, useful for inter-enemy logic
                if (enemy.Catchable)
                {
                    potentialPrizes.AddRange(itemsUsed);
                    catchableEnemyMons.AddRange(enemyTrainerInstance.PokemonInTeam); // Add also the mons if catchable
                }
                proceduralStrings.Add(string.Join(", ", [.. enemyTrainerInstance.PokemonInTeam.Select(p => p.AssociatedEntity.Species.Name).Distinct().Order()])); // Add this enemy to procedural strings
                allEnemyInstances.Add(enemyTrainerInstance); // Enemy is in now!
            }
            // Assembling of procedural strings
            List<string> prizes = encounter.PrizeType switch // Define the prize pool first, since the prizes will be important for assembling procedural strings
            {
                EncounterItemType.NONE => [], // No prize is no prize
                EncounterItemType.SPECIFIC_ITEM => [.. encounter.Params[paramsIndex++].Split(",")], // All the csv prizes in the next param
                EncounterItemType.COMMON_ITEM_POOL => [GeneralUtilities.GetRandomPick([.. _context.DungeonData.CommonDrops.Select(d => d.Name)], _context.Rng)], // One item of the common item pool
                EncounterItemType.RARE_ITEM_POOL => [GeneralUtilities.GetRandomPick([.. _context.DungeonData.RareDrops.Select(d => d.Name)], _context.Rng)], // One item of the rare item pool
                EncounterItemType.ALL_ITEM_POOLS => [GeneralUtilities.GetRandomPick([.. _context.DropTable.Select(d => d.Name)], _context.Rng)], // Any item from the pool
                EncounterItemType.EQUIPPED_ITEMS => [.. potentialPrizes], // All the items that were equipped by the enemies
                EncounterItemType.ENEMIES => [], // Nothing in prizes then (the 0-rank mon appears after)
                EncounterItemType.IMP => [$"{GeneralUtilities.GetRandomNumber(2, 4, _context.Rng)} IMP"], // Between 2-4 IMP
                EncounterItemType.DISKS => [GameplayElementsContainer.GlobalData.GetMoveDiskByMove(GeneralUtilities.GetRandomPick([.. GameplayElementsContainer.GlobalData.Moves.Values], _context.Rng)).Name,
                    GameplayElementsContainer.GlobalData.GetMoveDiskByType(GeneralUtilities.GetRandomPick([.. Enum.GetValues<PokemonType>()], _context.Rng)).Name,
                    "Blank Disk"], // Returns one move disk, type disk, and a set of blank disks (quantity to be determined by droprate)
                EncounterItemType.EVO_PLATE => [GeneralUtilities.GetRandomPick([.. GameplayElementsContainer.GlobalData.EvoPlates.Keys], _context.Rng)], // Random evo plate
                EncounterItemType.SANDWICH => [GameplayElementsContainer.GlobalData.GetSandwichByEffect(
                    GeneralUtilities.GetRandomPick([.. Enum.GetValues<SandwichEffectType>()], _context.Rng),
                    GeneralUtilities.GetRandomDecayingInt(1,5,2,_context.Rng),
                    GeneralUtilities.GetRandomNumber(1,5,_context.Rng)
                    ).Name], // Generate a random sandwich on the spot with a random effect, random duration and decaying power
                EncounterItemType.UNOWN => [GameplayElementsContainer.GlobalData.UnownLookup[encounter.Params[paramsIndex++]]],
                EncounterItemType.TRAINER => [.. GameplayElementsContainer.GlobalData.AllNpcTrainers[encounter.Params[paramsIndex++]].AvailableItems],
                _ => throw new Exception("Unknown scripted item type for prizes"),
            };
            if (potentialPrizes.Count > 0)
            {
                proceduralStrings.Add(string.Join(", ", [.. potentialPrizes.Distinct().Order()])); // And also all items
            }
            else
            {
                proceduralStrings.Add("None");
            }
            string encounterStartString = encounter.PreEncounterString;
            string encounterWinString = encounter.EncounterSuccessString;
            string encounterLoseString = encounter.EncounterFailureString;
            for (int i = 0; i < proceduralStrings.Count; i++) // Now assemble all possible strings
            {
                encounterStartString = encounterStartString.Replace($"${i + 1}", proceduralStrings[i]);
                encounterWinString = encounterWinString.Replace($"${i + 1}", proceduralStrings[i]);
                encounterLoseString = encounterLoseString.Replace($"${i + 1}", proceduralStrings[i]);
            }
            // Pre combat text
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounterStartString)); // TODO: Also animation later on ig
            // Resolve the outcome, either via combat or some other event style
            bool playerWon = true; // Determines if the event was a success (true if no combat)
            if (allEnemyInstances.Count > 0)
            {
                GameOutcome outcome = ResolveEncounter(allEnemyInstances, GetGameParameters(encounter), encounter.UseFollower);
                playerWon = outcome.WinningTeam == Side.BOTTOM; // Trainer won if bottom won
            }
            if (playerWon) // If won, can then assign the prizes and the capture tables
            {
                bool rolledItems = false;
                if (encounter.PrizeType == EncounterItemType.ENEMIES) // Special situation, add to 0-rank mons
                {
                    foreach (PokemonInstance mon in allEnemyInstances.SelectMany(e => e.PokemonInTeam))
                    {
                        AddMonToCatchTable(mon, 0); // Add mon to catchtable, but override the rank to 0
                    }
                }
                // Give the prizes then
                foreach (string item in prizes) // Assign item + count for this dung
                {
                    ItemDrop foundItem = _context.DropTable.FirstOrDefault(i => i.Name == item);
                    double count;
                    if (foundItem == null) // Item not in the drop table, no other choice but to give 1 I guess (may be a mon too)
                    {
                        count = 1;
                    }
                    else
                    {
                        // Then I can actually roll this
                        rolledItems = true;
                        count = GeneralUtilities.GetRandomNumber(foundItem.Min, foundItem.Max + 1, _context.Rng);
                        count *= _context.GetItemMult(); // Add the mult of course
                        if (count < 0) count = 0; // Can't remove items!
                    }
                    int totalCount = (int)Math.Round(count);
                    GameplayElementsContainer.GlobalData.GivePrizeToTrainer(item, _context.TrainerEntity, totalCount);
                    GeneralUtilities.AddtemToCountDictionary(_context.Prizes.ItemPrizes, item, totalCount); // Add also to log message
                }
                // If sandwich for item drop was used, then I roll it
                if (rolledItems)
                {
                    _context.UseSandwichEffect(SandwichEffectType.ITEM_DROP, s => { }); // Need to actually notify renderer about this later on
                }
                // Then, the mon tables. Check the CatchableSpecies, which should've been modified during battle
                foreach (PokemonInstance mon in catchableEnemyMons.Where(m => m.Availability == BattleAvailability.FAINT)) // Can only capture fainted mons
                {
                    AddMonToCatchTable(mon); // Add mon to catchtable
                }
            }
            // If a mon count or shiny sandwich was in effect, consume it
            if (encounter.UseEnemyCountSandwich)
            {
                _context.UseSandwichEffect(SandwichEffectType.ENEMY_NUMBER, s => { });
            }
            _context.UseSandwichEffect(SandwichEffectType.SHINY_CHANCE, s => { });
            // Final things, check which string to output
            string encounterEndString = playerWon ? encounterWinString : encounterLoseString;
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounterEndString)); // TODO: anim
        }
        /// <summary>
        /// Saves exploration outcome in a txt file for quick copying
        /// </summary>
        void SaveExplorationOutcome()
        {
            string explFile = $"{_context.TrainerEntity.Name.ToUpper()}_EXPLORATION.expl";
            _context.ExplorationMessage.EventText.Clear();
            _context.ExplorationMessage.EventText.AppendLine($"Fate: ||FATE HERE||");
            _context.ExplorationMessage.EventText.AppendLine();
            _context.ExplorationMessage.EventText.AppendLine($"__{_context.TrainerEntity.Name} has obtained the following items:__");
            _context.ExplorationMessage.EventText.AppendLine();
            // List of items ordered
            List<string> prizes = [.. _context.Prizes.ItemPrizes.Select(p => $"{p.Key}{((p.Value > 1) ? $" x{p.Value}" : "")}").Order()];
            _context.ExplorationMessage.EventText.AppendLine($"**Items Found: **||{string.Join(", ", prizes)}||");
            // The mon printing is weird
            _context.ExplorationMessage.EventText.AppendLine($"__Catchable Pokemon (As many as you want as long as you have the corresponding Poke Ball for the Pokemon rank):__");
            _context.ExplorationMessage.EventText.AppendLine();
            List<int> ranks = [.. _context.Prizes.MonsFound.Keys.Order()];
            _context.ExplorationMessage.EventText.Append($"||"); // Beginning of rank Spoilered text
            foreach (int rank in ranks)
            {
                string monsOfRank = string.Join(", ", _context.Prizes.MonsFound[rank].Select(p => $"{p.Key}{((p.Value > 1) ? $" x{p.Value}" : "")}").Order());
                _context.ExplorationMessage.EventText.Append($"**RANK {rank}:** {monsOfRank}. ");
            }
            _context.ExplorationMessage.EventText.Append($"||"); // Close rank-spoilered text
            // Check if any warning for items
            string warningString = GameplayElementsContainer.GetTrainerInventoryWarning(_context.TrainerEntity);
            if (warningString != "")
            {
                _context.ExplorationMessage.PostEventText.AppendLine($"**Note: **||{warningString}||");
            }
            _context.ExplorationMessage.Signature = "";
            // Save
            string filePath = Path.Combine(DirectoryPath, explFile);
            _context.ExplorationMessage.SaveToFile(filePath);
        }
        /// <summary>
        /// Applies all pre-fight traine rmods (i.e. team level)
        /// </summary>
        void PreFightTrainerMods()
        {
            foreach (PokemonInstance mon in _context.TrainerInstance.PokemonInTeam)
            {
                for (int i = 0; i < mon.BaseStatMult.Length; i++)
                {
                    mon.BaseStatMult[i] = _context.GetExtraStatMult();
                }
            }
        }
        /// <summary>
        /// Apllies all trainer post-fight events, including healing and reset of lvl
        /// </summary>
        void PostFightTrainerMods()
        {
            foreach (PokemonInstance mon in _context.TrainerInstance.PokemonInTeam)
            {
                for (int i = 0; i < mon.BaseStatMult.Length; i++)
                {
                    mon.BaseStatMult[i] = 1;
                }
                double postBattleHealing = _context.GetExtraHealing();
                if (postBattleHealing > 0)
                {
                    Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine($"{mon.AssociatedEntity} healed {postBattleHealing * 100}%"));
                    // Later on, put in renderer queue/state machine
                    mon.HpPercent += postBattleHealing;
                    mon.HpPercent = Math.Clamp(mon.HpPercent, 0, 1);
                }
                List<string> expirationMessages = [];
                // TODO: Add renderer action
                _context.UseSandwichEffect(SandwichEffectType.POST_HEALING, s => { });
                _context.UseSandwichEffect(SandwichEffectType.STAT_MULT, s => { });
                foreach (string msg in expirationMessages)
                {
                    Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(msg));
                    // Later on, put in renderer queue/state machine
                }
            }
        }
        /// <summary>
        /// Returns, for a specific dungeon, the stat multiplier that needs to apply
        /// </summary>
        /// <returns>The resulting difficulty stat mmultiplier</returns>
        double GetDifficultyStatMult()
        {
            double mult = 1;
            if (_context.DungeonData.Difficulty > 1)
            {
                mult += _context.DungeonData.Difficulty * 0.05; // Multiplier will go from 1.10 to 1.3 depending on the diff
            }
            return mult;
        }
        /// <summary>
        /// Resolves an encounter between explorer and enemy instance, ready to go
        /// </summary>
        /// <param name="enemies">All enemy side instances with all the mons and shit ready</param>
        /// <param name="parameters">The game parameters for this game</param>
        /// <returns>The Game Outcome</returns>
        GameOutcome ResolveEncounter(List<TrainerInstance> enemies, GameParameters parameters, bool useFollower)
        {
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.Write($"Pre vs enemies"));
            PreFightTrainerMods();
            List<TrainerInstance> allParticipants = [.. enemies];
            allParticipants.Add(_context.TrainerInstance);
            if (useFollower && _context.FollowerInstance != null) // Also add follower if any
            {
                allParticipants.Add(_context.FollowerInstance);
            }
            GameSimulator.GameSimulator sim = new GameSimulator.GameSimulator();
            GameOutcome outcome = sim.Simulate(allParticipants, parameters, _context.Rng); // Simulates the game
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.Write($"Post vs enemies"));
            PostFightTrainerMods();
            return outcome;
        }
        /// <summary>
        /// Gets the standard game params to use in an exploration battle
        /// </summary>
        /// <param name="encounter">Encounter, contains parameters that would be overriden</param>
        /// <returns>The params to be used</returns>
        GameParameters GetGameParameters(Encounter encounter)
        {
            // Check the battlefield to be used
            Battlefield battlefieldUsed;
            if (encounter.BattlefieldOverride != null)
            {
                battlefieldUsed = encounter.BattlefieldOverride;
            }
            else
            {
                // Otherwise get the dungeon's, picked at random
                List<Battlefield> potentialBattlefileds = [.. _context.DungeonData.PossibleBattlefields.Where(b => b.Floors.Contains(_context.CurrentFloor))]; // Obtain battlefields for this floor
                int battlefieldIdx = GeneralUtilities.GetRandomWeightedIndex([.. potentialBattlefileds.Select(b => b.Weight)], _context.Rng);
                battlefieldUsed = potentialBattlefileds[battlefieldIdx];
            }
            // Check the field mods for this time (or if overriden)
            FieldModifier fieldMod;
            if (encounter.WeatherOverrides.Count > 0)
            {
                int weatherOverrideIdx = GeneralUtilities.GetRandomWeightedIndex([.. encounter.WeatherOverrides.Select(w => w.Chance)], _context.Rng);
                fieldMod = GameplayElementsContainer.GlobalData.FieldModifiers[encounter.WeatherOverrides[weatherOverrideIdx].Name];
            }
            else
            {
                fieldMod = GameplayElementsContainer.GlobalData.FieldModifiers[_context.DungeonData.CurrentWeather.Name];
            }
            // Now instance it
            GameParameters parameters = new GameParameters()
            {
                FieldModifier = fieldMod, // Fetch the weather
                FocusedSide = Side.BOTTOM // Player is the star here
            };
            return parameters;
        }
        /// <summary>
        /// Adds a pokemon (+ count +overriden rank) to catch table
        /// </summary>
        /// <param name="mon">Which mon (instance) to add</param>
        /// <param name="rankOverride">Use species rank by default, define if there's override needed</param>
        void AddMonToCatchTable(PokemonInstance mon, int rankOverride = -1)
        {
            int rank = (rankOverride < 0) ? mon.AssociatedEntity.Species.Rank : rankOverride; // Check if special rank override
            string monName = mon.AssociatedEntity.Species.Name + ((mon.AssociatedEntity.IsShiny) ? " ★" : "");
            if (!_context.Prizes.MonsFound.TryGetValue(rank, out Dictionary<string, int> thisRankDict)) // Add for this rank if nothing yet
            {
                thisRankDict = [];
                _context.Prizes.MonsFound[rank] = thisRankDict;
            }
            GeneralUtilities.AddtemToCountDictionary(thisRankDict, monName, 1);
            GameplayElementsContainer.GlobalData.AddMonToBox(mon.AssociatedEntity, "wild"); // Add mon to box too, but as a "wild" trainer, so the Nature + Moves can persist
        }
        /// <summary>
        /// Resolves an encounter that performs something depending on actions on mons
        /// </summary>
        /// <param name="encounter">Encounter to resolve</param>
        /// <param name="mons">Mons to apply action to</param>
        /// <param name="action">Action to apply</param>
        static void ResolveActionOnMonsEncounter(Encounter encounter, List<PokemonInstance> mons, Action<PokemonInstance> action)
        {
            Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.PreEncounterString)); // TODO: Also animation later on ig
            if (mons.Count > 0)
            {
                string successString = encounter.EncounterSuccessString.Replace("$1", string.Join(", ", mons.Select(m => m.Name)));
                foreach (PokemonInstance mon in mons)
                {
                    action(mon);
                }
                Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(successString)); // TODO: Also animation later on ig
            }
            else
            {
                Debug.DebugAction(DebugLevel.EVENT_ORGANIZER, () => Console.WriteLine(encounter.EncounterFailureString)); // TODO: Also animation later on ig
            }
        }
    }
}
