using OpenQA.Selenium;

namespace LEGO_Brickster_AI;

// static abstract interface
interface IGetData
{
    public static abstract string Url { get; set; }

    static abstract bool CustomRun { get; }
    /// <summary>
    /// int defining the int value of the page. Can be utilized if the website have several pages of individual data 
    /// </summary>
    static abstract int StartFromPage { get; }

    /// <summary>
    ///  int defining the number of sets of data pr page. Usage of this field depends on the structure of the website on question. 
    /// </summary>
    static abstract int SetsPrPage { get; }


    /// <summary>
    /// int defining the last page of a custom run to scape data from (intended to be used inclusively) 
    /// </summary>
    static abstract int PageLimit { get; }


    /// <summary>
    /// a substring serving as an extension the main website url exte which allows can be combined with <param name = "StartFromPage"> to begin a run from a certain subpage.  
    /// </summary>
    static abstract string UrlPageVarient { get; }


    /// <summary>
    /// Depending on the webpage, and its responsiveness, there might be several next page elements. Depending on the page responsiveness. 
    /// Keys represent the litteral ElementString  while the values is the ByMechanism string utilized by the FindElement function.   
    /// bot may fail to locate a certain NextPage element and throw a WebDriverTimeoutException, which should be handled appropricately, 
    /// allowing for the remaing buttons to be found in the case of webpage responsiveness issues. 
    /// </summary>
    static abstract Dictionary<string,string> NextPageElements { get;}

    /// <summary>
    /// A simple reference counter for asserting correct amount of elements have been clicked, when finally compared to <param name = "ExpectedElementClickAmount">
    static abstract int ElementClickCounter { get; set; }



    /// <summary>
    ///  The absolute path to the download folder, when the Bot class is initialized with a download folder path. Should be definied with enough .. to reach the the desired download folder path, from the application's .dll file directory.
    /// </summary>
    static abstract string DownloadFolderPath { get; }

    /// <summary>
    /// int defining the expected amount of elements to be clicked, inferred from <param name = "SetsPrPage"> and <param name = "PageLimit">
    /// </summary>
    static abstract int ExpectedElementClickAmount { get; }


    /// <summary>
    /// Allows to define a custom starting page for a custom run, using the initial <param name = "url">
    /// with the <param name = "UrlPageVarient"> and param name = "_startFromPage"> to define the url of the custom run page. 
    /// </summary>
    public static abstract void UseCustomStartingPage();

    // function to access the main webpage, and perform any preliminary Bot actions to escape any pop-ups, that may occur. 
    public static abstract void AccessWebPage(Bot bot);

    /// <summary>
    /// function to iterate over the NextPageElements<string,string> maps key value pairs. For each key value pair it calls 
    /// bot.FindElement() to locate a button on the webpage which should function as a next button. The predetermined amount of tuples in the 
    /// map should be manually defined by in the NextPageElements map.  
    /// </summary>
    /// <param name="bot"></param>
    public static abstract IWebElement? GetNextPageElement(Bot bot);


    // The main entry point for the interface, used to initiate the process. 
    public static abstract void ProcessData();

}