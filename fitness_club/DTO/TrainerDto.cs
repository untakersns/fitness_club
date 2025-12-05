namespace fitness_club.DTO
{
    public class TrainerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Certification { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public decimal Rating { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime HireDate { get; set; }
    }
}
