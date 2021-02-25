using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApi.Utils
{
    public enum BrcokerKeysTypes
    {
        online,
        message,
        statistic,
        readmessage
    }
    public static class BrockerKeysFactory
    {
        public static string GenerateQueueKey(string sessionId, BrcokerKeysTypes type)
        {
            return type switch
            {
                var t when t == BrcokerKeysTypes.message => $"{sessionId}-messagequeie",
                var t when t == BrcokerKeysTypes.online => $"{sessionId}-online",
                var t when t == BrcokerKeysTypes.statistic => $"{sessionId}-statistic",
                var t when t == BrcokerKeysTypes.readmessage => $"{sessionId}-readmessage",
                _=> throw new Exception("Exception key")
            };
        }

    }
}
