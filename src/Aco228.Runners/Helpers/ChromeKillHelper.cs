using Aco228.Common.Helpers;

namespace Aco228.Runners.Helpers;

public class ChromeKillHelper
{
    public static bool IgnoreKill = false;
    
    public static void TryKill()
    {
        if(IgnoreKill)
            return;
        
        CoreKillProcess.TryKill("chrome");
    }
}