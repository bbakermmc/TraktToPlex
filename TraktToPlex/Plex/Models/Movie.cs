using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TraktToPlex.Plex.Models
{
    public class Movie : IMediaItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ExternalProvider { get; set; }
        public string ExternalProviderId { get; set; }

        [JsonProperty("guid")]
        public string ExternalProviderInfo
        {
            get => null;
            set
            {
                var match = Regex.Match(value, @"\.(?<provider>[a-z]+)://(?<id>[^\?]+)");
                ExternalProvider = match.Groups["provider"].Value;
                ExternalProviderId = match.Groups["id"].Value;
            }
        }
        
        [JsonProperty("studio", NullValueHandling = NullValueHandling.Ignore)]
        public string Studio { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
        public double? Rating { get; set; }

        [JsonProperty("audienceRating", NullValueHandling = NullValueHandling.Ignore)]
        public double? AudienceRating { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("tagline", NullValueHandling = NullValueHandling.Ignore)]
        public string Tagline { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("art", NullValueHandling = NullValueHandling.Ignore)]
        public string Art { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Duration { get; set; }

        [JsonProperty("originallyAvailableAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? OriginallyAvailableAt { get; set; }

        [JsonProperty("addedAt")]
        public long AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("Genre")]
        public Collection[] Genre { get; set; }

        [JsonProperty("Director")]
        public Collection[] Director { get; set; }

        [JsonProperty("Writer", NullValueHandling = NullValueHandling.Ignore)]
        public Collection[] Writer { get; set; }

        [JsonProperty("Country", NullValueHandling = NullValueHandling.Ignore)]
        public Collection[] Country { get; set; }

        [JsonProperty("Role")]
        public Collection[] Role { get; set; }

        [JsonProperty("viewCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? ViewCount { get; set; }

        [JsonProperty("lastViewedAt", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastViewedAt { get; set; }

        public DateTime? LastViewedAtDateTime => LastViewedAt == null ? (DateTime?) null : UnixTimeStampToDateTime(LastViewedAt.Value);

        [JsonProperty("titleSort", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleSort { get; set; }

        [JsonProperty("originalTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalTitle { get; set; }

        [JsonProperty("Collection", NullValueHandling = NullValueHandling.Ignore)]
        public Collection[] Collection { get; set; }

        [NotMapped]
        public string ImdbId => ExternalProvider.ToLower() == "imdb" ? ExternalProviderId : null;
        
        private DateTime? UnixTimeStampToDateTime(long? unixTimeStamp)
        {
            if (unixTimeStamp == null) return null;
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp.Value)
                .ToLocalTime();
            return dtDateTime;
        }
    }

    public partial class Collection
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
    
}
