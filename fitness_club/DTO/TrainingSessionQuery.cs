namespace fitness_club.DTO
{
    public class TrainingSessionQuery
    {
        public string? SearchTerm { get; set; }

        public int? TrainerId { get; set; }
        public string? Specialization { get; set; }
        public DateTime? StartTimeFrom { get; set; }
        public DateTime? StartTimeTo { get; set; }
        public int? StartTimeHour { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SessionType { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? FitnessGoal { get; set; }
        public decimal? MinTrainerRating { get; set; }
        public bool? HasAvailableSpots { get; set; }
        public string? Location { get; set; }
        public bool? IsActive { get; set; }

        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}