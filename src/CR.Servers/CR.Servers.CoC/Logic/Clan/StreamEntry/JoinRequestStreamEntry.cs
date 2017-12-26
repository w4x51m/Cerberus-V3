﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.Servers.CoC.Extensions.Helper;
using CR.Servers.CoC.Logic.Clan.Items;
using CR.Servers.CoC.Logic.Enums;
using CR.Servers.Extensions.List;
using Newtonsoft.Json.Linq;

namespace CR.Servers.CoC.Logic.Clan
{
    internal class JoinRequestStreamEntry : StreamEntry
    {
        internal override AllianceStream Type => AllianceStream.JoinRequest;

        public JoinRequestStreamEntry()
        {

        }

        internal string Message;
        internal string Judge;
        internal InviteState State = InviteState.Waiting;

        public JoinRequestStreamEntry(Member Member) : base(Member)
        {
        }

        internal override void Encode(List<byte> Packet)
        {
            base.Encode(Packet);
            Packet.AddString(this.Message);
            Packet.AddString(this.Message);
            Packet.AddInt((int)this.State);
        }

        internal override void Load(JToken Json)
        {
            base.Load(Json);

            if (JsonHelper.GetJsonString(Json, "message", out string Message))
            {
                this.Message = Message ?? string.Empty;
            }
            else
            {
                this.Message = string.Empty;
            }

            if (JsonHelper.GetJsonString(Json, "judge", out string Judge))
            {
                this.Judge = Judge ?? string.Empty;
            }
            else
            {
                this.Judge = string.Empty;
            }

            if (JsonHelper.GetJsonNumber(Json, "state", out int State))
            {
                this.State = (InviteState) State;
            }
        }

        internal override JObject Save()
        {
            JObject Json = base.Save();
            Json.Add("message", this.Message);
            Json.Add("judge", this.Judge);
            Json.Add("state", (int)this.State);
            return Json;
        }
    }
}