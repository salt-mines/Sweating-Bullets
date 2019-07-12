namespace Networking
{
    internal static class Constants
    {
        public const int AppPort = 13456;
        public const string AppName = "saltfps";

        public const int MaxPlayers = 8;

        public const int TickRate = 64;
        public const int SendRate = 32;

        // Interpolate two packets' worth
        public const float Interpolation = 2 * (1f / SendRate);
    }
}