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

public sealed class BotTest(ITestOutputHelper output)
{
    private const string TestDownloadFolderPath = @"..\..\..\LEGO_Data";

    private const string TestUrl = "https://library.ldraw.org/omr/sets";

    private readonly ITestOutputHelper _output = output;



    [Fact]
    public void InitializeBotTest()
    {
        Bot basicBot = new(TestUrl);
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        Assert.NotNull(basicBot.Driver);

        Assert.NotNull(configuredBot.Driver);
        string testAbsDownloadFolderPath = Bot.GetAbsoluteDownloadFolderPath(TestDownloadFolderPath);
        Assert.Equal(testAbsDownloadFolderPath, configuredBot.AbsDownloadFolderPath);
        basicBot.CloseBot();
        configuredBot.CloseBot();
    }


    [Fact]
    public void GoToWebpageTest()
    {
        Bot basicBot = new(TestUrl);
        basicBot.GoToWebpage();
        Assert.Equal(TestUrl, basicBot.Driver.Url);
        basicBot.CloseBot();
    }



    [Theory]
    [InlineData("tableSearch", "NAME")]
    [InlineData("main-logo", "ID")]
    [InlineData(".fi-ta-header-heading", "CSS")]
    [InlineData("fi-ta-header-heading", "CLASSNAME")]
    [InlineData("Metroliner", "LT")]
    [InlineData("//img[@id='main-logo']", "XP")]
    public void FindElementTest(string ElementString, string ByMechanism)
    {

        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        configuredBot.GoToWebpage();
        IWebElement? pageElement = configuredBot.FindPageElement(ElementString, ByMechanism);
        // make sure the IWeb element is not null.
        Assert.NotNull(pageElement);
        string? elementOuter = pageElement.GetAttribute("outerHTML");
        _output.WriteLine($"{ElementString} outer HTML via {ByMechanism}:\n {elementOuter}\n------------------------------------------------\n");
        configuredBot.CloseBot();
    }

    
    [Theory]
    [InlineData("Metroliner", "NOT_IMPLEMENTED_MECHANISM")]
    public void BotMechanismExceptionTest(string ElementString, string ByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        configuredBot.GoToWebpage();
        Assert.Throws<BotMechanismException>(() =>configuredBot.FindPageElement(ElementString, ByMechanism));
        configuredBot.CloseBot();
    }



    [Fact]
    public void FindElementsTest()
    {

    }

    //[Theory]
    //[InlineData]
    //public void ClickElementTest()
    //{
    //    if (configuredBot.WaitTillExists(pageElement))
    //    {
    //        Bot.ClickElement(pageElement);
    //    }
    //}

    [Fact]
    public void CloseBrowserTest()
    {
        Bot basicBot = new(TestUrl);
        basicBot.CloseBrowser();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        basicBot.CloseBot();
    }

    [Fact]
    public void CloseDriverTest()
    {
        Bot basicBot = new(TestUrl);
        basicBot.CloseDriver();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        basicBot.CloseBot();
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
