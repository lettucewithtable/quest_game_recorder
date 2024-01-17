

using System.Runtime.CompilerServices;

public class QuestGame
{
    public DateTime Time { get; set; } = DateTime.Now;
    public List<QuestPlayer> Players { get; set; } = new List<QuestPlayer>();
    public List<RoundWin> RoundWins { get; set; } = new List<RoundWin>();
    public List<string> RoundLeaders { get; set; } = new List<string>();
    public bool HasFinalQuest { get; set; } = false;
    // only have a bool value if HasFinalQuest and hunt is initiated
    public bool? HunterSuccessful { get; set; } = null;
    // only have a bool value if HasFinalQuest and hunt is not initiated
    public bool? GoodLastChanceSuccessful { get; set; } = null;
    public QuestGameConfig Config { get; set; } = new QuestGameConfig();
    public string Notes { get; set; } = "";


    public override string ToString()
    {
        string playersString = "\n";

        foreach (QuestPlayer player in Players)
        {
            playersString += $"    {player}\n";
        }

        string roundWinString = "";

        foreach (RoundWin rw in RoundWins)
        {
            roundWinString += $"{rw} ";
        }

        string questLeaders = "";

        foreach (string leader in RoundLeaders)
        {
            questLeaders += $"{leader} ";
        }

        string notesString = Notes.Replace("\n","\n    ");

        return
@$"DateTime: {Time}
Players: {playersString}
RoundWins: {roundWinString}
QuestLeaders: {questLeaders}
HasFinalQuest: {HasFinalQuest}
HunterSuccessful: {HunterSuccessful}
GoodLastChanceSuccessful: {GoodLastChanceSuccessful}
Notes: {notesString}";
    }

    public class QuestPlayer
    {
        public string PlayerID { get; set; } = "";
        public QuestRole? Role { get; set; } = null;
        public Victory? Victory { get; set; } = null;

        public QuestPlayer() {}
        public override string ToString()
        {
            string didWinText = Victory != null ? Victory.Value.ToString() : string.Empty;

            return $"{Role}: {PlayerID} | " + didWinText;
        }
    }

    public enum QuestRole
    {
        LoyalServantOfArthur,
        MinionOfMordred,
        MorganLeFey,
        Scion,
        Changeling,
        Duke,
        Archduke,
        BlindHunter,
        Cleric,
        Troublemaker,
        Youth,
        Apprentice,
        Brute,
        Lunatic,
        Mutineer,
        Arthur,
        Trickster,
        Revealer
    }

    public enum RoundWin
    {
        Good,
        Evil,
    }

    public enum Victory
    {
        Full,
        Partial,
        None
    }


    public class QuestGameConfig
    {
        public int NumberOfPlayers { get; set; } = 0;
        public List<QuestRole> Roles { get; set; } = new List<QuestRole>();

        public override string ToString()
        {
            var groupBy = this.Roles.GroupBy(role => role).ToDictionary(group => group.Key, group => group.Count());

            string returnVal = $"{this.NumberOfPlayers} |";

            foreach (var group in groupBy)
            {
                returnVal += $" x{group.Value} {group.Key}";
            }

            return returnVal;
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
}



