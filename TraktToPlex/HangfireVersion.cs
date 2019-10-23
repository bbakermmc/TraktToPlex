using System;
using System.IO;
using System.Threading.Tasks;
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
        public static async Task Execute(PerformContext ctx, string plexKey, string plexUrl, string plexClientSecret, string traktKey, string traktClientId, string traktClientSecret)
        {
            /*ctx.SetTextColor(ConsoleTextColor.Yellow);
            ctx.WriteLine("Plex");
            ctx.WriteLine($"Start Time: {DateTime.Now}");
            ctx.WriteLine($"Key: {plexKey}");
            ctx.ResetTextColor();*/
            
            var _plexClient = new PlexClient(plexClientSecret);
            
            var _traktClient = new TraktClient(traktClientId, traktClientSecret);
            
            _traktClient.Authorization = TraktAuthorization.CreateWith(traktKey);
            
            var guid = Guid.NewGuid().ToString();

            var file = $"D:\\{guid}.txt";
            var sw = new StreamWriter($"{file}", true);
            
            using (var tw = sw)
            {
                tw.WriteLine($"[{DateTime.Now}] PlexKey: {plexKey}");
                tw.WriteLine($"[{DateTime.Now}] TraktKey: {traktKey}");
            }

            var migration = new MigrationHub(_plexClient, _traktClient, file);
            await migration.StartMigration(traktKey, plexKey, plexUrl);
        }
    }
}