using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SendGrid;
using SendGrid.Helpers.Mail;
using TraktNet;
using TraktNet.Objects.Authentication;
using TraktNet.Objects.Basic;
using TraktNet.Objects.Get.Movies;
using TraktNet.Objects.Post.Syncs.History;
using TraktNet.Requests.Parameters;
using TraktToPlex.Plex;
using TraktToPlex.Plex.Models;

namespace TraktToPlex.Trakt
{
    public class TraktSync
    {
        private readonly string _emailApiKey;
        private readonly List<string> _log;
        private readonly string _plexClientSecret;

        private readonly string _plexKey;
        private readonly string _plexUrl;
        private readonly string _traktClientId;
        private readonly string _traktClientSecret;
        private readonly string _traktKey;
        private readonly string _emailSendTo;

        public TraktSync(string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId,
            string traktClientSecret, string emailApiKey, string emailSendTo)
        {
            _plexKey = plexKey;
            _plexUrl = plexUrl;
            _plexClientSecret = plexClientSecret;
            _traktKey = traktKey;
            _traktClientId = traktClientId;
            _traktClientSecret = traktClientSecret;
            _emailApiKey = emailApiKey;
            _emailSendTo = emailSendTo;

            _log = new List<string>();
        }

        public async Task SyncToTrakt()
        {
            var _plexClient = new PlexClient(_plexClientSecret);
            var _traktClient = new TraktClient(_traktClientId, _traktClientSecret);

            try
            {
                _plexClient.SetAuthToken(_plexKey);
                _plexClient.SetPlexServerUrl(_plexUrl);

                _traktClient.Authorization = TraktAuthorization.CreateWith(_traktKey);

                await MigrateMovies(_plexClient, _traktClient);
                //await MigrateTvShows(_plexClient, _traktClient);

                await SendEmail(_emailSendTo);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task MigrateMovies(PlexClient _plexClient, TraktClient _traktClient)
        {
            await ReportProgress("--------------------------------------------");
            await ReportProgress("Started syncing Movies!");
            await ReportProgress("--------------------------------------------");

            await ReportProgress("Importing Trakt movies..");
            var traktMovies =
                (await _traktClient.Sync.GetWatchedMoviesAsync(new TraktExtendedInfo().SetFull())).ToArray();
            await ReportProgress($"Found {traktMovies.Length} movies on Trakt");

            await ReportProgress("Importing Plex movies..");
            var plexMovies = await _plexClient.GetMovies();
            await ReportProgress($"Found {plexMovies.Length} movies on Plex");

            await ReportProgress("Going through all movies on Plex, to see if we find a match on Trakt..");
            var i = 0;
            var notMarkedMovies = new List<Movie>();
            foreach (var plexMovie in plexMovies.Where(x => x.ViewCount != null && x.ViewCount > 0))
            {
                i++;
                var traktMovie = traktMovies.FirstOrDefault(x => HasMatchingIdMovies(plexMovie, x.Ids));
                if (traktMovie == null) notMarkedMovies.Add(plexMovie);
            }

            var traktSyncHistoryPostMovies = new TraktSyncHistoryPostBuilder();

            foreach (var notMarkedMovie in notMarkedMovies)
            {
                if (notMarkedMovie.ImdbId == null)
                {
                    await ReportProgress($"{notMarkedMovie.Title} doesnt have an IMDB.  SKIPPING!");
                }
                else
                {
                    Debug.Assert(notMarkedMovie.LastViewedAtDateTime != null, "notMarkedMovie.LastViewedAtDateTime != null");
                    traktSyncHistoryPostMovies.AddMovie(new TraktMovie()
                    {
                        Title = notMarkedMovie.Title
                        ,Year = (int?) notMarkedMovie.Year
                        , Ids = new TraktMovieIds()
                        {
                            Imdb = notMarkedMovie.ImdbId
                        }
                    }, (DateTime) notMarkedMovie.LastViewedAtDateTime);
                }
            }

            var payload = traktSyncHistoryPostMovies.Build();

            if (payload.Movies != null)
            {
                await _traktClient.Sync.AddWatchedHistoryItemsAsync(payload);
            }

            await ReportProgress("--------------------------------------------");
            await ReportProgress("Finished syncing Movies!");
            await ReportProgress("--------------------------------------------");
        }

        #region Helpers
        
        private async Task SendEmail(string emailAddress)
        {
            var client = new SendGridClient(_emailApiKey);
            var html = "<html><div>{log}</div></html>";
            var logHtml = _log.Aggregate("", (current, log) => current + log + "<br/>");
            html = html.Replace("{log}", logHtml);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("noreply@blackhole.com", "Plex To Trakt"),
                Subject = "Plex to Trakt Results",
                PlainTextContent = string.Join(Environment.NewLine, _log.ToArray()),
                HtmlContent = html
            };
            msg.AddTo(new EmailAddress(emailAddress));
            var response = await client.SendEmailAsync(msg);
        }

        private bool HasMatchingIdTVShows(IMediaItem plexItem, ITraktIds traktIds)
        {
            switch (plexItem.ExternalProvider)
            {
                case "imdb":
                    return plexItem.ExternalProviderId.Equals(traktIds.Imdb);
                case "tmdb":
                    return uint.TryParse(plexItem.ExternalProviderId, out var tmdbId) &&
                           tmdbId.Equals(traktIds.Tmdb);
                case "thetvdb":
                    return uint.TryParse(plexItem.ExternalProviderId, out var tvdbId) &&
                           tvdbId.Equals(traktIds.Tvdb);
                case "tvrage":
                    return uint.TryParse(plexItem.ExternalProviderId, out var tvrageId) &&
                           tvrageId.Equals(traktIds.TvRage);
                default:
                    return false;
            }
        }

        private bool HasMatchingIdMovies(IMediaItem plexItem, ITraktMovieIds traktIds)
        {
            switch (plexItem.ExternalProvider)
            {
                case "imdb":
                    return plexItem.ExternalProviderId.Equals(traktIds.Imdb);
                case "tmdb":
                    return uint.TryParse(plexItem.ExternalProviderId, out var tmdbId) &&
                           tmdbId.Equals(traktIds.Tmdb);
                default:
                    return false;
            }
        }

        private async Task ReportProgress(string progress)
        {
            _log.Add($"[{DateTime.Now}] {progress}" + Environment.NewLine);
        }

        #endregion
    }
}