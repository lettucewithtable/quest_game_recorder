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
        while (true)
        {   
            Console.WriteLine(
@"(1) New Game
(2) List Games
(3) New Game Configuration
(4) Game Analysis
(5) Exit");
            Console.Write(">> ");
            ConsoleKeyInfo choice = Console.ReadKey();
            Console.WriteLine();
            int choiceInt;
            if (int.TryParse(choice.KeyChar.ToString(), out choiceInt))
            {
                switch (choiceInt)
                {
                    case 1:     // New Game
                        RecordNewGame();
                        break;
                    case 2:     // List Games
                        ListAndEditGames();
                        break;
                    case 3:     // New Game Configuration
                        MakeNewGameConfig();
                        break;
                    case 4:     // Game Analysis
                        break;
                    case 5:     // Exit
                        return;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
                UpdateJsonFiles();
            }
            else if (choice.Key == ConsoleKey.Escape)
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
            }
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
        if (games.Count != 0)
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
            Console.WriteLine($"Game {i}: ----------------------------------------------------------------");
            Console.WriteLine(games[i]);
            Console.WriteLine();
        }

        Console.Write(">> ");
        string? stringInput = Console.ReadLine();
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
        while (true)
        {   
            Console.WriteLine(
@"(1) Players
(2) Leadership
(3) Assign Players to Roles
(4) Round Wins
(5) Final Quest
(6) Victory
(7) Notes
(8) Back");
            Console.Write(">> ");
            ConsoleKeyInfo choice = Console.ReadKey();
            Console.WriteLine();
            int choiceInt;
            if (int.TryParse(choice.KeyChar.ToString(), out choiceInt))
            {
                switch (choiceInt)
                {
                    case 1:     
                        EditPlayers(gameIndex);
                        break;
                    case 2:     
                        
                        break;
                    case 3:     
                        
                        break;
                    case 4:     
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:     
                        return;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
                UpdateJsonFiles();
            }
            else if (choice.Key == ConsoleKey.Escape)
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
            }
        }
    }

    // Add and remove players
    public static void EditPlayers(int gameIndex)
    {
        while (true)
        {
            Console.WriteLine(
@"(1) Add Players
(2) Remove Players
(3) Back");
            Console.Write(">> ");
            ConsoleKeyInfo choice = Console.ReadKey();
            Console.WriteLine();
            int choiceInt;
            if (int.TryParse(choice.KeyChar.ToString(), out choiceInt))
            {
                switch (choiceInt)
                {
                    case 1:
                        AddPlayers(gameIndex);
                        break;
                    case 2:

                        break;
                    case 3:
                        return;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
                UpdateJsonFiles();
            }
            else if (choice.Key == ConsoleKey.Escape)
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
            }
        }
    }

    public static void AddPlayers(int gameIndex)
    {
        QuestGame currentGame = games[gameIndex];

        Console.Write("Input Players: ");
        string? rawInput = Console.ReadLine();
        Console.WriteLine();

        List<string> tokens = TokenizeRawInput(rawInput, "[^a-zA-Z]");

        List<string> namesToBeAdded = AliasesToNames(tokens);

        Console.Write("NamesToBeAdded: ");
        namesToBeAdded.ForEach(p => Console.Write($"{p}, "));
        Console.WriteLine();

        currentGame.Players.Add
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