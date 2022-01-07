using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiAFk
{
    public sealed class PConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Время (в секундах), сколько игрок может стоять afk.")]
        public int TimerAfk { get; private set; } = 120;

        [Description("За сколько секунд до кика, будет высвечиваться сообщение (Message).")]
        public int TimeToKick { get; private set; } = 15;

        [Description("{0} - кол-во секунд до кика.")]
        public string Message { get; private set; } = "<color=#00A287> Начните двигаться, иначе вы будите кикнуты через <color=red>{0}</color> сек.</color>";

        [Description("{0} - кол-во секунд до кика.")]
        public string MessageToScp079 { get; private set; } = "<color=#00A287> Начните двигать камерой, иначе вы будите кикнуты через <color=red>{0}</color> сек.</color>";

        [Description("Сообщение, с которым кикнет игрока.")]
        public string KickMessage { get; private set; } = "AFK";

        [Description("Заменять игроков? (true если да)")]
        public bool ReplacePlayer { get; private set; } = true;

        [Description("Заменять туториал? (true если да).")]
        public bool IgnorTut { get; private set; } = true;

        [Description("Сообщение игроку, который будет поставлен на место замены.")]
        public string ReplaceMessage { get; private set; } = "<color=#00A287>Вы были заменены на игрока, который вышел/был забанен.</color>";

        [Description("Отправлять сообщение в логи, о кике человека и его замене (если включен ReplacePlayer).")]
        public bool MessageAboutKickToLog { get; private set; } = true;
    }
}
