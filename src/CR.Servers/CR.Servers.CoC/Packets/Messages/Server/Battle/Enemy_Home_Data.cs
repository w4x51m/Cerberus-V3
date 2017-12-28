﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Core;
using CR.Servers.CoC.Files;
using CR.Servers.CoC.Files.CSV_Helpers;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using CR.Servers.CoC.Logic;
using CR.Servers.CoC.Logic.Map;
using CR.Servers.Extensions.List;
using CR.Servers.Logic.Enums;
using Newtonsoft.Json.Linq;

namespace CR.Servers.CoC.Packets.Messages.Server.Battle
{
    internal class Enemy_Home_Data : Message
    {
        internal override short Type => 24107;

        public Enemy_Home_Data(Device Device, Level Enemy) : base(Device)
        {
            this.Enemy = Enemy;
            this.Enemy.Tick();

            this.Player = this.Device.GameMode.Level.Player;
            this.Device.GameMode.Level.Tick();
        }

        public Enemy_Home_Data(Device Device) : base(Device)
        {
            this.Player = this.Device.GameMode.Level.Player;
            this.Device.GameMode.Level.Tick();
        }


        internal Level Enemy;
        internal Player Player;

        internal override void Encode()
        {
            this.Data.AddInt(0);
            this.Data.AddInt(-1);

            if (this.Player.ModSlot.AIAttack)
            {
                this.Player.ModSlot.AILevel.Home.Encode(this.Data);
                this.Player.ModSlot.AILevel.Player.Encode(this.Data);
            }
            else
            {
                this.Enemy.Home.Encode(this.Data);
                this.Enemy.Player.Encode(this.Data);
            }

            this.Device.GameMode.Level.Player.Encode(this.Data);

            this.Data.AddInt(this.Player.ModSlot.AIAttack ? 2 : 3);
            this.Data.AddInt(0);
            this.Data.AddByte(0);
        }

        internal override void Process()
        {
            this.Device.State = State.IN_PC_BATTLE;
        }
    }
}
