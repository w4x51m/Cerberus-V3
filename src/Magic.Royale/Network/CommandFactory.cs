using System;
using System.Collections.Generic;


namespace Magic.ClashOfClans.Network
{
    internal static class CommandFactory
    {
        public static readonly Dictionary<int, Type> Commands;

        static CommandFactory()
        {
            Commands = new Dictionary<int, Type>
            {
               
            };
        }
    }
}