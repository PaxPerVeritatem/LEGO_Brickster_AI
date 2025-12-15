namespace LEGO_Brickster_AI;


static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //GUI turned off for now 
        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());
        
        //GetDataLdraw.ProcessData();
        //GetDataBrickLink.ProcessData();
        DataConverter.ConvertFiles(); 

        
    }

}