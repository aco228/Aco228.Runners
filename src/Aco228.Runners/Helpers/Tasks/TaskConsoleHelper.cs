namespace Aco228.Runners.Helpers.Tasks;

public static class TaskConsoleHelper
{
    public static void Log(string message)
    {
        var date = DateTime.Now.ToString("HH:mm:ss"); 
        Console.WriteLine($"{date}:: {message}");
    }
    
    public static void Log(string sender, string message)
    {
        var date = DateTime.Now.ToString("HH:mm:ss"); 
        Console.WriteLine($"{date} [{sender}]:: {message}");
    }
}