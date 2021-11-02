namespace BrightChain.Engine.Helpers;

public static class DebugStatusHelper
{
    public static bool IsDebugMode
    {
        get
        {
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }
    }
}