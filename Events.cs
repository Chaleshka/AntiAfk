using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using UnityEngine;
using Exiled.API.Enums;
using DiscordIntegration;
using CommandSystem;

namespace AntiAFk
{
    using static AntiAfk;
    public class Events
    {
        private bool roundEnded = true;
        private CoroutineHandle timerCoroutine = new CoroutineHandle();
        System.Random rnd = new System.Random();

        public async void onRoundStarted()
        {
            roundEnded = false;

            await Task.Delay(100);
            foreach (Player ply in Player.List)
            {
                pl.Add(new PlayersList() { id = ply.Id, role = ply.Role});
            }
            if (timerCoroutine.IsRunning)
            {
                Timing.KillCoroutines(timerCoroutine);
            }
            timerCoroutine = Timing.RunCoroutine(CheckAFK());
        }
        public void onRoundEnded(RoundEndedEventArgs ev)
        {
            roundEnded = true;
        }
        public void OnLeave(DestroyingEventArgs ev)
        {
            if (!roundEnded)
            {
                pl.Remove(pl.Find(p => p.id == ev.Player.Id));
            }
        }
        public void OnLeave(LeftEventArgs ev)
        {
            if (!roundEnded)
            {
                if (AntiAfk.cfg.ReplacePlayer)
                    ReplacePlayer(ev.Player);
            }
        }
        public void OnConnected(VerifiedEventArgs ev)
        {
            if (!roundEnded)
            {
                pl.Add(new PlayersList() { id = ev.Player.Id, role = ev.Player.Role });
            }
        }
        public async void onChangingRole(ChangingRoleEventArgs ev)
        {
            await Task.Delay(100);
            pl[pl.FindIndex(p => p.id == ev.Player.Id)].role = ev.Player.Role;
            if(ev.Player.Role == RoleType.Scp079)
            {
                pl[pl.FindIndex(p => p.id == ev.Player.Id)].camera = ev.Player.Camera;
                pl[pl.FindIndex(p => p.id == ev.Player.Id)].experience = ev.Player.Experience;
                pl[pl.FindIndex(p => p.id == ev.Player.Id)].energy = ev.Player.Energy;
                pl[pl.FindIndex(p => p.id == ev.Player.Id)].pitch = ev.Player.Camera.curPitch;
                pl[pl.FindIndex(p => p.id == ev.Player.Id)].rot = ev.Player.Camera.curRot;
            }
        }












        bool repl = false;
        MessageToLog msToLog = new MessageToLog();
        private IEnumerator<float> CheckAFK()
        {
            while (!roundEnded)
            {
                yield return Timing.WaitForSeconds(1f);
                foreach (PlayersList ply in pl)
                {
                    if ((ply.role == RoleType.Spectator || ply.role == RoleType.None) || (AntiAfk.cfg.IgnorTut && ply.role == RoleType.Tutorial) || ply.ignorAFK)
                    {
                        ply.timer = 0;
                    }
                    else if(ply.role == RoleType.Scp079)
                    {
                        if (ply.rot == Player.Get(ply.id).Camera.curRot && ply.camera == Player.Get(ply.id).Camera &&
                            ply.experience == Player.Get(ply.id).Experience && ply.energy == Player.Get(ply.id).Energy &&
                            ply.pitch == Player.Get(ply.id).Camera.curPitch && ply.camera == Player.Get(ply.id).Camera)
                            ply.timer++;
                        else
                        {
                            ply.pitch = Player.Get(ply.id).Camera.curPitch;
                            ply.rot = Player.Get(ply.id).Camera.curRot;
                            ply.camera = Player.Get(ply.id).Camera;
                            ply.Rotation = Player.Get(ply.id).Rotation;
                            ply.experience = Player.Get(ply.id).Experience;
                            ply.energy = Player.Get(ply.id).Energy;
                            ply.timer = 0;
                        }
                        if (ply.timer >= AntiAfk.cfg.TimerAfk - AntiAfk.cfg.TimeToKick)
                            Player.Get(ply.id).Broadcast(1, string.Format(AntiAfk.cfg.MessageToScp079, AntiAfk.cfg.TimerAfk - ply.timer));
                        if (ply.timer >= AntiAfk.cfg.TimerAfk)
                        {
                            if (AntiAfk.cfg.MessageAboutKickToLog)
                                msToLog.Send("bans", string.Format("```diff\n+AntiAFK+\n{0} ({1}) был кикнут за afk.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));

                            Player.Get(ply.id).Kick(AntiAfk.cfg.KickMessage, "AntiAFK");
                        }
                    }
                    else
                    {
                        if (ply.Position == Player.Get(ply.id).Position &&
                            ply.Rotation == Player.Get(ply.id).Rotation)
                            ply.timer++;
                        else
                        {
                            ply.Position = Player.Get(ply.id).Position;
                            ply.Rotation = Player.Get(ply.id).Rotation;
                            ply.timer = 0;
                        }
                        if (ply.timer >= AntiAfk.cfg.TimerAfk - AntiAfk.cfg.TimeToKick)
                            Player.Get(ply.id).Broadcast(1, string.Format(AntiAfk.cfg.Message, AntiAfk.cfg.TimerAfk - ply.timer));
                        if (ply.timer >= AntiAfk.cfg.TimerAfk)
                        {
                            if(AntiAfk.cfg.MessageAboutKickToLog)
                                msToLog.Send("bans", string.Format("```diff\n+AntiAFK+\n{0} ({1}) был кикнут за afk.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));
                            Player.Get(ply.id).Kick(AntiAfk.cfg.KickMessage, "AntiAFK");
                        }
                    }
                }
            }
        }



        private async void ReplacePlayer(Player player1)
        {
            Player PlayerToReplace; List<Player> spects; RoleType role; ushort ammo1, ammo2, ammo3, ammo4, ammo5; 
            List<Item> inventory; Vector3 pos, rotation; float hp, ahp; Player cuffer; float energy, exp; byte level; 
            bool cuffered;
            if (player1.Role == RoleType.Tutorial && AntiAfk.cfg.IgnorTut)
                return;
            spects = new List<Player>();
            foreach(Player ply in Player.List)
            {
                if (ply.Role == RoleType.Spectator && ply.IsOverwatchEnabled == false)
                    spects.Add(ply);
            }
            try
            {
                PlayerToReplace = spects[rnd.Next(spects.Count)];
                repl = true;
            }
            catch(Exception ex) { repl = false; return; }

            role = player1.Role;

            inventory = new List<Item>();
            foreach (Item item in player1.Items)
            {
                inventory.Add(item);
            }

            pos = player1.Position; rotation = player1.Rotation;

            hp = player1.Health; ahp = player1.ArtificialHealth;

            ammo1 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato556); ammo2 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato762);
            ammo3 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato9); ammo4 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Ammo12Gauge);
            ammo5 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Ammo44Cal);
            Player.Get(player1.Id).SetAmmo(AmmoType.Nato556, 0); Player.Get(player1.Id).SetAmmo(AmmoType.Nato762, 0); 
            Player.Get(player1.Id).SetAmmo(AmmoType.Nato9, 0);
            Player.Get(player1.Id).SetAmmo(AmmoType.Ammo12Gauge, 0); Player.Get(player1.Id).SetAmmo(AmmoType.Ammo44Cal, 0);

            cuffer = player1.Cuffer;
            cuffered = player1.IsCuffed;

            energy = player1.Energy; exp = player1.Experience;
            level = player1.Level;

            Camera079 cam = player1.Camera;
            player1.ClearInventory(true);
            player1.Role = RoleType.Spectator;





            PlayerToReplace.Role = role;
            await Task.Delay(400);
            PlayerToReplace.Position = pos;
            PlayerToReplace.Rotation = rotation;

            PlayerToReplace.ClearInventory();
            PlayerToReplace.ResetInventory(inventory);
            if (cuffered)
                PlayerToReplace.Cuffer = cuffer;
            PlayerToReplace.Health = hp;
            PlayerToReplace.ArtificialHealth = ahp;
            PlayerToReplace.SetAmmo(AmmoType.Nato556, ammo1); PlayerToReplace.SetAmmo(AmmoType.Nato762, ammo2);
            PlayerToReplace.SetAmmo(AmmoType.Nato9, ammo3); PlayerToReplace.SetAmmo(AmmoType.Ammo12Gauge, ammo4);
            PlayerToReplace.SetAmmo(AmmoType.Ammo44Cal, ammo5);
            PlayerToReplace.Broadcast(5, AntiAfk.cfg.ReplaceMessage);

            if (role == RoleType.Scp079)
            {
                await Task.Delay(100);
                PlayerToReplace.Camera = cam;
                PlayerToReplace.Level = level;
                PlayerToReplace.Experience = exp;
                PlayerToReplace.Energy = energy;
            }
            await Task.Delay(100);
            string com = "/setlevel " + PlayerToReplace.Id + " " + level;
            Server.RunCommand(com);
            PlayerToReplace.Rotation = rotation;
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    class IgnorAFK : ICommand
    {
        public string Command { get; set; } = "IgnorAfk";

        public string[] Aliases { get; set; } = { "IA", "Ignorafk", "Iafk"};

        public string Description { get; set; } = "Игнорирует афк у игрока.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.PlayersManagement))
            {
                response = string.Format("You need \"{0}\" permission to run this command!", PlayerPermissions.PlayersManagement);
                return false;
            }
            if (arguments.Count == 0)
            {
                response = "Команда введена неверно.";
                return false;
            }
            if (arguments.Count == 1)
            {
                try
                {
                    if (pl[pl.FindIndex(p => p.id == Convert.ToInt32(arguments.At(0)))].ignorAFK)
                    {
                        pl[pl.FindIndex(p => p.id == Convert.ToInt32(arguments.At(0)))].ignorAFK = false;
                    }
                    else
                    {
                        pl[pl.FindIndex(p => p.id == Convert.ToInt32(arguments.At(0)))].ignorAFK = true;
                    }
                    response = "Команда выполнена.";
                    return true;
                }
                catch
                {
                    try
                    {
                        if(arguments.At(0) == "all")
                        {
                            foreach(PlayersList ply in pl)
                            {
                                if (ply.ignorAFK)
                                    ply.ignorAFK = false;
                                else
                                    ply.ignorAFK = true;
                            }
                        }
                        response = "Команда выполнена.";
                        return true;
                    }
                    catch
                    {
                        response = "Команда введена неверно.";
                        return false;
                    }
                }
            }
            response = "Команда введена неверно.";
            return false;
        }
    }
}
