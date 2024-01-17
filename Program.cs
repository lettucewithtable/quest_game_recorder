using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            using (_ = File.CreateText(pathGames)) {} // The using block will automatically close the text file after creation
            
            
            JObject file = new JObject();
            file.Add("games", new JArray());
            File.WriteAllText(pathGames, file.ToString());
        }

        if (!File.Exists(pathNames))
        {   
            using (_ = File.CreateText(pathNames)) {}

            JObject file = new JObject();

            File.WriteAllText(pathNames, file.ToString());
        }
        
        if (!File.Exists(pathConfigs))
        {
            using (_ = File.CreateText(pathConfigs)) {}

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
        List<(string,Action)> mainMenuValueActions = new List<(string, Action)>{
            ("New Game",RecordNewGame),
            ("List Games",ListAndEditGames),
            ("New Game Configuration",MakeNewGameConfig),
            ("Game Analysis",new Action(() => throw new NotImplementedException()))
        };
        ConsoleOptionsAndFunction(null,mainMenuValueActions);
    }

    public static void ConsoleOptionsAndFunction(Action? runEachGo, List<(string,Action)> optionFunctionPairs)
    {   
        runEachGo?.Invoke();

        Console.WriteLine("Select (#) or (ESC):");
        for (int i = 0; i < optionFunctionPairs.Count; i++)
        {   
            Console.WriteLine($"({i+1}) {optionFunctionPairs[i].Item1}");
        }
        Console.Write(">> ");
        ConsoleKeyInfo choice = Console.ReadKey();
        Console.WriteLine();
        int choiceInt;
        if (int.TryParse(choice.KeyChar.ToString(), out choiceInt) && 0 < choiceInt && choiceInt <= optionFunctionPairs.Count)
        {
            Console.WriteLine(BLOCK);
            optionFunctionPairs[choiceInt-1].Item2.Invoke();
            Console.WriteLine();
            Console.WriteLine(BLOCK);
            ConsoleOptionsAndFunction(runEachGo,optionFunctionPairs);
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
            ConsoleOptionsAndFunction(runEachGo,optionFunctionPairs);
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


        int gameIndex = games.Count-1;
        if (games.Count != 1)
        {   
            while (true)
            {
                Console.WriteLine("Play with previous players? (y/n)");
                if (games[gameIndex].Config.NumberOfPlayers != games[gameIndex-1].Config.NumberOfPlayers)
                {
                    Console.WriteLine("WARNING: Player Count Mismatch");
                }
                Console.Write(">> ");
                ConsoleKeyInfo keyChoice = Console.ReadKey();
                Console.WriteLine();
                if (keyChoice.KeyChar.ToString().ToLower() == "y")
                {
                    for (int i = 0; i < games[gameIndex].Players.Count; i++)
                    {
                        games[gameIndex].Players[i].PlayerID = games[gameIndex-1].Players[i].PlayerID;
                    }
                    break;
                }
                else if (keyChoice.KeyChar.ToString().ToLower() == "n")
                {
                    break;
                }
                else
                {   
                    Console.WriteLine("Invalid Input");
                    continue;
                }
            }
        }

        EditGame(gameIndex);
    }


    public static void ListAndEditGames()
    {   
        for (int i = 0; i < games.Count; i++)
        {
            Console.WriteLine(PadTruc($"Game {PadTruc(i.ToString(),3)}: {DASH}",64));
            Console.WriteLine(games[i]);
            Console.WriteLine();
        }

        Console.Write(">> ");
        string? stringInput = Console.ReadLine();
        Console.WriteLine(BLOCK);
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
        List<(string,Action)> editGameValueActions = new List<(string, Action)>{
            ("Players",() => EditPlayers(gameIndex)),
            ("Leadership",() => throw new NotImplementedException()),
            ("Assign Players to Roles",() => throw new NotImplementedException()),
            ("Round Wins",() => throw new NotImplementedException()),
            ("Final Quest",() => throw new NotImplementedException()),
            ("Victory",() => throw new NotImplementedException()),
            ("Notes",() => throw new NotImplementedException()),
        };
        ConsoleOptionsAndFunction(() => {
                Console.WriteLine(games[gameIndex]);
                Console.WriteLine(DASH);
            }
            , editGameValueActions);
        UpdateJsonFiles();
    }

    // Add and remove players
    public static void EditPlayers(int gameIndex)
    {
        List<(string,Action)> editPlayersValueAction = new List<(string, Action)>{
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

            currentGame.Players.Add(new QuestGame.QuestPlayer{ PlayerID = name });
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
        games[gameIndex].Players = new List<QuestGame.QuestPlayer>();
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
        for(int i = 0; i < roleNames.Length; i++)
        {
            Console.WriteLine($"{i} - {roleNames[i]}");
        }
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> tokens = TokenizeRawInput(rawInput, "[^0-9]");

        foreach (string token in tokens)
        {
            newConfig.Roles.Add((QuestGame.QuestRole) int.Parse(token));
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
        tokens.ForEach(p => Console.Write($"{PadTruc(p,2,false)}, "));
        Console.WriteLine();

        tokens.Sort();
        return tokens;
    }

    
    private static string PadTruc(string val, int length, bool alignRight = true)
    {
        if (alignRight)
        {
            return val.Length > length ? val.Substring(0,length) : val.PadLeft(length,' ');
        }
        return val.Length > length ? val.Substring(0,length) : val.PadRight(length,' ');
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