using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Handlers = Exiled.Events.Handlers;

namespace AntiAFk
{
    public class AntiAfk : Plugin<PConfig>
    {
        public override string Name { get; } = "AntiAfk";
        public override string Author { get; } = "Chalęshka";
        public override string Prefix { get; } = "AntiAfk";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(4, 2, 2);

        public override PluginPriority Priority { get; } = PluginPriority.Highest;

        public static List<PlayersList> pl;

        public static PConfig cfg;

        Events ev;
        public override void OnEnabled()
        {
            cfg = Config;
            ev = new Events();

            Handlers.Player.Destroying += ev.OnLeave;
            Handlers.Player.Verified += ev.OnConnected;
            Handlers.Server.RoundStarted += ev.onRoundStarted;
            Handlers.Server.RoundEnded += ev.onRoundEnded;
            Handlers.Player.ChangingRole += ev.onChangingRole;
            Handlers.Player.Left += ev.OnLeave;
            Handlers.Server.RoundStarted += onRoundStarted;
            Log.Info("AntiAFk was created by Chalęshka.");
        }

        public override void OnDisabled()
        {
            Handlers.Player.Destroying -= ev.OnLeave;
            Handlers.Player.Verified -= ev.OnConnected;
            Handlers.Server.RoundStarted -= ev.onRoundStarted;
            Handlers.Server.RoundEnded -= ev.onRoundEnded;
            Handlers.Player.ChangingRole -= ev.onChangingRole;
            Handlers.Player.Left -= ev.OnLeave;
            Handlers.Server.RoundStarted -= onRoundStarted;

            cfg = null;
            ev = null;
        }

        public void onRoundStarted()
        {
            pl = new List<PlayersList>();
        }
    }
}
