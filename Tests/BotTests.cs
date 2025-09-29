namespace Tests;

using LEGO_Brickster_AI;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class BotTests
{
    private static readonly string downloadFolderString = @"..\..\..\LEGO_Data";

    private static readonly string url = "https://library.ldraw.org/omr/sets";

    public static readonly Bot testBot = new(url, downloadFolderString);
    //private static readonly IWebElement? ancestorElement = null;
    //private static readonly string elementString = "fi-select-input";
    //private static readonly string byMechanism = "CLASSNAME";






    [Fact]
    public void BotInitializeTest()
    {
        Assert.NotNull(testBot);
        if (testBot.DownloadFolderPath != null && testBot.Options != null)
        {
            Assert.Equal(testBot.DownloadFolderPath, downloadFolderString);
        }
        else
        {
            Assert.Equal(testBot.DownloadFolderPath, downloadFolderString);
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
