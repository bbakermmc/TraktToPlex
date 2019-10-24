using System;
using Newtonsoft.Json;

namespace TraktToPlex.Plex.Models.Shows
{
    public class Episode : IHasId
    {
        [JsonProperty("index")]
        public int No { get; set; }

        [JsonProperty("viewCount")]
        public int ViewCount { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("parentRatingKey")]
        public long ParentRatingKey { get; set; }

        [JsonProperty("grandparentRatingKey")]
        public long GrandparentRatingKey { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("parentGuid")]
        public string ParentGuid { get; set; }

        [JsonProperty("grandparentGuid")]
        public string GrandparentGuid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleSort")]
        public string TitleSort { get; set; }

        [JsonProperty("grandparentKey")]
        public string GrandparentKey { get; set; }

        [JsonProperty("parentKey")]
        public string ParentKey { get; set; }

        [JsonProperty("grandparentTitle")]
        public string GrandparentTitle { get; set; }

        [JsonProperty("parentTitle")]
        public string ParentTitle { get; set; }

        [JsonProperty("contentRating")]
        public string ContentRating { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("parentIndex")]
        public long ParentIndex { get; set; }

        [JsonProperty("lastViewedAt")]
        public long? LastViewedAt { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("parentThumb")]
        public string ParentThumb { get; set; }

        [JsonProperty("grandparentThumb")]
        public string GrandparentThumb { get; set; }

        [JsonProperty("grandparentArt")]
        public string GrandparentArt { get; set; }

        [JsonProperty("grandparentTheme")]
        public string GrandparentTheme { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("originallyAvailableAt")]
        public DateTimeOffset OriginallyAvailableAt { get; set; }

        [JsonProperty("addedAt")]
        public long AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        public string Id { get; set; }
    }
}