using System;
using Newtonsoft.Json;

namespace TraktToPlex.Plex.Models
{
    public class Section
    {
        [JsonProperty("key")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("composite")]
        public string Composite { get; set; }

        [JsonProperty("filters")]
        public bool Filters { get; set; }

        [JsonProperty("refreshing")]
        public bool Refreshing { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("agent")]
        public string Agent { get; set; }

        [JsonProperty("scanner")]
        public string Scanner { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("scannedAt")]
        public long ScannedAt { get; set; }

        [JsonProperty("content")]
        public bool Content { get; set; }

        [JsonProperty("directory")]
        public bool DirectoryDirectory { get; set; }

        [JsonProperty("contentChangedAt")]
        public long ContentChangedAt { get; set; }

        [JsonProperty("Location")]
        public Location[] Location { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
