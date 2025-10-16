interface GetData
{
    private static string _url;

    private static readonly bool _customRun = false;


    /// <summary>
    /// int defining the int value of the page. Can be utilized if the website have several pages of individual data 
    /// </summary>
    private static readonly int _startFromPage;

    /// <summary>
    ///  int defining the number of sets of data pr page. Usage of this field depends on the structure of the website on question. 
    /// </summary>
    private static readonly int _setsPrPage;


    /// <summary>
    /// int defining the last page of a custom run to scape data from (intended to be used inclusively) 
    /// </summary>
    private static readonly int _pageLimit;


    /// <summary>
    /// a substring serving as an extension the main website url exte which allows can be combined with <param name = "_startFromPage"> to begin a run from a certain subpage.  
    /// </summary>
    private readonly static string _urlPageVarient;

    /// <summary>
    /// int defining the expected amount of elements to be clicked, inferred from <param name = "_setsPrPage"> and <param name = "_pageLimit">
    /// </summary>
    private static readonly int _expectedElementClickAmount = _setsPrPage * _pageLimit;


    /// <summary>
    /// A simple reference counter for asserting correct amount of elements have been clicked, when finally compared to <param name = "_expectedElementClickAmount">
    private static int _elementClickAmount;



    /// <summary>
    ///  The absolute path to the download folder, when the Bot class is initialized with a download folder path. Should be definied with enough .. to reach the the desired download folder path, from the application's .dll file directory.
    /// </summary>
    private static readonly string _downloadFolderPath;


    /// <summary>
    /// Allows to define a custom starting page for a custom run, using the initial <param name = "url">
    /// with the <param name = "_urlPageVarient"> and param name = "_startFromPage"> to define the url of the custom run page. 
    /// </summary>
    private static void UseCustomStartingPage()
    {
        _url = $"{_url}{_urlPageVarient}{_startFromPage}";
    }



    // The main entry point for the interface, used to initiate the process. 
    public static void GetData()
    {

    }


    public static void AccessTestWebPage(Bot bot)
    {
        
    }
}