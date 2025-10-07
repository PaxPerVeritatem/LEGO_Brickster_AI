namespace LEGO_Brickster_AI;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var result = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "C:\\invalid|path"));
        Console.WriteLine(result);

    }

}