namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
/* Quick Decision Guide  for tests

 - Single test case, no parameters? → Use [Fact]
 - Same test, multiple inputs? → Use [Theory] with [InlineData]
 - Complex or reusable test data? → Use [Theory] with [MemberData] or [ClassData]
 - Need to categorize tests? → Add [Trait] to any test
*/


public class BotTest
{
    private static readonly string downloadFolderPath = @"..\..\..\LEGO_Data";

    private static readonly string url = "https://library.ldraw.org/omr/sets";

    private readonly Bot _testBot;
    //private static readonly IWebElement? ancestorElement = null;
    //private static readonly string elementString = "fi-select-input";
    //private static readonly string byMechanism = "CLASSNAME";

    public BotTest()
    {
        _testBot = new(url, downloadFolderPath);
        _testBot.CloseBrowser(); // close browser so we dont have to do it manually for each test. 

    }






    /// <summary>
    /// Tests the Bot class constructor to ensure that a new instance of the class is created successfully.
    /// </summary>
    /// <remarks>
    /// This test creates a new instance of the Bot class and checks that it is not null.
    /// It also checks that the download folder path is set correctly.
    /// </remarks>
    [Theory]
    [InlineData(@"..\..\..\LEGO_Data")]
    public void BotInitializeTest(string testAbsDownloadFolderPath)
    {
        Assert.NotNull(_testBot);
        Assert.Equal(testAbsDownloadFolderPath, _testBot.AbsDownloadFolderPath);
        _testBot.Dispose();
    }

    [Fact]
    public void GoToWebpageTest()
    {
        _testBot.GoToWebpage(); 
    }


    // We try to locate the number of set elements pr page, either directly or 
    // via an ancestor element. 
    // IWebElement ancestorElement Xpath = "//div[contains(class(fi-pagination-records-per-page-select)"
    // IWebElement decendantElement Xpath = ".//following[contains](text("fi-input-wrp-content-ctn"))

    // [Theory]
    // // without AncestorElement
    // [InlineData("//div[contains(class() 'fi-input-wrp-content-ctn'", "//button[@aria-label='Go to page 59']", "XP")]
    // // with AncestorElement
    // public static int GetClickAmount(string SetsPrPageString, string TotalPagesString, string ByMechanism, IWebElement? AncestorElement = null)
    // {
    //     IWebElement? setsPrPageElement;
    //     IWebElement? TotalPagesElement;
    //     int setsPrPage;
    //     testBot.GoToWebpage(url);


    //     if (AncestorElement != null)
    //     {
    //         // find sets pr page value 
    //         setsPrPageElement = testBot.FindPageElement(SetsPrPageString, ByMechanism);
    //     }
    //     else
    //     {
    //         setsPrPageElement = testBot.FindPageElement(SetsPrPageString, ByMechanism, AncestorElement);
    //     }
    //     // find total pages element
    //     TotalPagesElement = testBot.FindPageElement(TotalPagesString, ByMechanism);


    //     // convert element text field to int32 
    //     setsPrPage = Convert.ToInt32(setsPrPageElement?.Text);
    //     Console.WriteLine($"Amount of LEGO sets pr page:{setsPrPage}");


    //     //convert text field to int32'
    //     int totalPages = Convert.ToInt32(TotalPagesElement?.Text);
    //     Console.WriteLine($"Amount of Pages:{totalPages}");


    //     int clickAmount = setsPrPage * totalPages;
    //     return clickAmount;
    // }




    // [Fact]
    // public static bool ExpectedClickAmount(int ClickAmount)
    // {
    //     GetClickAmount(testBot, byMechanism, ClickAmount,elementString, ancestorElement);



    //     }
    //     else
    //     {
    //         testBot.GoToWebpage(url);
    //         IWebElement? element = testBot.FindPageElement(ElementString, ByMechanism);
    //         Console.WriteLine(element.Text);
    //     }
    // }

}
