﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Logic;
using CR.Servers.Extensions.List;
using CR.Servers.Logic.Enums;

namespace CR.Servers.CoC.Packets.Messages.Server.Authentication
{
    internal class Authentication_OK : Message
    {
        internal override short Type => 20104;

        public Authentication_OK(Device client) : base(client)
        {
            Device.State = State.LOGGED;
            this.Version = 1;
        }

        internal int ServerBuild;
        internal int ServerMajorVersion;
        internal int ContentVersion;

        internal override void Encode()
        {
            this.Data.AddLong(2);
            this.Data.AddLong(2);

            this.Data.AddString("2");

            this.Data.AddString(string.Empty); // Facebook ID
            this.Data.AddString(string.Empty); // Gamecenter ID

            this.Data.AddInt(9);
            this.Data.AddInt(256);
            this.Data.AddInt(0); // Content Version

            this.Data.AddString("prod");

            this.Data.AddInt(1); // Total Session
            this.Data.AddInt(0); // Play Time Seconds
            this.Data.AddInt(0); // Days Since Started Playing

            this.Data.AddString(null); // 103121310241222
            this.Data.AddString(null); // Server Time
            this.Data.AddString("0"); // Account Creation Date

            this.Data.AddInt(0); // StartupCooldownSeconds

            this.Data.AddString(null); // Google Service ID
            this.Data.AddString("fr");
            this.Data.AddString(null);

            this.Data.AddInt(1);

            this.Data.AddString(null);
            this.Data.AddString(null);
            this.Data.AddString(null);

            this.Data.AddInt(2);
            {
                this.Data.AddString("https://game-assets.clashofclans.com/");
                this.Data.AddString("http://b46f744d64acd2191eda-3720c0374d47e9a0dd52be4d281c260f.r11.cf2.rackcdn.com/");
            }

            this.Data.AddInt(1);
            {
                this.Data.AddString("https://event-assets.clashofclans.com");
            }
        }
    }
}