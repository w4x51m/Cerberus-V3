﻿using CR.Servers.CoC.Logic;
using CR.Servers.Extensions.List;

namespace CR.Servers.CoC.Packets.Messages.Server.Home
{
    internal class Global_Chat_Line : Message
    {
        internal override short Type => 24715;

        public Global_Chat_Line(Device Device) : base(Device)
        {
        }

        public Global_Chat_Line(Device Device, Player Player) : base(Device)
        {
            this.Player = Player;
            this.Name = this.Player.Name;
            this.ExpLevel = this.Player.ExpLevel;
            this.League = this.Player.League;
            this.UserId = this.Player.UserId;
        }

        internal string Message;

        internal Player Player;
        internal string Name;
        internal int ExpLevel;
        internal int League;
        internal long UserId;
        internal bool Bot;

        internal override void Encode()
        {
            this.Data.AddString(this.Message);
            this.Data.AddString(this.Name);
            this.Data.AddInt(this.ExpLevel);
            this.Data.AddInt(this.League);
            this.Data.AddLong(this.UserId);
            this.Data.AddLong(this.UserId);

            if (!this.Bot && this.Player.InAlliance)
            {
                this.Data.AddBool(true);

                this.Data.AddLong(this.Player.AllianceId);
                this.Data.AddString(this.Player.Alliance.Header.Name);
                this.Data.AddInt(this.Player.Alliance.Header.Badge);
            }
            else
                this.Data.AddBool(false);
        }
    }
}