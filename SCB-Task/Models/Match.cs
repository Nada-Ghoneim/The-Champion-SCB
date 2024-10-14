namespace SCB.Models
{
    public class Match
    {
        public int Player1 { get; set; }
        public int Player2 { get; set; }
        public int? Winner { get; set; }
        public int RoundID { get; set; }
        public DateTime MatchTime { get; set; }
        public int GroupNum { get; set; } // Ensure this property is defined
    }
}