using BepInEx.Logging;

namespace UltraNightmare
{
    public static class Logging 
    {
        public static ManualLogSource UltraNightmareLogger = Logger.CreateLogSource("UltraNIGHTMARE");

        public static void Debug(string text)
        {
            UltraNightmareLogger.LogDebug(text);
        }
        
        public static void Message(string text)
        {
            UltraNightmareLogger.LogMessage(text);
        }
        
        public static void Warn(string text)
        {
            UltraNightmareLogger.LogWarning(text);
        }
        
        public static void Error(string text)
        {
            UltraNightmareLogger.LogError(text);
        }
        
        public static void Fatal(string text)
        {
            UltraNightmareLogger.LogFatal(text);
        }
        
        public static void Info(string text)
        {
            UltraNightmareLogger.LogInfo(text);
        }
    }
}