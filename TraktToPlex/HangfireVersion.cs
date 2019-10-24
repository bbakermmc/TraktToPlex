using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using TraktNet;
using TraktNet.Objects.Authentication;
using TraktToPlex.Hubs;
using TraktToPlex.Plex;

namespace TraktToPlex
{
    public static class HangfireVersion
    {
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisplayName("Plex Sync")]
        public static async Task Execute(PerformContext ctx, string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId, string traktClientSecret, string emailApiKey)
        {
            /*ctx.SetTextColor(ConsoleTextColor.Yellow);
            ctx.WriteLine("Plex");
            ctx.WriteLine($"Start Time: {DateTime.Now}");
            ctx.WriteLine($"Key: {plexKey}");
            ctx.ResetTextColor();*/
            
            
            
            var plexSync = new PlexSync(plexKey, plexUrl, plexClientSecret, traktKey, traktClientId, traktClientSecret, emailApiKey);
            await plexSync.SyncToPlex();


            /*var guid = Guid.NewGuid().ToString();

            var file = $"D:\\{guid}.txt";
            var sw = new StreamWriter($"{file}", true);
            
            
            
            using (var tw = sw)
            {
                tw.WriteLine($"[{DateTime.Now}] PlexKey: {plexKey}");
                tw.WriteLine($"[{DateTime.Now}] TraktKey: {traktKey}");
            }

            var migration = new MigrationHub(_traktClient, _plexClient, file);
            await migration.StartMigration(traktKey, plexKey, plexUrl);*/
        }
    }
}