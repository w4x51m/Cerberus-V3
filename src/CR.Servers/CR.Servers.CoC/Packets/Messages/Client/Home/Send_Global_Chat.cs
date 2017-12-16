﻿using System;
using CR.Servers.CoC.Core;
using CR.Servers.CoC.Core.Network;
using CR.Servers.CoC.Logic;
using CR.Servers.CoC.Packets.Messages.Server.Home;
using CR.Servers.Extensions.Binary;

namespace CR.Servers.CoC.Packets.Messages.Client.Home
{
    internal class Send_Global_Chat : Message
    {
        internal override short Type => 14715;

        public Send_Global_Chat(Device Device, Reader Reader) : base(Device, Reader)
        {

        }

        internal string Message;

        internal override void Decode()
        {
            this.Message = this.Reader.ReadString();
        }

        internal override void Process()
        {
            if (this.Device.Chat != null)
            {
                //if (DateTime.UtcNow != this.Device.LastGlobalChatEntry)
                {
                    if (!string.IsNullOrEmpty(this.Message))
                    {

                        if (this.Message.Length <= 128)
                        {
                            this.Message = Resources.Regex.Replace(this.Message, " ");

                            if (this.Message.StartsWith(" "))
                            {
                                this.Message = this.Message.Remove(0, 1);
                            }

                            if (this.Message.Length > 0)
                            {
                                if (this.Message.StartsWith(Factory.Delimiter.ToString()))
                                {
                                    var Debug = Factory.CreateDebug(this.Message, this.Device, out string CommandName);

                                    new Global_Chat_Line(this.Device, this.Device.GameMode.Level.Player) { Message = this.Message }.Send();

                                    if (Debug != null)
                                    {
                                        if (Device.GameMode.Level.Player.Rank >= Debug.RequiredRank)
                                        {
                                            Debug.Process();
                                        }
                                        else
                                        {
                                            Debug.SendChatMessage("Debug command failed. Insufficient privileges.");
                                            return;
                                        }
                                    }
                                    else
                                        this.SendChatMessage($"Unknown command '{CommandName}'. Type '/help' for more information.");

                                    return;
                                }

                                this.Device.Chat.AddEntry(this.Device, this.Message);
                            }
                        }
                    }
                }
                
                this.Device.LastGlobalChatEntry = DateTime.Now;
            }
        }
    }
}