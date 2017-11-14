﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Logic;
using CR.Servers.Extensions.List;

namespace CR.Servers.CoC.Packets.Messages.Server.Home
{
    internal class Own_Home_Data : Message
    {
        internal override short Type => 24101;

        public Own_Home_Data(Device Device) : base(Device)
        {
        }

        internal override void Encode()
        {
            this.Data.AddInt(0);
            this.Data.AddInt(-1);
            
            this.Device.Account.Home.Encode(this.Data);
            this.Device.Account.Player.Encode(this.Data);
            this.Data.AddInt(0);
            this.Data.AddInt(0);
            // Data.AddInt(Device.State == State.WAR_EMODE ? 1 : 0);
            this.Data.AddInt(0);

            this.Data.AddLong(1462629754000);
            this.Data.AddLong(1462629754000);
            this.Data.AddLong(1462629754000);
            this.Data.AddInt(0);
        }
    }
}
