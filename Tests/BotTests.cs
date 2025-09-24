namespace LEGO_Brickster_AI;

using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class BotTests
{
    private static readonly string downloadFolderString = @"C:\Users\admin\OneDrive\Skrivebord\LEGO_Brickster_AI\LEGO_Brickster_AI\LEGO_Data";
    private static readonly string url = "https://library.ldraw.org/omr/sets";

    private static readonly Bot testBot = new(url, downloadFolderString);
    //private static readonly IWebElement? ancestorElement = null;
    //private static readonly string elementString = "fi-select-input";
    //private static readonly string byMechanism = "CLASSNAME";


    // setup a bot and check that it is not null. 
    // Also check that the correct downloadPath is set, if it is defined in bot constructor.
    // Should be run before every test 

    [Fact]
    public void Setup()
    {
        Assert.NotNull(testBot);
        if (testBot.Downloadfolderstring != null && testBot.Options != null)
        {
           string output = "I am an output";

            Console.WriteLine(output);
            //Assert.Equal(testBot.Downloadfolderstring, downloadFolderString);
            testBot.StopBot();
        }
        else
        {
            //Assert.Equal(testBot.Downloadfolderstring, downloadFolderString);
        }
    }




    // [Fact]
    // public static bool ExpectedClickAmount(int ClickAmount)
    // {
    //     GetClickAmount(testBot, byMechanism, ClickAmount,elementString, ancestorElement);
    //     public static void GetClickAmount(Bot testBot, string ByMechanism, int ClickAmount, string ElementString, IWebElement? AncestorElement = null)
    // {
    //     if (AncestorElement != null)
    //     {
    //         testBot.GoToWebpage(url);


    //     }
    //     else
    //     {
    //         testBot.GoToWebpage(url);
    //         IWebElement? element = testBot.FindPageElement(ElementString, ByMechanism);
    //         Console.WriteLine(element.Text);
    //     }
    // }

}
