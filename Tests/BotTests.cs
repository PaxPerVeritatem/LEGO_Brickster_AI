namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using Xunit.Sdk;
using OpenQA.Selenium.Chrome;

/* Quick Decision Guide  for tests

- Single test case, no parameters? → Use [Fact]
- Same test, multiple inputs? → Use [Theory] with [InlineData]
- Complex or reusable test data? → Use [Theory] with [MemberData] or [ClassData]
- Need to categorize tests? → Add [Trait] to any test
*/

public sealed class BotTest(ITestOutputHelper output)
{
    private const string TestDownloadFolderPath = @"..\..\..\LEGO_Data";

    private const string TestUrl_1 = "https://library.ldraw.org/omr/sets";

    private const string TestUrl_2 = "https://www.google.com";

    private readonly ITestOutputHelper _output = output;



    [Fact]
    public void InitializeBotTest()
    {
        Bot basicBot = new(TestUrl_1);
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        Assert.NotNull(basicBot.Driver);
        Assert.NotNull(configuredBot.Driver);

        string? testAbsDownloadFolderPath = Bot.GetAbsoluteDownloadFolderPath(TestDownloadFolderPath);
        Assert.NotNull(testAbsDownloadFolderPath);
        Assert.Equal(testAbsDownloadFolderPath, configuredBot.AbsDownloadFolderPath);
        basicBot.CloseBot();
        configuredBot.CloseBot();
    }

    [Fact]
    public void GoToWebpageTest()
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.GoToWebpage();
            Assert.Equal(TestUrl_1, basicBot.Driver.Url);
            basicBot.CloseBot();
        }
        catch (BotUrlException)
        {
            basicBot.CloseBot();
        }
        catch (BotDriverException)
        {
            basicBot.CloseBot();
        }

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

        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        configuredBot.GoToWebpage();
        try
        {
            IWebElement? pageElement = configuredBot.FindPageElement(ElementString, ByMechanism);
            // make sure the IWeb element is not null.
            Assert.NotNull(pageElement);
            string? elementOuter = pageElement.GetAttribute("outerHTML");
            _output.WriteLine($"{ElementString} outer HTML via {ByMechanism}:\n {elementOuter}\n------------------------------------------------\n");
            configuredBot.CloseBot();
        }
        catch (BotElementException)
        {

            configuredBot.CloseBot();
        }
        catch (BotMechanismException)
        {
            configuredBot.CloseBot();
        }

    }

    [Theory]
    [InlineData(TestUrl_1, "//div[contains(text(),'Model')]", ".//following::a[contains(.,'Download')]", "XP", "XP")]
    [InlineData(TestUrl_2, "svg[aria-label='Google']", "//following::input[@class='gNO89b']", "CSS", "XP")]

    public void FindElementWithAncestorTest(string TestUrl, string AncestorElementString, string DecendentElementString, string AncestorByMechanism, string DecendentByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        try
        {
            configuredBot.GoToWebpage();
            // niche test but fine for now
            if (Equals(TestUrl.ToLower(), "www.google.com"))
            {
                //press reject to cookies on google.com if 
                var RejectButton = configuredBot.FindPageElement("//button[@id='W0wltc']", "XP");
                Bot.ClickElement(RejectButton);
            }

            IWebElement? AncestorElement = configuredBot.FindPageElement(AncestorElementString, AncestorByMechanism);
            Assert.NotNull(AncestorElement);
            string? A_valueAttributeString = AncestorElement.GetAttribute("value");
            _output.WriteLine($"The text of the Google.com webpage picture: {A_valueAttributeString}");


            IWebElement? DecendentElement = configuredBot.FindPageElement(DecendentElementString, DecendentByMechanism, AncestorElement);
            Assert.NotNull(DecendentElement);
            string? D_valueAttributeString = DecendentElement.GetAttribute("value");
            _output.WriteLine($"The text  of the decended element value attribute {D_valueAttributeString}");


            configuredBot.CloseBot();
        }
        catch (BotElementException)
        {

            configuredBot.CloseBot();
        }
        catch (BotMechanismException)
        {
            configuredBot.CloseBot();
        }
    }





    [Fact]
    public void FindElementsTest()
    {

    }


    [Theory]
    [InlineData("Metroliner", "NOT_IMPLEMENTED_MECHANISM")]
    public void BotMechanismExceptionTest(string ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestUrl_1);
        basicBot.GoToWebpage();
        Assert.Throws<BotMechanismException>(() => basicBot.FindPageElement(ElementString, ByMechanism));
        basicBot.CloseBot();
    }

    [Theory]
    [InlineData("NO_SUCH_ELEMENT", "LT")]
    public void NoSuchElementExceptionTest(string ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestUrl_1);
        basicBot.GoToWebpage();
        Assert.Throws<BotElementException>(() => basicBot.FindPageElement(ElementString, ByMechanism));
        basicBot.CloseBot();
    }


    [Theory]
    [InlineData(null, "LT")]
    public void NullElementTest(string? ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestUrl_1);
        basicBot.GoToWebpage();
        Assert.Throws<BotElementException>(() => basicBot.FindPageElement(ElementString!, ByMechanism));
        basicBot.CloseBot();
    }

    [Theory]
    [InlineData("www.NotAWebsiteForBots.Bot.com")]
    public void InvalidUrlTest(string InvalidUrl)
    {
        Bot basicBot = new(InvalidUrl);
        Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        basicBot.CloseBot();
    }

    [Theory]
    [InlineData(null)]
    public void NullUrlTest(string? NullUrl)
    {
        Bot basicBot = new(NullUrl!);
        Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        basicBot.CloseBot();

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
        Bot basicBot = new(TestUrl_1);
        basicBot.CloseBotBrowser();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        basicBot.CloseBot();
    }

    [Fact]
    public void CloseDriverTest()
    {
        Bot basicBot = new(TestUrl_1);
        basicBot.CloseBotDriver();
        // use a Action type with delegator to reference the method
        Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        basicBot.CloseBot();
    }


    [Fact]
    public void GetChromeOptionsTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        ChromeOptions? actualOptions = configuredBot.Options;
        Assert.NotNull(actualOptions);
        configuredBot.CloseBot();
    }


    [Fact]
    public void GetNameListTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath)
        {
            NameList = ["Test_NameList"]
        };
        string[] expectedList = ["Test_NameList"];
        Assert.NotNull(configuredBot.NameList);
        Assert.Equal(expectedList, configuredBot.NameList);
        configuredBot.CloseBot();
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
