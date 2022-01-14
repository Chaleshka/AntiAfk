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

namespace AntiAFk
{
    public class Events
    {
        private bool roundEnded = true;
        private CoroutineHandle timerCoroutine = new CoroutineHandle();
        System.Random rnd = new System.Random();

        List<PlayersList> pl;
        public async void onRoundStarted()
        {
            roundEnded = false;
            pl = new List<PlayersList>();

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
                    if ((ply.role == RoleType.Spectator || ply.role == RoleType.None) || (AntiAfk.cfg.IgnorTut && ply.role == RoleType.Tutorial))
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
                            if (AntiAfk.cfg.ReplacePlayer)
                            {
                                ReplacePlayer(Player.Get(ply.id));
                            }
                            if (AntiAfk.cfg.ReplacePlayer && AntiAfk.cfg.MessageAboutKickToLog && repl)
                                msToLog.Send("bans", string.Format("```ansi\n[0;32mAntiAFK[0m\n{0} ({1}) был кикнут за afk и заменен другим игроком.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));
                            else if (AntiAfk.cfg.MessageAboutKickToLog)
                                msToLog.Send("bans", string.Format("```ansi\n[0;32mAntiAFK[0m\n{0} ({1}) был кикнут за afk.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));

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
                            if (AntiAfk.cfg.ReplacePlayer)
                            {
                                ReplacePlayer(Player.Get(ply.id));
                            }
                            if (AntiAfk.cfg.ReplacePlayer && AntiAfk.cfg.MessageAboutKickToLog && repl)
                                msToLog.Send("bans", string.Format("```diff\n+AntiAFK+\n{0} ({1}) был кикнут за afk и заменен другим игроком.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));
                            else if(AntiAfk.cfg.MessageAboutKickToLog)
                                msToLog.Send("bans", string.Format("```diff\n+AntiAFK+\n{0} ({1}) был кикнут за afk.\n```", Player.Get(ply.id).Nickname, Player.Get(ply.id).UserId));
                            
                            Player.Get(ply.id).Kick(AntiAfk.cfg.KickMessage, "AntiAFK");
                        }
                    }
                }
            }
        }

        List<Player> spects;
        Player PlayerToReplace;
        private void ReplacePlayer(Player player1)
        {
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

            RoleType role = player1.Role;

            List<Item> inventory = new List<Item>();
            foreach (Item item in player1.Items)
            {
                inventory.Add(item);
            }

            Vector3 pos = player1.Position, rotation = player1.Rotation;

            float hp = player1.Health, ahp = player1.ArtificialHealth;

            ushort ammo1 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato556), ammo2 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato762),
            ammo3 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Nato9), ammo4 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Ammo12Gauge),
            ammo5 = player1.GetAmmo(Exiled.API.Enums.AmmoType.Ammo44Cal);

            Player cuffer = player1.Cuffer;

            float energy = player1.Energy, exp = player1.Experience;
            byte level = player1.Level;


            player1.ClearInventory();
            player1.SetAmmo(AmmoType.Nato556, 0); player1.SetAmmo(AmmoType.Nato762, 0); player1.SetAmmo(AmmoType.Nato9, 0);
            player1.SetAmmo(AmmoType.Ammo12Gauge, 0); player1.SetAmmo(AmmoType.Ammo44Cal, 0); player1.Role = RoleType.Spectator;


            Player.Get(PlayerToReplace.Id).Role = role;
            Timing.CallDelayed(1f, () =>
            {
                Player.Get(PlayerToReplace.Id).Position = pos;
                Player.Get(PlayerToReplace.Id).Rotation = rotation;

                if(role == RoleType.Scp079)
                {
                    Player.Get(PlayerToReplace.Id).Level = level;
                    Player.Get(PlayerToReplace.Id).Experience = exp;
                    Player.Get(PlayerToReplace.Id).Energy = energy;
                }

                if (player1.IsCuffed)
                    Player.Get(PlayerToReplace.Id).Cuffer = cuffer;

                Player.Get(PlayerToReplace.Id).ResetInventory(inventory);
                Player.Get(PlayerToReplace.Id).Health = hp;
                Player.Get(PlayerToReplace.Id).ArtificialHealth = ahp;
                Player.Get(PlayerToReplace.Id).SetAmmo(AmmoType.Nato556, ammo1); PlayerToReplace.SetAmmo(AmmoType.Nato762, ammo2);
                Player.Get(PlayerToReplace.Id).SetAmmo(AmmoType.Nato9, ammo3); PlayerToReplace.SetAmmo(AmmoType.Ammo12Gauge, ammo4);
                Player.Get(PlayerToReplace.Id).SetAmmo(AmmoType.Ammo44Cal, ammo5);
                Player.Get(PlayerToReplace.Id).Broadcast(5, AntiAfk.cfg.ReplaceMessage);
            });
        }
    }
}
