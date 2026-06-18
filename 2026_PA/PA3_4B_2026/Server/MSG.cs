namespace Server
{
    public class MSG
    {
        public enum MessageType {SEARCH, SEARCHRESULT, DETAIL, DETAILRESULT}
        public String? Search {  get; set; }
        public String? Sex { get; set; }
        public List<String>? Names { get; set; }
        public List<DataModels.Babyname>? Details { get; set; }
        public List<String>? AlternativeNames { get; set; }

        public MessageType type { get; set; }
    }
}
