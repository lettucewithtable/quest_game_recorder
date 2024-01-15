using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

static class Program
{
    static string pathGames = "games.json";
    static string pathNames = "namesandalias.json";
    static List<QuestGame> games = new List<QuestGame>();
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

        games = QuestGamesToCSharpObjects();


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
(4) Game Analysis");
            Console.Write(">> ");
            string? choice = Console.ReadLine();
            int choiceInt;
            if (int.TryParse(choice, out choiceInt))
            {
                switch (choiceInt)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
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

    public static string QuestGamesToJson()
    {
        JObject key = new JObject
        {
            ["games"] = JToken.FromObject(games)
        };
        return key.ToString();
    }

    public static void UpdateJsonFile()
    {
        try
        {
            File.WriteAllText(pathGames, QuestGamesToJson());
            Console.WriteLine("File successfully updated.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating file: {e.Message}");
        }
    }
}