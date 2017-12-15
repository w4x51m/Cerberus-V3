﻿using System.Collections.Generic;
using CR.Servers.CoC.Core;
using CR.Servers.CoC.Extensions.Helper;
using CR.Servers.CoC.Files.CSV_Logic.Logic;
using CR.Servers.CoC.Logic;
using CR.Servers.Extensions.Binary;
using CR.Servers.Extensions.List;
using CR.Servers.Logic.Enums;
using Newtonsoft.Json.Linq;

namespace CR.Servers.CoC.Packets.Commands.Client.Battle
{
    internal class Place_Hero : Command
    {
        internal override int Type => 705;

        public Place_Hero(Device Device, Reader Reader) : base(Device, Reader)
        {

        }

        internal int X;
        internal int Y;
        internal HeroData Hero;

        internal override void Decode()
        {
            this.X = this.Reader.ReadInt32();
            this.Y = this.Reader.ReadInt32();
            this.Hero = this.Reader.ReadData<HeroData>();
            base.Decode();
        }

        internal override void Encode(List<byte> Data)
        {
            Data.AddInt(this.X);
            Data.AddInt(this.Y);
            Data.AddInt(this.Hero.GlobalId);
            base.Encode(Data);
        }

        internal override void Execute()
        {
            if (this.Hero != null)
            {
                var Level = this.Device.GameMode.Level;
                if (Level.GameObjectManager.Map == 1)
                {
                    if (this.Device.State == State.IN_1VS1_BATTLE)
                    {
                        var Battle = Resources.BattlesV2.GetPlayer(Level.Player.BattleIdV2, Level.Player.UserId);

                        int Index = Battle.ReplayInfo.Units.FindIndex(T => T[0] == this.Hero.GlobalId);
                        if (Index > -1)
                            Battle.ReplayInfo.Units[Index][1]++;
                        else
                            Battle.ReplayInfo.Units.Add(new[] {this.Hero.GlobalId, 1});

                        Level.BattleManager.BattleCommandManager.StoreCommands(this);
                    }
                }
            }
        }

        internal override JObject Save()
        {
            JObject Json = new JObject
            {
                {"base", this.SaveBase()},
                {"x", this.X},
                {"y", this.Y},
                {"d", this.Hero.GlobalId}
            };

            return Json;
        }
    }
}