using System;
using System.Net;

public sealed class ServerInfo : EventArgs
{
    public IPEndPoint IP { get; set; }

    public byte PlayerCount { get; set; }
    public byte MaxPlayerCount { get; set; }

    public override string ToString()
    {
        return $"ServerInfo {{ IP: {IP}, PlayerCount: {PlayerCount}, MaxPlayerCount: {MaxPlayerCount} }}";
    }
}