﻿namespace CR.Servers.CoC.Packets.Messages.Server.Avatar
{
    using System.Linq;
    using CR.Servers.CoC.Logic;

    internal class AvatarStreamMessage : Message
    {
        public AvatarStreamMessage(Device Device) : base(Device)
        {
        }

        internal override short Type => 24411;


        internal override void Encode()
        {
            this.Device.GameMode.Level.Player.Inbox.Encode(this.Data);
        }

        internal override void Process()
        {
            MailEntry[] Streams = this.Device.GameMode.Level.Player.Inbox.Entries.Values.Where(T => T.New == 2).ToArray();

            foreach (MailEntry Stream in Streams)
            {
                Stream.New = 0;
            }
        }
    }
}