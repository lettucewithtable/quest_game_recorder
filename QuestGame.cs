

using System.Runtime.CompilerServices;

public class QuestGame
{
    public List<QuestPlayer> Players { get; set; } = new List<QuestPlayer>();
    public List<RoundWin> RoundWins { get; set; } = new List<RoundWin>();
    public List<QuestPlayer> RoundLeaders { get; set; } = new List<QuestPlayer>();
    public bool HasFinalQuest { get; set; } = false;
    public bool? HunterSuccessful { get; set; } = null;
    public bool? GoodLastChanceSuccessful { get; set; } = null;

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

    