namespace MGBlazorApp.Models
{
    public enum MoodType
    {
        Happy,
        Sad,
        Energetic,
        Calm,
        Angry,
        Romantic,
        Nostalgic,
        Focused,
        Party,
        Chill
    }

    public class Mood
    {
        public MoodType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<string> AssociatedGenres { get; set; } = new();
        public Dictionary<string, float> AudioFeatures { get; set; } = new();
    }

    public static class MoodData
    {
        public static List<Mood> GetAllMoods()
        {
            return new List<Mood>
            {
                new Mood
                {
                    Type = MoodType.Happy,
                    Name = "Happy",
                    Description = "Feeling joyful and upbeat",
                    Emoji = "😊",
                    Color = "#FFD700",
                    AssociatedGenres = new List<string> { "pop", "funk", "disco", "dance" },
                    AudioFeatures = new Dictionary<string, float> { {"valence", 0.8f}, {"energy", 0.7f}, {"danceability", 0.8f} }
                },
                new Mood
                {
                    Type = MoodType.Sad,
                    Name = "Sad",
                    Description = "Feeling melancholic or blue",
                    Emoji = "😢",
                    Color = "#4682B4",
                    AssociatedGenres = new List<string> { "blues", "indie", "alternative", "acoustic" },
                    AudioFeatures = new Dictionary<string, float> { {"valence", 0.2f}, {"energy", 0.3f}, {"acousticness", 0.7f} }
                },
                new Mood
                {
                    Type = MoodType.Energetic,
                    Name = "Energetic",
                    Description = "Full of energy and ready to move",
                    Emoji = "⚡",
                    Color = "#FF6347",
                    AssociatedGenres = new List<string> { "electronic", "rock", "hip-hop", "dance" },
                    AudioFeatures = new Dictionary<string, float> { {"energy", 0.9f}, {"danceability", 0.8f}, {"tempo", 0.8f} }
                },
                new Mood
                {
                    Type = MoodType.Calm,
                    Name = "Calm",
                    Description = "Peaceful and relaxed",
                    Emoji = "🧘",
                    Color = "#98FB98",
                    AssociatedGenres = new List<string> { "ambient", "classical", "new-age", "lo-fi" },
                    AudioFeatures = new Dictionary<string, float> { {"energy", 0.2f}, {"valence", 0.5f}, {"acousticness", 0.8f} }
                },
                new Mood
                {
                    Type = MoodType.Romantic,
                    Name = "Romantic",
                    Description = "Love is in the air",
                    Emoji = "💕",
                    Color = "#FF69B4",
                    AssociatedGenres = new List<string> { "r&b", "soul", "jazz", "acoustic" },
                    AudioFeatures = new Dictionary<string, float> { {"valence", 0.6f}, {"energy", 0.4f}, {"acousticness", 0.6f} }
                },
                new Mood
                {
                    Type = MoodType.Party,
                    Name = "Party",
                    Description = "Ready to celebrate and have fun",
                    Emoji = "🎉",
                    Color = "#FF1493",
                    AssociatedGenres = new List<string> { "dance", "hip-hop", "pop", "electronic" },
                    AudioFeatures = new Dictionary<string, float> { {"energy", 0.9f}, {"danceability", 0.9f}, {"valence", 0.8f} }
                },
                new Mood
                {
                    Type = MoodType.Chill,
                    Name = "Chill",
                    Description = "Laid back and relaxed",
                    Emoji = "😎",
                    Color = "#20B2AA",
                    AssociatedGenres = new List<string> { "lo-fi", "chillout", "trip-hop", "indie" },
                    AudioFeatures = new Dictionary<string, float> { {"energy", 0.3f}, {"valence", 0.6f}, {"danceability", 0.5f} }
                }
            };
        }
    }
}