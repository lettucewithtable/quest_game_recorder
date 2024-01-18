using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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
            Console.WriteLine($"{i} - {configs[i]}");
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
                Console.WriteLine("WARNING: Player Count Mismatch");
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
            Console.WriteLine(games[i]);
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
            ("Round Wins",() => InputRounds(gameIndex)),
            ("Final Quest",() => InputFinalQuest(gameIndex)),
            ("Assign Players to Roles",() => AssignPlayersToRoles(gameIndex)),
            ("Victory",() => EditVictory(gameIndex)),
            ("Notes",() => EditNotes(gameIndex)),
        };
        ConsoleOptionsAndFunction(() =>
            {
                Console.WriteLine(games[gameIndex]);
                Console.WriteLine(DASH);
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
        List<string> playerIDs = AliasesToNames(TokenizeRawInput(rawInput, "[^a-zA-Z]"));
        games[gameIndex].RoundLeaders = playerIDs;
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
        Console.WriteLine("Did the Final Quest Begin (5 mins of talking)? (y/n)");
        games[gameIndex].HasFinalQuest = InputYesNo();

        if (!games[gameIndex].HasFinalQuest)
        {
            games[gameIndex].HunterSuccessful = null;
            games[gameIndex].GoodLastChanceSuccessful = null;
            return;
        }

        Console.WriteLine("Was the Hunter Successful? (y/n)");
        games[gameIndex].HunterSuccessful = InputYesNo();

        if ((bool)games[gameIndex].HunterSuccessful!)
        {
            games[gameIndex].GoodLastChanceSuccessful = null;
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
                    Console.WriteLine($"{names[0]} (y/n)");
                    var player = games[gameIndex].Players.Where(p => p.PlayerID == names[0]).ToList();
                    if (player.Count > 1)
                    {
                        Console.WriteLine("WARNING: Duplicate Player ID");
                    }

                    if (player[0].Role != null)
                    {
                        Console.WriteLine("WARNING: Player already has Role");
                    }
                    if (InputYesNo())
                    {


                        player[0].Role = role;
                        break;
                    }
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
        if (currentGame.HasFinalQuest)
        {
            if (currentGame.HunterSuccessful ?? false)
            {
                GoodWins = false;
            }
            else if (!currentGame.GoodLastChanceSuccessful ?? false)
            {
                GoodWins = false;
            }
        }

        if (GoodWins)
        {
            currentGame.Players.ForEach(p => p.Victory = p.Role.IsGood() ? Victory.Full : Victory.None);
        }
        else
        {
            currentGame.Players.ForEach(p => p.Victory = p.Role.IsEvil() ? Victory.Full : Victory.None);
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
            QuestRole.Brute,
            QuestRole.Lunatic,
            QuestRole.Trickster,
            QuestRole.Revealer
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
        QuestGame.QuestGameConfig newConfig = new QuestGame.QuestGameConfig();

        Console.Write("# of players: ");
        int choice;
        if (int.TryParse(Console.ReadLine(), out choice) && 0 < choice)
        {
            newConfig.NumberOfPlayers = choice;
        }
        else
        {
            Console.WriteLine("Invalid Number");
            MakeNewGameConfig();
            return;
        }

        string[] roleNames = Enum.GetNames(typeof(QuestGame.QuestRole));
        for (int i = 0; i < roleNames.Length; i++)
        {
            Console.WriteLine($"{i} - {roleNames[i]}");
        }
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> tokens = TokenizeRawInput(rawInput, "[^0-9]");

        foreach (string token in tokens)
        {
            newConfig.Roles.Add((QuestGame.QuestRole)int.Parse(token));
        }

        configs.Add(newConfig);
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



    private static string PadTruc(string val, int length, bool alignLeft = false)
    {
        if (alignLeft)
        {
            return val.Length > length ? val.Substring(0, length) : val.PadRight(length, ' ');
        }
        return val.Length > length ? val.Substring(0, length) : val.PadLeft(length, ' ');
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
}