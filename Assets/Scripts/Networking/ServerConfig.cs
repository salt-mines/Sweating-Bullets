using Game;

namespace Networking
{
    public class ServerConfig
    {
        public byte MaxPlayerCount { get; set; }
        public string StartingLevel { get; set; }
        public GameMode GameMode { get; set; }
    }
}