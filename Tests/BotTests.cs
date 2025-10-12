namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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

    private const string TestUrl_3 = "https://www.bricklink.com/v3/studio/gallery.page";

    private readonly ITestOutputHelper _output = output;



    [Fact]
    public void InitializeBotTest()
    {
        Bot basicBot = new(TestUrl_1);
        Bot configuredBot = new(TestUrl_1, TestDownloadFolderPath);
        try
        {
            Assert.NotNull(basicBot.Driver);
            Assert.NotNull(configuredBot.Driver);
            string? testAbsDownloadFolderPath = Bot.GetAbsoluteDownloadFolderPath(TestDownloadFolderPath);
            Assert.NotNull(testAbsDownloadFolderPath);
            Assert.Equal(testAbsDownloadFolderPath, configuredBot.AbsDownloadFolderPath);
            basicBot.CloseBot();
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }
    private static void AccessTestWebPage(string TestUrl, Bot ConfiguredBot)
    {
        ConfiguredBot.GoToWebpage();

        //parse Testurl to make preliminary actions on specific test webpage. 
        switch (TestUrl)
        {
            case TestUrl_2:
                //press reject to cookies on google.com if TestUrl_2 is utilized
                IWebElement? RejectButton = ConfiguredBot.FindPageElement("//button[@id='W0wltc']", "xp");
                Assert.NotNull(RejectButton);
                Bot.ClickElement(RejectButton);
                break;

            case TestUrl_3:
                ChromeDriver testdriver = ConfiguredBot.Driver;
                Actions actionsBuilder = new(testdriver);
                // find and click the ageGateElement
                IWebElement? ageGateElement = ConfiguredBot.FindPageElement("//input[@class='blp-age-gate__input-field']", "xp");
                if (ConfiguredBot.WaitTillExists(ageGateElement))
                {
                    Bot.ClickElement(ageGateElement);
                    actionsBuilder.SendKeys("1");
                    actionsBuilder.SendKeys("9");
                    actionsBuilder.SendKeys("9");
                    actionsBuilder.SendKeys("4");
                    actionsBuilder.Perform();
                    actionsBuilder.Reset();
                }
                // find and press cookie button 
                IWebElement? cookieButton = ConfiguredBot.FindPageElement("//div[@class='cookie-notice__content']//button[contains(text(), 'Just necessary')]", "xp");
                if (ConfiguredBot.WaitTillExists(cookieButton))
                {
                    Bot.ClickElement(cookieButton);
                }
                break;

            default:
                break;
        }
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
    [InlineData("tableSearch", "name")]
    [InlineData("main-logo", "id")]
    [InlineData(".fi-ta-header-heading", "css")]
    [InlineData("fi-ta-header-heading", "class")]
    [InlineData("Metroliner", "lt")]
    [InlineData("//img[@id='main-logo']", "xp")]
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
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "xp", "xp")]
    public void FindElementWithAncestorTest(string TestUrl, string AncestorElementString, string DecendentElementString, string AncestorByMechanism, string DecendentByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);

        //local function
        static IList<string> InterpolateByMechanism(IWebElement element, string elementByMechanism)
        {
            // use null forgiveness in order to test
            IList<string> elementAttributeList = [];
            string? attributeString;
            attributeString = element.GetAttribute("class");
            elementAttributeList.Add("class");
            elementAttributeList.Add(attributeString!);
            return elementAttributeList;
        }

        try
        {
            configuredBot.GoToWebpage();

            // AncestorElement
            IWebElement? AncestorElement = configuredBot.FindPageElement(AncestorElementString, AncestorByMechanism);
            Assert.NotNull(AncestorElement);
            IList<string> a_AttributeList = InterpolateByMechanism(AncestorElement, AncestorByMechanism);
            Assert.Equal(2, a_AttributeList.Count);
            _output.WriteLine($"The value of the ancestor element {a_AttributeList[0]} attribute:{a_AttributeList[1]}");


            //DecendentElement
            IWebElement? DecendentElement = configuredBot.FindPageElement(DecendentElementString, DecendentByMechanism, AncestorElement);
            Assert.NotNull(DecendentElement);
            IList<string> d_AttributeList = InterpolateByMechanism(DecendentElement, DecendentByMechanism);
            Assert.Equal(2, d_AttributeList.Count);
            _output.WriteLine($"The value of the decendent element {d_AttributeList[0]} attribute:{d_AttributeList[1]})\n------------------------------");
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(TestUrl_1, 25, "fi-ta-cell-name", "class")]
    //[InlineData(TestUrl_3,)]
    public void FindElementsTest(string TestUrl, int ExpectedElementAmount, string ElementTypeString, string ByMechanism)
    {
        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        configuredBot.GoToWebpage();
        try
        {
            configuredBot.NameList = configuredBot.FindPageElements(ElementTypeString, ByMechanism);
            Assert.Equal(ExpectedElementAmount, configuredBot.NameList.Count);
        }
        finally
        {
            configuredBot.CloseBot();
        }
    }

    [Theory]
    [InlineData(TestUrl_1, "//a[@href='https://library.ldraw.org/documentation']",true)]
    [InlineData(TestUrl_2, "//div[contains(@class,'FPdoLc')]//input[@class='RNmpXc']",true)]
    [InlineData(TestUrl_3, "//div[@class='studio-staff-picks__bar l-margin-top--md']",false)]
    public void WaitTilExistsElementTest(string TestUrl, string ElementString,bool Goback)
    {

        Bot configuredBot = new(TestUrl, TestDownloadFolderPath);
        try
        {
            AccessTestWebPage(TestUrl, configuredBot);
            ClickElementTest(ElementString, configuredBot, Goback);
        }
        finally
        {
            configuredBot.CloseBot();
        }
        static void ClickElementTest(string ElementString, Bot testBot, bool GoBack)
        {
            IWebElement? clickableElement;
            if (GoBack)
            {
                for (int i = 0; i < 2; i++)
                {
                    clickableElement = testBot.FindPageElement(ElementString, "xp");
                    Assert.NotNull(clickableElement);
                    if (testBot.WaitTillExists(clickableElement))
                    {
                        Bot.ClickElement(clickableElement);
                        testBot.GoBack();
                    }
                }
            }
            else
            {
                clickableElement = testBot.FindPageElement(ElementString, "xp");
                if (testBot.WaitTillExists(clickableElement))
                {
                    Bot.ClickElement(clickableElement);
                }
            }
        }
    }

    [Fact]
    public void WaitTillExistsFalseTest()
    {
        Bot basic_bot = new(TestUrl_1);
        basic_bot.GoToWebpage();
        try
        {
            IWebElement? element = null;
            Assert.False(basic_bot.WaitTillExists(element));
        }
        finally
        {
            basic_bot.CloseBot();
        }
    }


    [Theory]
    [InlineData("//a[@href='https://library.ldraw.org/documentation']")]
    public void ClickElementExceptionTest(string elementString)
    {
        Bot basic_bot = new(TestUrl_1);
        basic_bot.GoToWebpage();
        IWebElement? element = basic_bot.FindPageElement(elementString, "xp");
        Assert.NotNull(element);
        try
        {
            basic_bot.GoToWebpage();
            Assert.Throws<BotElementException>(() => Bot.ClickElement(element));
        }
        finally
        {
            basic_bot.CloseBot();
        }
    }




    [Theory]
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "NOT_IMPLEMENTED_BY_MECHANISM", null, null)]
    [InlineData(TestUrl_1, "//h2[@class='fi-ta-header-heading']", "xp", ".//following::a[@href='https://library.ldraw.org/omr/sets/657']", "NOT_IMPLEMENTED_BY_MECHANISM")]
    public void BotMechanismExceptionTest(string TestURl, string FirstElementString, string FirstByMechanism, string? SecondElementString, string? SecondByMechanism)
    {
        Bot basicBot = new(TestURl);
        basicBot.GoToWebpage();
        try
        {

            // if we are testing for ancestor element pattern matching
            if (SecondElementString != null && SecondByMechanism != null)
            {

                IWebElement? FirstElement = basicBot.FindPageElement(FirstElementString, FirstByMechanism);
                Assert.NotNull(FirstElement);
                Assert.Throws<BotMechanismException>(() => basicBot.FindPageElement(SecondElementString, SecondByMechanism, FirstElement));
            }
            else
            {
                Assert.Throws<BotMechanismException>(() => basicBot.FindPageElement(FirstElementString, FirstByMechanism));
            }
        }
        finally
        {
            basicBot.CloseBot();
        }
    }

    [Theory]
    [InlineData("NO_SUCH_ELEMENT", "lt")]
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
    [InlineData(null, "lt")]
    public void NullFindElementTest(string? ElementString, string ByMechanism)
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
    [InlineData("//h2[@class='fi-ta-header-heading'", "xp")]
    public void InvalidSelectorTest(string ElementString, string ByMechanism)
    {
        Bot basicBot = new(TestUrl_1);
        try
        {
            basicBot.GoToWebpage();
            Assert.Throws<BotMechanismException>(() => basicBot.FindPageElement(ElementString, ByMechanism));
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
