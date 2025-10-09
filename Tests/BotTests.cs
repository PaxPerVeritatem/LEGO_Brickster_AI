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
        finally
        {
            configuredBot.CloseBot(); 
        }
    }

    [Theory]
    //[InlineData(TestUrl_1, "pt_search_comp", ".//following::input[@placeholder='Quick Search']", "NAME", "XP")]
    //[InlineData(TestUrl_2, "gb", ".//a[@class='gb_Z']", "ID", "XP")]
    //[InlineData(TestUrl_2, "svg[aria-label='Google']", ".//following::input[@class='gNO89b']", "CSS", "XP")]
    [InlineData(TestUrl_2,"gb_y",".//a[@class='gb_Z']","CLASSNAME","XP")]
    //[InlineData(TestUrl_1,,,LT,XP)]
    //[InlineData(TestUrl_1, "//div[contains(text(),'Model')]", ".//following::a[contains(.,'Download')]", "XP", "XP")]






    public void FindElementWithAncestorTest(string TestUrl, string AncestorElementString, string DecendentElementString, string AncestorByMechanism, string DecendentByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        try
        {
            configuredBot.GoToWebpage();
            // kinda weird but fine for now
            if (Equals(TestUrl.ToLower(), "www.google.com"))
            {
                //press reject to cookies on google.com if TestUrl_2 is utilized
                var RejectButton = configuredBot.FindPageElement("//button[@id='W0wltc']", "XP");
                Bot.ClickElement(RejectButton);
            }

            // AncestorElement
            IWebElement? AncestorElement = configuredBot.FindPageElement(AncestorElementString, AncestorByMechanism);
            Assert.NotNull(AncestorElement);
            string? a_AttributeString = AncestorElement.GetAttribute($"{AncestorByMechanism.ToLower()}");
            _output.WriteLine($"The value of the ancestor {AncestorByMechanism} attribute: {a_AttributeString}");


            //DecendentElement
            IWebElement? DecendentElement = configuredBot.FindPageElement(DecendentElementString, DecendentByMechanism, AncestorElement);
            Assert.NotNull(DecendentElement);
            string? d_AttributeString;
            // Handle string interpolation for "XP" byMechanism
            if (Equals(DecendentElement.GetAttribute($"{AncestorByMechanism.ToLower()}"), "xp"))
            {
                // hope there is a class if xp 
                d_AttributeString = DecendentElement.GetAttribute("class");
            }
            else
            { 
                d_AttributeString = DecendentElement.GetAttribute($"{AncestorByMechanism.ToLower()}");
            }
            _output.WriteLine($"The value of the decended {DecendentByMechanism} attribute: {d_AttributeString}\n------------------------------");
        }
        finally
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
        try
        {
            basicBot.GoToWebpage();
            Assert.Throws<BotElementException>(() => basicBot.FindPageElement(ElementString, ByMechanism));
        }
        finally
        {
            basicBot.CloseBot();
        }

    }


    [Theory]
    [InlineData(null, "LT")]
    public void NullElementTest(string? ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.GoToWebpage();
            Assert.Throws<BotElementException>(() => basicBot.FindPageElement(ElementString!, ByMechanism));
        }
        finally
        {
            basicBot.CloseBot();
        }


    }

    [Theory]
    [InlineData("www.NotAWebsiteForBots.Bot.com")]
    public void InvalidUrlTest(string InvalidUrl)
    {
        Bot basicBot = new(InvalidUrl);
        try
        {
            Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(null)]
    public void NullUrlTest(string? NullUrl)
    {
        // use null forgiveness to actually make test possible 
        Bot basicBot = new(NullUrl!);
        try
        {
            Assert.Throws<BotUrlException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
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
        try
        {
            basicBot.CloseBotBrowser();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Fact]
    public void CloseDriverTest()
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.CloseBotDriver();
            // use a Action type with delegator to reference the method
            Assert.Throws<BotDriverException>(basicBot.GoToWebpage);
        }
        finally
        {
            basicBot.CloseBot();
        }



    }


    [Fact]
    public void GetChromeOptionsTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        try
        {
            ChromeOptions? actualOptions = configuredBot.Options;
            Assert.NotNull(actualOptions);
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }


    [Fact]
    public void GetNameListTest()
    {
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath)
        {
            NameList = ["Test_NameList"]
        };
        try
        {
            string[] expectedList = ["Test_NameList"];
            Assert.NotNull(configuredBot.NameList);
            Assert.Equal(expectedList, configuredBot.NameList);
        }
        finally
        {
            configuredBot.CloseBot();
        }
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
