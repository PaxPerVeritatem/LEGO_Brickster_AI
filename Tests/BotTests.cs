using LEGO_Brickster_AI;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Security.Cryptography.X509Certificates;

public class testBotTests
{
    private static readonly string downloadFolderString = @"C:\Users\admin\OneDrive\Skrivebord\LEGO_Brickster_AI\LEGO_Data";
    private static readonly string url = "https://library.ldraw.org/omr/sets";


    // setup a bot and check that it is not null.
    [Fact]
    public static void Setup()
    {
        Bot testBot = new(url, downloadFolderString);
        Assert.NotNull(testBot);
    }
    public static int GetClickAmount(Bot testBot,string ByMechanism, int ClickAmount,IWebElement ClickAmountElement, IWebElement? AncestorElement = null)
    {
        if (AncestorElement != null)
        {
            testBot.GoToWebpage(url); 
            testBot.FindPageElement()
        }
        else
        {
            testBot.GoToWebpage(url);
        }

        

    }


    [Fact]
    public static bool ExpectedClickAmount(int ClickAmount)
    {
        int clickAmount = getClickAmount(ClickAmount);
    }
}