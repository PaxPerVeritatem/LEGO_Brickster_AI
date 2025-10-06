namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using Xunit.Sdk;

/* Quick Decision Guide  for tests

- Single test case, no parameters? → Use [Fact]
- Same test, multiple inputs? → Use [Theory] with [InlineData]
- Complex or reusable test data? → Use [Theory] with [MemberData] or [ClassData]
- Need to categorize tests? → Add [Trait] to any test
*/

public sealed class BotTest : IDisposable
{
    private const string TestDownloadFolderPath = @"..\..\..\LEGO_Data";

    private const string TestUrl = "https://library.ldraw.org/omr/sets";

    private readonly Bot _basicBot ; 
    private readonly Bot _configuredBot; 

    private readonly TestOutputHelper _testOutput;






    public BotTest()
    {
        _testOutput = new TestOutputHelper();
        _basicBot = new(TestUrl);
        _configuredBot = new(TestUrl, TestDownloadFolderPath);
    }

    public void Dispose()
    {
        _basicBot.CloseBot();
        _configuredBot.CloseBot();
    }



    [Fact]
    public void InitializeBotTest()
    {
        Assert.NotNull(_basicBot.Driver);

        Assert.NotNull(_configuredBot.Driver);
        string testAbsDownloadFolderPath = Bot.GetAbsoluteDownloadFolderPath(TestDownloadFolderPath);
        Assert.Equal(testAbsDownloadFolderPath, _configuredBot.AbsDownloadFolderPath);
    }


    [Fact]
    public void GoToWebpageTest()
    {
        _basicBot.GoToWebpage();

        Assert.Equal(TestUrl, _basicBot.Driver.Url);
    }



    [Theory]
    [InlineData("tableSearch", "NAME")]
    //[InlineData("//a[@href='https://www.ldraw.org']", "ID")]
    //[InlineData("//a[@href='https://www.ldraw.org']", "CSS")]
    //[InlineData("//a[@href='https://www.ldraw.org']", "CLASSNAME")]
    //[InlineData("//a[@href='https://www.ldraw.org']", "LT")]
    [InlineData("//a[@href='https://www.ldraw.org']", "XP")]
    public void FindElementTest(string ElementString, string ByMechanism)
    {
       
        _configuredBot.GoToWebpage();
        IWebElement? pageElement = _configuredBot.FindPageElement(ElementString, ByMechanism);
        // make sure the IWeb element is not null. 
        Assert.NotNull(pageElement);
        string elementText = pageElement.Text; 
        _testOutput.WriteLine(elementText); 
        if (_configuredBot.WaitTillExists(pageElement))
        {
            Bot.ClickElement(pageElement);
        }   
    }

    [Fact]
    public void FindElementsTest()
    { 
        
    }

    [Fact]
    public void CloseBrowserTest()
    {

        _basicBot.CloseBrowser();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(_basicBot.GoToWebpage);
    }

    [Fact]
    public void CloseDriverTest()
    {
        _basicBot.CloseDriver();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(_basicBot.GoToWebpage);
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
