using Cassie;

namespace CustomFramework.CustomTeams
{
    public class TeamCassieSubtitled : TeamCassieBase
    {
        public string Announcement { get; set; }
        public string Subtitles { get; set; }

        public TeamCassieSubtitled(string announcement, string subtitle)
        {
            Announcement = announcement;
            Subtitles = subtitle;
        }

        public override CassieTtsPayload GetPayload()
        {
            return new CassieTtsPayload(Announcement, Subtitles);
        }
    }
}
