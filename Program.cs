using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Quic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static QuestGame;

static class Program
{
    static string pathGames = "games.json";
    static string pathNames = "namesandalias.json";
    static string pathConfigs = "configs.json";
    static List<QuestGame> games = new List<QuestGame>();
    static List<QuestGame.QuestGameConfig> configs = new List<QuestGame.QuestGameConfig>();
    const string BLOCK = "████████████████████████████████████████████████████████████████";
    const string DASH = "----------------------------------------------------------------";
    static void Main()
    {
        if (!File.Exists(pathGames))
        {
            using (_ = File.CreateText(pathGames)) { } // The using block will automatically close the text file after creation


            JObject file = new JObject();
            file.Add("games", new JArray());
            File.WriteAllText(pathGames, file.ToString());
        }

        if (!File.Exists(pathNames))
        {
            using (_ = File.CreateText(pathNames)) { }

            JObject file = new JObject();

            File.WriteAllText(pathNames, file.ToString());
        }

        if (!File.Exists(pathConfigs))
        {
            using (_ = File.CreateText(pathConfigs)) { }

            JObject file = new JObject();
            file.Add("configs", new JArray());
            File.WriteAllText(pathConfigs, file.ToString());
        }

        games = QuestGamesToCSharpObjects();
        configs = QuestConfigsToCSharpObjects();

        MainMenu();
                // Dictionary<string,int[]> tallyArray = new Dictionary<string, int[]>();
        // tallyArray.Add("Orion",new int[4]);
        // tallyArray.Add("Jacob",new int[4]);
        // tallyArray.Add("Ali",new int[4]);
        // tallyArray.Add("Simon",new int[4]);
        // tallyArray.Add("Xiameera",new int[4]);
        // tallyArray.Add("Seven",new int[4]);

        // for (int i = 0; i < games.Count; i++)
        // {
        //     foreach (QuestPlayer q in games[i].Players)
        //     {
        //         int[] tallies = tallyArray[q.PlayerID];
        //         if (q.Role.IsGood())
        //         {
        //             tallies[1] += 1;
        //             if (q.Victory == Victory.Full)
        //             {
        //                 tallies[0] += 1;
        //             }
        //         }
                
        //         if (q.Role.IsEvil())
        //         {
        //             tallies[3] += 1;
        //             if (q.Victory == Victory.Full)
        //             {
        //                 tallies[2] += 1;
        //             }
        //         }
        //     }

            
        // }

        // foreach (string player in tallyArray.Keys)
        // {
        //     Console.WriteLine($"{player}: ");
        //     Console.WriteLine($"    GoodWinRate: {(double) tallyArray[player][0]/tallyArray[player][1]}");
        //     Console.WriteLine($"    EvilWinRate: {(double) tallyArray[player][2]/tallyArray[player][3]}");
        //     Console.WriteLine($"   TotalWinRate: {(double) (tallyArray[player][0]+tallyArray[player][2])/(tallyArray[player][3]+tallyArray[player][1])}");
        // }
    }


    public static void MainMenu()
    {
        List<(string, Action)> mainMenuValueActions = new List<(string, Action)>{
            ("New Game",RecordNewGame),
            ("List Games",ListAndEditGames),
            ("New Game Configuration",MakeNewGameConfig),
            ("Game Analysis",new Action(() => throw new NotImplementedException()))
        };
        ConsoleOptionsAndFunction(null, mainMenuValueActions);
    }

    public static void ConsoleOptionsAndFunction(Action? runEachGo, List<(string, Action)> optionFunctionPairs)
    {
        runEachGo?.Invoke();

        Console.WriteLine("Select (#) or (ESC):");
        for (int i = 0; i < optionFunctionPairs.Count; i++)
        {
            Console.WriteLine($"({i + 1}) {optionFunctionPairs[i].Item1}");
        }
        Console.Write(">> ");
        ConsoleKeyInfo choice = Console.ReadKey();
        Console.WriteLine();
        int choiceInt;
        if (int.TryParse(choice.KeyChar.ToString(), out choiceInt) && 0 < choiceInt && choiceInt <= optionFunctionPairs.Count)
        {
            Console.WriteLine(BLOCK);
            for (int i = 0; i < 10; i++) { Console.WriteLine(); }
            optionFunctionPairs[choiceInt - 1].Item2.Invoke();
            Console.WriteLine();
            Console.WriteLine(BLOCK);
            ConsoleOptionsAndFunction(runEachGo, optionFunctionPairs);
        }
        else if (choice.Key == ConsoleKey.Escape)
        {
            //This character is simply to remove the adverse effect of pressing escape in the c# console
            Console.WriteLine("0");
            return;
        }
        else
        {
            Console.WriteLine("Invalid input.");
            Console.WriteLine();
            ConsoleOptionsAndFunction(runEachGo, optionFunctionPairs);
        }
    }

    public static void RecordNewGame()
    {
        QuestGame newQuestGame = new QuestGame();

        Console.WriteLine("Select Config: ");
        for (int i = 0; i < configs.Count; i++)
        {
            Console.WriteLine($"{i} {DASH}");
            Console.WriteLine($"{configs[i]}");
        }

        int choice;
        if (int.TryParse(Console.ReadLine(), out choice) && 0 <= choice && choice < configs.Count)
        {
            newQuestGame.Config = configs[choice];
        }
        else
        {
            Console.WriteLine("Invalid Number");
            RecordNewGame();
            return;
        }

        games.Add(newQuestGame);


        int gameIndex = games.Count - 1;
        if (games.Count != 1)
        {
            Console.WriteLine("Play with previous players? (y/n)");
            if (games[gameIndex].Config.NumberOfPlayers != games[gameIndex - 1].Config.NumberOfPlayers)
            {
                WriteWarning("WARNING: Player Count Mismatch");
            }

            if (InputYesNo())
            {
                for (int i = 0; i < games[gameIndex - 1].Players.Count; i++)
                {
                    games[gameIndex].Players.Add(new QuestGame.QuestPlayer()
                    {
                        PlayerID = games[gameIndex - 1].Players[i].PlayerID
                    });
                }
            }
        }


        EditGame(gameIndex);
    }


    public static void ListAndEditGames()
    {
        for (int i = 0; i < games.Count; i++)
        {
            Console.WriteLine(PadTruc($"Game {PadTruc(i.ToString(), 3, true)}: {DASH}", 64));
            WriteQuestGame(games[i]);
            CheckForGameInconsistencies(i);
            Console.WriteLine();
        }

        Console.Write(">> ");
        string? stringInput = Console.ReadLine();
        Console.WriteLine(BLOCK);
        for (int i = 0; i < 10; i++) { Console.WriteLine(); }
        int choice;
        if (int.TryParse(stringInput, out choice))
        {
            if (choice < 0)
            {
                return;
            }

            if (choice >= games.Count)
            {
                choice = games.Count - 1;
            }
            EditGame(choice);
        }
    }


    public static void EditGame(int gameIndex)
    {
        List<(string, Action)> editGameValueActions = new List<(string, Action)>{
            ("Players",() => EditPlayers(gameIndex)),
            ("Leadership",() => InputLeadership(gameIndex)),
            ("AmuletObservations", () => InputAmuletObservations(gameIndex)),
            ("Round Wins",() => InputRounds(gameIndex)),
            ("Final Quest or Hunt",() => InputFinalQuest(gameIndex)),
            ("Assign Players to Roles",() => AssignPlayersToRoles(gameIndex)),
            ("Victory",() => EditVictory(gameIndex)),
            ("Notes",() => EditNotes(gameIndex)),
        };
        ConsoleOptionsAndFunction(() =>
            {
                UpdateJsonFiles();
                Console.WriteLine(BLOCK);
                WriteQuestGame(games[gameIndex]);
                Console.WriteLine(DASH);
                CheckForGameInconsistencies(gameIndex);
            }
            , editGameValueActions);
        UpdateJsonFiles();
    }

    // Add and remove players
    public static void EditPlayers(int gameIndex)
    {
        List<(string, Action)> editPlayersValueAction = new List<(string, Action)>{
            ("Add Players",() => AddPlayers(gameIndex)),
            ("Remove Players",() => RemovePlayers(gameIndex)),
            ("Clear Players", () => ClearPlayers(gameIndex))
        };

        ConsoleOptionsAndFunction(null, editPlayersValueAction);
    }

    public static void AddPlayers(int gameIndex)
    {
        QuestGame currentGame = games[gameIndex];

        Console.WriteLine("Current Players:");
        Console.Write("[");
        currentGame.Players.ForEach(p => Console.Write(p.PlayerID + ", "));
        Console.WriteLine("]");

        Console.Write("Input Players: ");
        string? rawInput = Console.ReadLine();
        Console.WriteLine();

        List<string> tokens = TokenizeRawInput(rawInput, "[^a-zA-Z]");

        List<string> namesToBeAdded = AliasesToNames(tokens);

        Console.Write("NamesToBeAdded: ");
        namesToBeAdded.ForEach(p => Console.Write($"{p}, "));
        Console.WriteLine();

        // Make sure the name isn't already added, (this is the UNION set
        // operation, but can't be done with List.Union because I didn't bother
        // implementing equality for QuestPlayer)
        foreach (string name in namesToBeAdded)
        {
            if (currentGame.Players.Select(p => p.PlayerID).Contains(name))
            {
                return;
            }

            currentGame.Players.Add(new QuestGame.QuestPlayer { PlayerID = name });
        }
    }

    public static void RemovePlayers(int gameIndex)
    {
        QuestGame currentGame = games[gameIndex];

        Console.WriteLine("Current Players:");
        Console.Write("[");
        currentGame.Players.ForEach(p => Console.Write(p.PlayerID + ", "));
        Console.WriteLine("]");

        Console.Write("Input Players (to be removed): ");
        string? rawInput = Console.ReadLine();
        Console.WriteLine();

        List<string> tokens = TokenizeRawInput(rawInput, "[^a-zA-Z]");

        List<string> namesToBeRemoved = AliasesToNames(tokens);

        Console.Write("NamesToBeRemoved: ");
        namesToBeRemoved.ForEach(p => Console.Write($"{p}, "));
        Console.WriteLine();

        // Make sure the name isn't already added, (this is the UNION set
        // operation, but can't be done with List.Union because I didn't bother
        // implementing equality for QuestPlayer)
        foreach (string name in namesToBeRemoved)
        {
            QuestGame.QuestPlayer? playerToRemove = currentGame.Players.SingleOrDefault(p => p.PlayerID == name);

            if (playerToRemove != null)
            {
                currentGame.Players.Remove(playerToRemove);
            }
        }
    }

    public static List<string> AliasesToNames(List<string> tokens)
    {
        string jsonString = System.IO.File.ReadAllText(pathNames);
        JObject namesAndAliasesJ = JObject.Parse(jsonString);
        List<string> matchingNames = new List<string>();

        foreach (string token in tokens)
        {
            bool tokenRecognized = false;
            foreach (KeyValuePair<string, JToken?> child in namesAndAliasesJ)
            {
                if (child.Key.ToLower() == token.ToLower())
                {
                    matchingNames.Add(child.Key);
                    tokenRecognized = true;
                    break;
                }
                JArray aliases = (JArray)child.Value!;
                if (aliases.Select(p => p.ToString().ToLower()).Contains(token.ToLower()))
                {
                    matchingNames.Add(child.Key);
                    tokenRecognized = true;
                    break;
                }
            }
            if (!tokenRecognized)
            {
                Console.WriteLine($"Unrecognized Token: {token}");
            }
        }

        return matchingNames;
    }

    public static void ClearPlayers(int gameIndex)
    {
        games[gameIndex].Players = new List<QuestPlayer>();
    }

    public static void InputLeadership(int gameIndex)
    {
        Console.WriteLine("Input Leaders (in order)");
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> playerNames = AliasesToNames(TokenizeRawInput(rawInput, "[^a-zA-Z]"));
        games[gameIndex].RoundLeaders = playerNames;
        // if (playerNames.Distinct().Count() < playerNames.Count())
        // {
        //     WriteWarning("WARNING: Duplicate Player");
        // }
        // List<string> playerIDs = games[gameIndex].Players.Select(p => p.PlayerID).ToList();
        // foreach (string pstr in playerNames)
        // {
        //     if (!playerIDs.Contains(pstr))
        //     {
        //         WriteWarning($"WARNING: {pstr} is not in game");
        //     }
        // }
    }

    public static void InputAmuletObservations(int gameIndex)
    {
        List<(string, Action)> inputAmuletMenu = new List<(string, Action)>{
            ("Amulet Observers",() => InputAmuletObservers(gameIndex)),
            ("Amulet Observed",() => InputAmuletObserved(gameIndex))
        };

        ConsoleOptionsAndFunction(null, inputAmuletMenu);
    }

    public static void InputAmuletObservers(int gameIndex)
    {
        Console.WriteLine("Input Amulet Observers (in order)");
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> playerNames = AliasesToNames(TokenizeRawInput(rawInput, "[^a-zA-Z]"));
        while (playerNames.Count > games[gameIndex].AmuletObservations.Count)
        {
            games[gameIndex].AmuletObservations.Add(("",""));
        }
        for (int i = 0; i < playerNames.Count; i++)
        {
            games[gameIndex].AmuletObservations[i] = (playerNames[i],games[gameIndex].AmuletObservations[i].Item2);
        }

        // if (playerNames.Distinct().Count() < playerNames.Count())
        // {
        //     WriteWarning("WARNING: Duplicate Player");
        // }
    }


    public static void InputAmuletObserved(int gameIndex)
    {
        Console.WriteLine("Input Amulet Observed (in order)");
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> playerNames = AliasesToNames(TokenizeRawInput(rawInput, "[^a-zA-Z]"));
        while (playerNames.Count > games[gameIndex].AmuletObservations.Count)
        {
            games[gameIndex].AmuletObservations.Add(("",""));
        }
        for (int i = 0; i < playerNames.Count; i++)
        {
            games[gameIndex].AmuletObservations[i] = (games[gameIndex].AmuletObservations[i].Item1,playerNames[i]);
        }

        // if (playerNames.Distinct().Count() < playerNames.Count())
        // {
        //     WriteWarning("WARNING: Duplicate Player");
        // }
    }


    public static void InputRounds(int gameIndex)
    {
        var currentGame = games[gameIndex];

        Console.WriteLine("Input Game Rounds: ");
        string? rawString = Console.ReadLine();

        List<string> tokens = TokenizeRawInput(rawString, "[^GEge]");

        var selectedTokens = tokens.Where(p => p.Length == 1);

        currentGame.RoundWins = new List<QuestGame.RoundWin>();
        foreach (var token in selectedTokens)
        {
            if (token.ToLower() == "g")
            {
                currentGame.RoundWins.Add(QuestGame.RoundWin.Good);
            }
            else if (token.ToLower() == "e")
            {
                currentGame.RoundWins.Add(QuestGame.RoundWin.Evil);
            }
            else
            {
                throw new UnreachableException("Round Win was not of the character 'G' or 'E'");
            }
        }
    }

    public static void InputFinalQuest(int gameIndex)
    {
        Console.WriteLine("Is the hunter in the game or did the Final Quest Begin (5 mins of talking)? (y/n)");
        games[gameIndex].HunterSuccessful = null;
        games[gameIndex].GoodLastChanceSuccessful = null;
        games[gameIndex].HasFinalQuestOrHunt = InputYesNo();

        if (!games[gameIndex].HasFinalQuestOrHunt)
        {
            return;
        }

        Console.WriteLine("Did the Hunter hunt? (y/n)");
        if (InputYesNo())
        {
            Console.WriteLine("Did the Hunter Succeed? (y/n)");
            games[gameIndex].HunterSuccessful = InputYesNo();
            return;
        }

        Console.WriteLine("Did Good's Last Chance Succeed? (y/n)");
        games[gameIndex].GoodLastChanceSuccessful = InputYesNo();
    }

    public static void AssignPlayersToRoles(int gameIndex)
    {
        games[gameIndex].Players.ForEach(p => p.Role = null);
        foreach (QuestRole role in games[gameIndex].Config.Roles)
        {   
            Console.Write("[");
            games[gameIndex].Players.Where(p => p.Role == null).ToList().ForEach(p => Console.Write($"{p.PlayerID}, "));
            Console.WriteLine("]");
            Console.WriteLine($"Who was {role}?");
            while (true)
            {
                Console.Write(">> ");
                string? rawInput = Console.ReadLine();
                if (rawInput == null || rawInput == "")
                {   
                    Console.WriteLine("Skipped Role");
                    break;
                }
                List<string> names = AliasesToNames(TokenizeRawInput(rawInput, "[^a-zA-Z]"));
                if (names.Count == 0)
                {

                }
                else if (!games[gameIndex].Players.Select(p => p.PlayerID).Contains(names[0]))
                {
                    Console.WriteLine("ERROR: That player is not in this game");
                }
                else
                {   
                    // decided it was overly annoying to ask for confirmation, and we can just enter it again if you get it wrong.
                    // Console.WriteLine($"{names[0]} (y/n)");
                    var player = games[gameIndex].Players.Where(p => p.PlayerID == names[0]).ToList();
                    // if (player.Count > 1)
                    // {
                    //     WriteWarning("WARNING: Duplicate Player ID");
                    // }

                    // if (player[0].Role != null)
                    // {
                    //     WriteWarning("WARNING: Player already has Role");
                    // }
                    // if (InputYesNo())
                    // {

                    // }

                    player[0].Role = role;
                    Console.WriteLine(player[0].PlayerID);
                    break;
                }
                Console.WriteLine(DASH);
            }
            Console.WriteLine(BLOCK);
        }
    }

    public static void EditVictory(int gameIndex)
    {
        List<(string, Action)> editPlayersValueAction = new List<(string, Action)>{
            ("Auto Assign Victory",() => AutoAssignVictory(gameIndex)),
            ("Manually Assign Victory",() => ManuallyAssignVictory(gameIndex))
        };
        ConsoleOptionsAndFunction(null, editPlayersValueAction);
    }

    public static void AutoAssignVictory(int gameIndex)
    {
        bool GoodWins = true;
        QuestGame currentGame = games[gameIndex];
        if (currentGame.HasFinalQuestOrHunt)
        {
            if (currentGame.HunterSuccessful ?? false)
            {
                GoodWins = false;
            }
            else if (!currentGame.GoodLastChanceSuccessful ?? false)
            {
                GoodWins = false;
                // This line looks complicated, its just asking, for all the roundleaders, how many were evil? if all of them, then good wins IF good's last chance happens (even if unsuccessful)
                if (games[gameIndex].Players.Where(p => p.Role.IsEvil()).Select(p => p.PlayerID).Intersect(games[gameIndex].RoundLeaders).Count() == games[gameIndex].RoundLeaders.Count)
                {
                    GoodWins = true;
                }
            }
        }

        if (GoodWins)
        {
            currentGame.Players.Where(p => p.Role.IsGood()).ToList().ForEach(p => p.Victory = Victory.Full);
            currentGame.Players.Where(p => p.Role.IsEvil()).ToList().ForEach(p => p.Victory = Victory.None);
            currentGame.Players.Where(p => !p.Role.IsEvil() && !p.Role.IsGood()).ToList().ForEach(p => p.Victory = null);
        }
        else
        {
            currentGame.Players.Where(p => p.Role.IsGood()).ToList().ForEach(p => p.Victory = Victory.None);
            currentGame.Players.Where(p => p.Role.IsEvil()).ToList().ForEach(p => p.Victory = Victory.Full);
            currentGame.Players.Where(p => !p.Role.IsEvil() && !p.Role.IsGood()).ToList().ForEach(p => p.Victory = null);
        }
    }

    static readonly List<QuestRole> Good = new List<QuestRole>{
            QuestRole.LoyalServantOfArthur,
            QuestRole.Duke,
            QuestRole.Archduke,
            QuestRole.Cleric,
            QuestRole.Troublemaker,
            QuestRole.Youth,
            QuestRole.Apprentice,
            QuestRole.Arthur
        };
    public static bool IsGood(this QuestRole? role)
    {
        if (role == null) { return false; }
        return Good.Contains((QuestRole)role);
    }

    static readonly List<QuestRole> Evil = new List<QuestRole>{
            QuestRole.MinionOfMordred,
            QuestRole.MorganLeFey,
            QuestRole.Scion,
            QuestRole.Changeling,
            QuestRole.BlindHunter,
            QuestRole.Brute,
            QuestRole.Lunatic,
            QuestRole.Trickster,
            QuestRole.Revealer,
        };

    public static bool IsEvil(this QuestRole? role)
    {
        if (role == null) { return false; }
        return Evil.Contains((QuestRole)role);
    }

    public static void ManuallyAssignVictory(int gameIndex)
    {
        foreach (QuestPlayer player in games[gameIndex].Players)
        {
            Console.WriteLine($"{player.Role} : {player.PlayerID} : {player.Victory}");
            Console.WriteLine("Change player's victory status? (y/n)");
            if (InputYesNo())
            {
                Console.WriteLine("Player's new victory status, (F)ull, (P)artial, (N)one.");
                char playerInput = InputChar(new char[]{'f','p','n'});
                if (playerInput == 'f')
                {
                    player.Victory = Victory.Full;
                }
                else if (playerInput == 'p')
                {
                    player.Victory = Victory.Partial;
                }
                else if (playerInput == 'n')
                {
                    player.Victory = Victory.None;
                }
                else
                {
                    throw new UnreachableException("Invalid Return Character for PlayerInput");
                }
            }
        }
    }

    public static void EditNotes(int gameIndex)
    {
        List<(string, Action)> editPlayersValueAction = new List<(string, Action)>{
            ("Add Line to Notes",() => AddLineToNotes(gameIndex)),
            ("Clear Notes", () => ClearNotes(gameIndex))
        };

        ConsoleOptionsAndFunction(() =>
        {
            Console.Write(PadTruc("Notes " + DASH, 64));
            Console.WriteLine(games[gameIndex].Notes);
            Console.WriteLine(DASH);
        }, editPlayersValueAction);
    }

    public static void AddLineToNotes(int gameIndex)
    {
        Console.Write(">> ");
        string? stringInput = Console.ReadLine();

        if (stringInput != null)
        {
            games[gameIndex].Notes += $"\n{stringInput}";
        }
    }

    public static void ClearNotes(int gameIndex)
    {
        Console.WriteLine("Confirm Deletion of Notes (y/n)");
        if (InputYesNo())
        {
            games[gameIndex].Notes = "";
        }
    }



    public static void MakeNewGameConfig()
    {
        QuestGameConfig newConfig = new QuestGame.QuestGameConfig();

        Console.WriteLine("Input Name of Config");
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        while (rawInput == null || rawInput == "")
        {
            Console.WriteLine("Invalid Input. (cannot be empty)");
            Console.Write(">> ");
            rawInput = Console.ReadLine();
        }
        newConfig.Name = rawInput;

        Console.WriteLine("Input Game Type --> (D)efault, Directors(C)ut, (M)odified, (U)nspecified");
        char inputChar = InputChar(new char[]{'d','c','m','u'});

        switch (inputChar)
        {
            case 'd':
                newConfig.Type = GameType.Default;
                break;
            case 'c':
                newConfig.Type = GameType.DirectorsCut;
                break;
            case 'm':
                newConfig.Type = GameType.Modified;
                break;
            case 'u':
            default:
                newConfig.Type = GameType.Unspecified;
                break;
        }


        Console.WriteLine("Input # of players");
        Console.Write(">> ");
        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || choice <= 0)
        {
            Console.WriteLine("Invalid Input");
            Console.Write(">> ");
        }

        string[] roleNames = Enum.GetNames(typeof(QuestGame.QuestRole));
        for (int i = 0; i < roleNames.Length; i++)
        {
            Console.WriteLine($"{i} - {roleNames[i]}");
        }
        Console.Write(">> ");
        rawInput = Console.ReadLine();
        List<string> tokens = TokenizeRawInput(rawInput, "[^0-9]");

        foreach (string token in tokens)
        {
            newConfig.Roles.Add((QuestGame.QuestRole)int.Parse(token));
        }

        configs.Add(newConfig);
        UpdateJsonFiles();
    }

    public static List<string> TokenizeRawInput(string? rawInput, string regexReplace)
    {
        if (rawInput == null)
        {
            return new List<string>();
        }

        List<string> tokens = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList(); // tokenize by space

        for (var i = 0; i < tokens.Count; i++)
        {
            tokens[i] = Regex.Replace(tokens[i], regexReplace, "");
        }
        Console.Write("Tokens: ");
        tokens.ForEach(p => Console.Write($"{p}, "));
        Console.WriteLine();

        return tokens;
    }

    public static bool InputYesNo()
    {
        Console.Write(">> ");
        ConsoleKeyInfo keyChoice = Console.ReadKey();
        Console.WriteLine();
        if (keyChoice.KeyChar.ToString().ToLower() == "y")
        {
            return true;
        }
        else if (keyChoice.KeyChar.ToString().ToLower() == "n" || keyChoice.Key == ConsoleKey.Escape)
        {
            return false;
        }
        else
        {
            Console.WriteLine("Invalid Input");
            return InputYesNo();
        }
    }

    public static char InputChar(char[] inputChars)
    {
        Console.Write(">> ");
        ConsoleKeyInfo keyChoice = Console.ReadKey();
        Console.WriteLine();
        if (inputChars.Contains(char.ToLower(keyChoice.KeyChar)))
        {
            return char.ToLower(keyChoice.KeyChar);
        }
        else
        {
            Console.WriteLine("Invalid Input");
            return InputChar(inputChars);
        }
    }



    private static string PadTruc(object? val, int length, bool alignLeft = false)
    {   
        string valStr = val?.ToString() ?? "";
        if (alignLeft)
        {
            return valStr.Length > length ? valStr.Substring(0, length) : valStr.PadRight(length, ' ');
        }
        return valStr.Length > length ? valStr.Substring(0, length) : valStr.PadLeft(length, ' ');
    }

    public static List<QuestGame> QuestGamesToCSharpObjects()
    {
        string jsonString = File.ReadAllText(pathGames);
        var root = JsonConvert.DeserializeObject<List<QuestGame>>(JObject.Parse(jsonString)["games"]!.ToString());
        return root!;
    }

    public static List<QuestGame.QuestGameConfig> QuestConfigsToCSharpObjects()
    {
        string jsonString = File.ReadAllText(pathConfigs);
        var root = JsonConvert.DeserializeObject<List<QuestGame.QuestGameConfig>>(JObject.Parse(jsonString)["configs"]!.ToString());
        return root!;
    }

    public static string QuestGamesToJson()
    {
        JObject key = new JObject
        {
            ["games"] = JToken.FromObject(games)
        };
        return key.ToString();
    }

    public static string QuestConfigsToJson()
    {
        JObject key = new JObject
        {
            ["configs"] = JToken.FromObject(configs)
        };
        return key.ToString();
    }

    public static void UpdateJsonFiles()
    {
        try
        {
            File.WriteAllText(pathGames, QuestGamesToJson());
            File.WriteAllText(pathConfigs, QuestConfigsToJson());
            Console.WriteLine("File successfully updated.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating file: {e.Message}");
        }
    }

    public static void CheckForGameInconsistencies(int gameIndex)
    {
        //check for if leader and amulet observers overlap
        WriteWarning("WARNING: TEST WARNING");
        //ensure that everyone has proper victory value 

        //ensure that final quest does not occur if good wins with 3 wins

        //ensure that if final quest happens that either hunterSuccessful or goodsLastChance (but not both) are not null

        //someone does not have a role

        //game is incomplete

        //duplicate leadership or amulet-ship

        //leadership or amulet-ship person does not appear in the game

        //someone has a role not in the config

        //more leaders than their are rounds

        //more or less amulet's then their should be (need knowledge about boards)
        //notes should highlight why a warning is acceptable (say extra amulet or amulet in different location/quest) otherwise assume basic set up
        //if game type is directors cut or default, this check should check, otherwise, don't

        //amulet observer/observed count mismatch

        //no hunter to hunt (yet the hunterSuccessful is still not null)

        //invalid quest/finalquest games state (if 3 good quests but still final quest, or finalquest but not 3 bad wins, or no final quest with 3 bad wins)

        //if game looks correct, green "valid game" should appear
        //notes should specify something special if modified game is selected
    }
    
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteQuestGame(QuestGame questGame)
    {
        Console.WriteLine(KeyValuePadTruc64("Time",questGame.Time));
        Console.WriteLine(KeyValuePadTruc64("Roles",""));
        foreach (QuestPlayer player in questGame.Players)
        {
            Console.Write(PadTruc("",15));
            if (player.Role.IsGood())
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (player.Role.IsEvil())
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write(PadTruc(player.Role,15) + ": " + PadTruc(player.PlayerID,8,true));

            if (questGame.AmuletObservations.Select(p => p.Item1).Contains(player.PlayerID))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($" ({PadTruc(questGame.AmuletObservations.Select(p => p.Item1).ToList().IndexOf(player.PlayerID),1)})");
            }
            else
            {
                Console.Write(PadTruc("",4));
            }

            if (questGame.AmuletObservations.Select(p => p.Item2).Contains(player.PlayerID))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write($"({PadTruc(questGame.AmuletObservations.Select(p => p.Item2).ToList().IndexOf(player.PlayerID),1)})");
            }
            else
            {
                Console.Write(PadTruc("",3));
            }

            Console.ResetColor();
            if (questGame.RoundLeaders.Contains(player.PlayerID))
            {
                Console.Write($" {PadTruc(questGame.RoundLeaders.IndexOf(player.PlayerID)+1,2,true)}");
            }
            else
            {
                Console.Write(PadTruc("",3));
            }

            switch (player.Victory)
            {
                case Victory.Full:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(PadTruc("| + ",11,true));
                    break;
                case Victory.Partial:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(PadTruc("| ~ ",11,true));
                    break;
                case Victory.None:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(PadTruc("| x ",11,true));
                    break;
                case null:
                    Console.ResetColor();
                    Console.WriteLine(PadTruc("| ? ",11,true));
                    break;
            }
            Console.ResetColor();
        }
        Console.Write(PadTruc("Quests: [ ", 22-5));
        foreach (int r in questGame.RoundWins)
        {
            if (r == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("@ ");
            }
            else if (r == 0)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("@ ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("@ ");
            }
        }
        Console.ResetColor();
        Console.Write("]");
        if (questGame.HasFinalQuestOrHunt)
        {
            Console.Write(" ---> ");
            if (questGame.HunterSuccessful != null)
            {
                if ((bool)questGame.HunterSuccessful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("@ ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("@ ");
                }
            }
            else
            {
                Console.Write("o ---> ");
                if (questGame.GoodLastChanceSuccessful == null)
                {
                    //this is an invalid state for the game to be in, it can't be that the it has a final quest, but huntersuccessful and goodlastchance be null
                    Console.Write("???");
                }
                else
                {
                    if ((bool)questGame.GoodLastChanceSuccessful)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("@");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("@");
                    }
                }
            }


        }
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(KeyValuePadTruc64("Unassigned","["));
        List<QuestPlayer> nonNullRoles = questGame.Players.Where(p => p.Role != null).ToList();
        List<QuestRole> unassignedRoles = questGame.Config.Roles.Except(nonNullRoles.Select(p => p.Role ?? (QuestRole)(-1))).ToList();
        foreach (QuestRole role in unassignedRoles)
        {
            Console.WriteLine(PadTruc("",15) + PadTruc(role,44,true));
        }
        Console.WriteLine(PadTruc("",15) + "]");
        Console.Write(KeyValuePadTruc64("Notes",""));
        string notesString = questGame.Notes.Replace("\n","\n" + PadTruc("",15));
        Console.Write(notesString);
        Console.WriteLine();
    }

    public static string KeyValuePadTruc64(object key, object value)
    {
        return PadTruc($"{key}: ", 15) + PadTruc(value,49,true);
    }
}