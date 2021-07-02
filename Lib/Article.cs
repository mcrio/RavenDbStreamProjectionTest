namespace Lib
{
    public class Article
    {
        public Article(string title, string description, bool isDeleted)
        {
            Title = title;
            Description = description;
            IsDeleted = isDeleted;
        }

        public string Id { get; private set; } = string.Empty;

        public string Title { get; private set; }

        public string Description { get; private set; }

        public bool IsDeleted { get; private set; }
    }
}