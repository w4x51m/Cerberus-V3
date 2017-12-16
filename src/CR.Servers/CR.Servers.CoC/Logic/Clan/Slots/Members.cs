﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CR.Servers.CoC.Logic.Clan.Items;
using CR.Servers.CoC.Logic.Mode;
using CR.Servers.Extensions.List;
using Newtonsoft.Json;

namespace CR.Servers.CoC.Logic.Clan.Slots
{
    internal class Members
    {
        internal Alliance Alliance;

        [JsonProperty] internal ConcurrentDictionary<long, Member> Slots;
        internal ConcurrentDictionary<long, Player> Connected;

        public Members()
        {
            this.Slots = new ConcurrentDictionary<long, Member>();
            this.Connected = new ConcurrentDictionary<long, Player>();
        }

        public Members(Alliance Alliance) : this()
        {
            this.Alliance = Alliance;
        }

        internal bool Join(Player Player, out Member Member)
        {
            if (this.Alliance.Header.NumberOfMembers < 50)
            {
                int Count = Interlocked.Increment(ref this.Alliance.Header.NumberOfMembers);

                if (Count <= 50)
                {
                    Member = new Member(Player);

                    if (this.Slots.TryAdd(Player.UserId, Member))
                    {
                        if (Player.Connected)
                        {
                            this.Connected.TryAdd(Player.UserId, Player);
                        }

                        return true;
                    }
                }

                this.Alliance.Header.NumberOfMembers--;
            }

            Member = null;

            return false;
        }

        internal bool Quit(long Player, out Member Member)
        {
            if (this.Slots.TryRemove(Player, out Member))
            {
                this.Alliance.Header.NumberOfMembers--;
                this.Connected.TryRemove(Player, out _);
                return true;
            }

            return false;
        }

        internal bool Quit(Player Player, out Member Member) => this.Quit(Player.UserId, out Member);

        internal bool Quit(Member Player, out Member Member) => this.Quit(Player.PlayerId, out Member);

        internal Member Get(long UserID) =>  this.Slots.ContainsKey(UserID) ? this.Slots[UserID] : null;

        internal void Encode(List<byte> Packet)
        {
            var Members = this.Slots.Values.ToArray();

            Packet.AddInt(Members.Length);
            int i = 0;
            foreach (var Member in Members)
            {
                var Player = Member.Player;
                Packet.AddLong(Player.UserId);
                Packet.AddString(Player.Name);
                Packet.AddInt((int) Member.Role);

                Packet.AddInt(Player.ExpLevel);
                Packet.AddInt(Player.League);
                Packet.AddInt(Player.Score);
                Packet.AddInt(Player.DuelScore);

                Packet.AddInt(Member.TroopSended);
                Packet.AddInt(Member.TroopReceived);

                Packet.AddInt(i++);
                Packet.AddInt(0);
                Packet.AddInt(i);
                Packet.AddInt(0);

                Packet.AddInt(Member.TimeSinceJoined);
                Packet.AddInt(0); //War Cooldown
                Packet.AddInt(Player.ClanWarPreference);

                Packet.AddByte(1);
                Packet.AddLong(Player.UserId);
            }
        }
    }
}
