

using System.Runtime.CompilerServices;

public class QuestGame
{
    public DateTime Time { get; set; } = DateTime.Now;
    public List<QuestPlayer> Players { get; set; } = new List<QuestPlayer>();
    public List<RoundWin> RoundWins { get; set; } = new List<RoundWin>();
    public List<QuestPlayer> RoundLeaders { get; set; } = new List<QuestPlayer>();
    public bool HasFinalQuest { get; set; } = false;
    public bool? HunterSuccessful { get; set; } = null;
    public bool? GoodLastChanceSuccessful { get; set; } = null;
    public QuestGameConfig Config { get; set; } = new QuestGameConfig();

    public class QuestPlayer
    {
        public string PlayerID { get; set; } = "";
        public Role Role { get; set; } = Role.LoyalServantOfArthur;
        public bool? DidWin { get; set; } = null;
    }


    public enum Role
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

}

public class QuestGameConfig
{
    public int NumberOfPlayers { get; set; } = 0;
    public List<QuestGame.Role> Roles { get; set; } = new List<QuestGame.Role>();

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

    private static string PadTruc(string val, int length, bool alignRight = true)
    {
        if (alignRight)
        {
            return val.Length > length ? val.Substring(0,length) : val.PadLeft(length,' ');
        }
        return val.Length > length ? val.Substring(0,length) : val.PadRight(length,' ');
    }
}

    