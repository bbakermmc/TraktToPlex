using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using TraktNet;
using TraktNet.Objects.Authentication;
using TraktNet.Objects.Basic;
using TraktNet.Objects.Get.Movies;
using TraktNet.Requests.Parameters;
using TraktToPlex.Plex.Models;
using TraktToPlex.Plex.Models.Shows;

namespace TraktToPlex.Plex
{
    public class PlexSync
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

        public PlexSync(string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId,
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

        public async Task SyncToPlex()
        {
            var _plexClient = new PlexClient(_plexClientSecret);
            var _traktClient = new TraktClient(_traktClientId, _traktClientSecret);

            try
            {
                _plexClient.SetAuthToken(_plexKey);
                _plexClient.SetPlexServerUrl(_plexUrl);

                _traktClient.Authorization = TraktAuthorization.CreateWith(_traktKey);

                await MigrateMovies(_plexClient, _traktClient);
                await MigrateTvShows(_plexClient, _traktClient);

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

            if (traktMovies.Length <= plexMovies.Length)
            {
                await ReportProgress("Going through all shows on Trakt, to see if we find a match on Plex..");

                var i = 0;

                foreach (var watchedMovie in traktMovies)
                {
                    i++;
                    var plexMovie = plexMovies.FirstOrDefault(x => HasMatchingIdMovies(x, watchedMovie.Ids));
                    if (plexMovie == null)
                    {
                        await ReportProgress(
                            $"({i}/{traktMovies.Length}) The movie \"{watchedMovie.Title}\" was not found as watched on Plex. Skipping!");
                        continue;
                    }

                    if (plexMovie.ViewCount != null && plexMovie.ViewCount > 0)
                    {
                        await ReportProgress(
                            $"({i}/{traktMovies.Length}) The movie {watchedMovie.Title} is already watched..  Skipping!");
                    }
                    else
                    {
                        await _plexClient.Scrobble(plexMovie);
                        await ReportProgress(
                            $"({i}/{traktMovies.Length}) Marked the movie \"{watchedMovie.Title}\" as watched!");
                    }
                }
            }
            else
            {
                await ReportProgress("Going through all shows on Plex, to see if we find a match on Trakt..");
                var i = 0;
                foreach (var plexMovie in plexMovies)
                {
                    i++;
                    var traktMovie = traktMovies.FirstOrDefault(x => HasMatchingIdMovies(plexMovie, x.Ids));
                    if (traktMovie == null)
                    {
                        await ReportProgress(
                            $"({i}/{plexMovies.Length}) The movie \"{plexMovie.Title}\" was not found as watched on Trakt. Skipping!");
                        continue;
                    }

                    await ReportProgress(
                        $"({i}/{plexMovies.Length}) Found the movie \"{plexMovie.Title}\" as watched on Trakt. Processing!");
                    await _plexClient.Scrobble(plexMovie);
                    await ReportProgress($"Marking {plexMovie.Title} as watched..");
                }
            }


            await ReportProgress("--------------------------------------------");
            await ReportProgress("Finished syncing Movies!");
            await ReportProgress("--------------------------------------------");
        }

        private async Task MigrateTvShows(PlexClient _plexClient, TraktClient _traktClient)
        {
            await ReportProgress("--------------------------------------------");
            await ReportProgress("Started syncing Tv Shows!");
            await ReportProgress("--------------------------------------------");

            await ReportProgress("Importing Trakt shows..");
            var traktShows = (await _traktClient.Sync.GetWatchedShowsAsync(new TraktExtendedInfo().SetFull()))
                .ToArray();
            await ReportProgress($"Found {traktShows.Length} shows on Trakt");

            await ReportProgress("Importing Plex shows..");
            var plexShows = await _plexClient.GetShows();
            await ReportProgress($"Found {plexShows.Length} shows on Plex");
            await ReportProgress("Going through all shows on Plex, to see if we find a match on Trakt..");

            if (traktShows.Length <= plexShows.Length)
            {
                var i = 0;

                foreach (var watchedShow in traktShows)
                {
                    i++;

                    var plexShow = plexShows.FirstOrDefault(x => HasMatchingIdTVShows(x, watchedShow.Ids));

                    if (plexShow == null)
                    {
                        await ReportProgress(
                            $"({i}/{traktShows.Length}) The show \"{watchedShow.Title}\" was not found as watched on Plex. Skipping!");
                        continue;
                    }

                    if (plexShow.ExternalProvider.Equals("themoviedb"))
                    {
                        await ReportProgress(
                            $"Skipping {plexShow.Title} since it's configured to use TheMovieDb agent for metadata. This agent isn't supported, as Trakt doesn't have TheMovieDb ID's.");
                        continue;
                    }

                    await ReportProgress(
                        $"({i}/{traktShows.Length}) Found the show \"{watchedShow.Title}\" as watched on Trakt. Processing!");
                    await _plexClient.PopulateSeasons(plexShow);
                    foreach (var traktSeason in watchedShow.WatchedSeasons.Where(x => x.Number.HasValue))
                    {
                        var scrobbleEpisodes = new List<Episode>();
                        var plexSeason = plexShow.Seasons.FirstOrDefault(x => x.No == traktSeason.Number);

                        if (plexSeason == null)
                            continue;

                        await _plexClient.PopulateEpisodes(plexSeason);

                        foreach (var traktEpisode in traktSeason.Episodes.Where(x => x.Number.HasValue))
                        {
                            var plexEpisode = plexSeason.Episodes.FirstOrDefault(x => x.No == traktEpisode.Number);
                            if (plexEpisode == null || plexEpisode.ViewCount > 0)
                                continue;
                            scrobbleEpisodes.Add(plexEpisode);
                        }

                        await Task.WhenAll(scrobbleEpisodes.Select(_plexClient.Scrobble));
                        await ReportProgress(
                            $"     Marked {scrobbleEpisodes.Count} episodes as watched in season {plexSeason.No} of \"{plexShow.Title}\"..");
                    }
                }
            }
            else
            {
                var i = 0;
                foreach (var plexShow in plexShows)
                {
                    i++;
                    if (plexShow.ExternalProvider.Equals("themoviedb"))
                    {
                        await ReportProgress(
                            $"Skipping {plexShow.Title} since it's configured to use TheMovieDb agent for metadata. This agent isn't supported, as Trakt doesn't have TheMovieDb ID's.");
                        continue;
                    }

                    var traktShow = traktShows.FirstOrDefault(x => HasMatchingIdTVShows(plexShow, x.Ids));
                    if (traktShow == null)
                    {
                        await ReportProgress(
                            $"({i}/{plexShows.Length}) The show \"{plexShow.Title}\" was not found as watched on Trakt. Skipping!");
                        continue;
                    }

                    await ReportProgress(
                        $"({i}/{plexShows.Length}) Found the show \"{plexShow.Title}\" as watched on Trakt. Processing!");
                    await _plexClient.PopulateSeasons(plexShow);
                    foreach (var traktSeason in traktShow.WatchedSeasons.Where(x => x.Number.HasValue))
                    {
                        var scrobbleEpisodes = new List<Episode>();
                        var plexSeason = plexShow.Seasons.FirstOrDefault(x => x.No == traktSeason.Number);
                        if (plexSeason == null)
                            continue;
                        await _plexClient.PopulateEpisodes(plexSeason);
                        foreach (var traktEpisode in traktSeason.Episodes.Where(x => x.Number.HasValue))
                        {
                            var plexEpisode = plexSeason.Episodes.FirstOrDefault(x => x.No == traktEpisode.Number);
                            if (plexEpisode == null || plexEpisode.ViewCount > 0)
                                continue;
                            scrobbleEpisodes.Add(plexEpisode);
                        }

                        await ReportProgress(
                            $"Marking {scrobbleEpisodes.Count} episodes as watched in season {plexSeason.No} of \"{plexShow.Title}\"..");
                        await Task.WhenAll(scrobbleEpisodes.Select(_plexClient.Scrobble));
                        await ReportProgress("Done!");
                    }
                }
            }

            await ReportProgress("--------------------------------------------");
            await ReportProgress("Finished syncing Tv Shows!");
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
                From = new EmailAddress("noreply@blackhole.com", "Trakt To Plex"),
                Subject = "Trakt to Plex Results",
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
                    return uint.TryParse(plexItem.ExternalProviderId, out var tmdbId) && tmdbId.Equals(traktIds.Tmdb);
                case "thetvdb":
                    return uint.TryParse(plexItem.ExternalProviderId, out var tvdbId) && tvdbId.Equals(traktIds.Tvdb);
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
                    return uint.TryParse(plexItem.ExternalProviderId, out var tmdbId) && tmdbId.Equals(traktIds.Tmdb);
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