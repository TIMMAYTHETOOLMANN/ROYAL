namespace BotCore.Dto
{
    public static class StreamService
    {
        public enum Service
        {
            Twitch,
            Youtube,
            DLive,
            NimoTv,
            Twitter,
            Facebook,
            TrovoLive,
            BigoLive
        }
    }
}
namespace BotCore.Dto
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

