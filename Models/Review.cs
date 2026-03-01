namespace Car_Project.Models
{
    public class Review : BaseEntity
    {
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;

        /// <summary>1–5 aras? reytinq</summary>
        public int Rating { get; set; } = 5;

        public bool IsApproved { get; set; }
    }
}
