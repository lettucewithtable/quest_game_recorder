using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

static class Program
{
    static string pathGames = "games.json";
    static string pathNames = "namesandalias.json";
    static string pathConfigs = "configs.json";
    static List<QuestGame> games = new List<QuestGame>();
    static List<QuestGameConfig> configs = new List<QuestGameConfig>();
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
                        break;
                    case 3:     // New Game Configuration
                        MakeNewGameConfig();
                        UpdateJsonFiles();
                        break;
                    case 4:     // Game Analysis
                        break;
                    case 5:     // Exit
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
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
        
    }


    public static void MakeNewGameConfig()
    {
        QuestGameConfig newConfig = new QuestGameConfig();

        Console.Write("# of players: ");
        int choice;
        if (int.TryParse(Console.ReadLine(), out choice))
        {
            newConfig.NumberOfPlayers = choice;
        }
        
        string[] roleNames = Enum.GetNames(typeof(QuestGame.Role));
        for(int i = 0; i < roleNames.Length; i++)
        {
            Console.WriteLine($"{i} - {roleNames[i]}");
        }
        Console.Write(">> ");
        string? rawInput = Console.ReadLine();
        List<string> tokens = TokenizeRawInput(rawInput, "[^0-9]");

        foreach (string token in tokens)
        {
            newConfig.Roles.Add((QuestGame.Role) int.Parse(token));
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
        string jsonString = System.IO.File.ReadAllText(pathGames);
        var root = JsonConvert.DeserializeObject<List<QuestGame>>(JObject.Parse(jsonString)["games"]!.ToString());
        return root!;
    }

    public static List<QuestGameConfig> QuestConfigsToCSharpObjects()
    {
        string jsonString = System.IO.File.ReadAllText(pathConfigs);
        var root = JsonConvert.DeserializeObject<List<QuestGameConfig>>(JObject.Parse(jsonString)["configs"]!.ToString());
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