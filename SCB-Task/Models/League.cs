namespace SCB.Models
{
    public class League
    {
        public string Name { get; set; }
        public int? Champion { get; set; }  // Nullable if no champion is set
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
