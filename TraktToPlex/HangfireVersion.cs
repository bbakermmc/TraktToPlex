using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using TraktToPlex.Plex;
using TraktToPlex.Trakt;

namespace TraktToPlex
{
    public static class HangfireVersion
    {
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Plex Sync")]
        public static async Task ExecutePlex(PerformContext ctx, string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId, string traktClientSecret, string emailApiKey, string emailSendTo)
        {
            var plexSync = new PlexSync(plexKey, plexUrl, plexClientSecret, traktKey, traktClientId, traktClientSecret, emailApiKey, emailSendTo);
            await plexSync.SyncToPlex();
        }
        
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Trakt Sync")]
        public static async Task ExecuteTrakt(PerformContext ctx, string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId, string traktClientSecret, string emailApiKey, string emailSendTo)
        {
            var traktSync = new TraktSync(plexKey, plexUrl, plexClientSecret, traktKey, traktClientId, traktClientSecret, emailApiKey, emailSendTo);
            await traktSync.SyncToTrakt();
        }
    }
}