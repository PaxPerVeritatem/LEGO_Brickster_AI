import subprocess
import yaml
from pathlib import Path  
import shutil


def getNewReport(): 
    """
    This function runs all tests for the test project with the Xplat Code Coverage dotnet test flag.
    It will generate a new report in the TestResults folder based on the ReportGenerator_config.yaml file.
    The report will be generated in the targetdir folder specified in the ReportGenerator_config.yaml file.
    If a report already exists in the targetdir folder, it will be deleted and a new one will me made.
    default target dir also contains the .XML file for the report to be made from, hence the need to delete it.
    for every new report. 
    """
    
    subprocess.run([
        "dotnet", "test","--collect", "Xplat Code Coverage"
     ])

    with open("Tests/ReportGenerator_config.yaml") as f : 
        data = yaml.safe_load(f)
        reports= data["reports"] 
        targetdir = data ["targetdir"]
        #classfilters = data ["classfilters"]

    subprocess.run([
        "dotnet",
        "reportgenerator", 
        f"-reports:{reports}",
        f"-targetdir:{targetdir}"
        #f"-classfilters:{classfilters}"
     ])
    print("New html test report made\n")



if __name__ == "__main__": 
    test_result_folder = Path("Tests/TestResults")
    if(test_result_folder.exists()): 
        print("Old report found. Deleting old report\n")
        shutil.rmtree(test_result_folder)
        getNewReport()
    else: 
        getNewReport()
    


