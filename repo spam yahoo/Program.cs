using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Reflection;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;
using System.Collections.Generic;


internal class Program
{
    private static Random random = new Random();
    static string emailFileName = "emails.txt";
    static string proxiesFileName = "proxies.txt";
    static string openedEmailFileName = "opendEmails.txt";
    
    private static async Task Main(string[] args)
    {



        DisplayLogo();
        CreateFileIfNotExists(emailFileName);
        CreateFileIfNotExists(proxiesFileName);
        CreateFileIfNotExists(openedEmailFileName);
        string profilesDirectory = createProfilesDirectory();
        await FirstMenu(profilesDirectory);
        








    }


    private static string createProfilesDirectory() 
    {
        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string profilesDirectory = Path.Combine(appDirectory, "ChromeProfiles");

        // Create the profiles directory if it doesn't exist
        if (!Directory.Exists(profilesDirectory))
        {
            Console.WriteLine("No profiles directory found.");
            Directory.CreateDirectory(profilesDirectory);
        }

      

        return profilesDirectory;
    }
    private static async Task  FirstMenu( string profilesDirectory)
    {
        string choice;
       
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Save a new profile");
            Console.WriteLine("2. Reporting Spam ");
            Console.WriteLine("3. Reporting Inbox");
            Console.WriteLine("4. Clear spam for existing profile");
        do
            {

                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SaveNewProfile(profilesDirectory);
                        break;
                    case "2":
                        await ReportNotSpamAsync(profilesDirectory);
                        break;
                    case "3":
                        await ReportInboxAsyn(profilesDirectory);
                        break;
                    case "4":
                        await ClearSpamProfilesAsync(profilesDirectory);
                        break;
                    default:
                            Console.WriteLine("Invalid choice.");
                            break;
                }
        } while (choice !="1" && choice!="2");
           
        
    }

    public static void SaveNewProfile(string profilesDirectory)
    {
        Console.WriteLine("Do you want to delete saved profiles :(yes/no) ");
        string choice = Console.ReadLine();
        if (choice.ToLower() == "yes" || choice.ToLower() == "y")
        {
            // Delete the profiles to create new ones 
            if (Directory.Exists(profilesDirectory))
                Directory.Delete(profilesDirectory, true);
        }
        OpenTextFile(emailFileName);

        int profileNumber = 1;
        Console.Write("enter Number of profiles you want to add : ");

        List<string> openedEmailsList= new List<string>();

        int NBofProfiles =int.Parse(Console.ReadLine());
        for (int i = 0; i < NBofProfiles; i++)
        {
            try
            {
                // Determine the next available profile number

                while (Directory.Exists(Path.Combine(profilesDirectory, profileNumber.ToString())))
                {
                    profileNumber++;
                }

                string profilePath = Path.Combine(profilesDirectory, profileNumber.ToString());

                // Proxy settings
                string proxyHost = "104.238.50.75";
                int proxyPort = 6621;



                // Set Chrome options
                ChromeOptions options = new ChromeOptions();
                //options.AddArguments($"--proxy-server=http://{proxyHost}:{proxyPort}");
                string userAgent = GetRandomUserAgent();
                options.AddArgument($"--user-agent={userAgent}");
                options.AddArgument($"--user-data-dir={profilePath}");


                // Initialize the ChromeDriver with the specified options
                IWebDriver driver = new ChromeDriver(options);

                //open yahoo spam link 
                string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
                //string autoITScriptPath = @"ProxyAuth.exe";
               // Thread.Sleep(2000);
               // Process.Start(autoITScriptPath);
               // Thread.Sleep(2000);



                //fill the login and password fields
                string filepath = filePath(emailFileName);
                string openedEmail = fillLoginFields(driver,i, filepath);


                openedEmailsList.Add(openedEmail);
                Console.WriteLine($"Profile '{profileNumber}' has been saved.");
            }
            catch (Exception)
            { 
                Console.WriteLine($"unexpacted error in the profile Number {profileNumber}");
            }
        }
        SaveListToFile(openedEmailsList, openedEmailFileName);
    }

    private static void SaveListToFile(List<string> list, string filePath)
    {
        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Overwrite the file with empty content
            File.WriteAllText(filePath, string.Empty);

            // Write each item in the list to the file (append mode)
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (string item in list)
                {
                    writer.WriteLine(item);
                }
            }

            Console.WriteLine("List has been saved to the file.");
        }
        else
        {
            Console.WriteLine("File does not exist.");
        }
    }




    static void OpenTextFile(string filePath)
    {
        try
        {
            Process.Start("cmd", $"/c start {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while opening the text file: {ex.Message}");
        }
    }

    public static async Task ReportNotSpamAsync(string profilesDirectory)
    {
        OpenTextFile(proxiesFileName);

        (int,int) fromTo = MenuOpenExistingProfile(profilesDirectory);
        int from = fromTo.Item1;
        int to = fromTo.Item2;
        List<Task> tasks = new List<Task>();
        for (int i = from-1; i < to; i++)
        {
            // Initialize ChromeDriver with the selected profile
            ChromeOptions options = optionProxy(i);
            string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
            options.AddArgument($"--user-data-dir={profile}");


            IWebDriver driver = new ChromeDriver(options);
            string url = "https://mail.yahoo.com/d/folders/6";
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
                string autoITScriptPath = @"ProxyAuth.exe";
            // Thread.Sleep(2000);
               Process.Start(autoITScriptPath);
               Thread.Sleep(2000);

            Console.WriteLine($"Profile '{i}' has been opened.");


            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            tasks.Add(Task.Run(() => ReportNotSpam(driver)));

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        }
        // Await either all tasks to complete or a timeout of 10 minutes, whichever comes first
        Task allTasks = Task.WhenAll(tasks);
        Task delayTask = Task.Delay(TimeSpan.FromMinutes(10));

        Task completedTask = await Task.WhenAny(allTasks, delayTask);

        if (completedTask == delayTask)
        {
            Console.WriteLine("Operation timed out.");
        }
        else
        {

        }
    }

    private static ChromeOptions optionProxy(int index)
    {
        ChromeOptions options = new ChromeOptions();
        Proxy proxy = new Proxy();

        try
        {
            string proxiesFilePath = filePath("proxies.txt");
            string proxyIp = extractfile(0, index, proxiesFilePath);
            int proxyPort = int.Parse(extractfile(1, index, proxiesFilePath));
            string ipAndPort = $"{proxyIp}:{proxyPort}";
            proxy.HttpProxy = ipAndPort;
            proxy.SslProxy = ipAndPort;

            options.Proxy = proxy;
        }
        catch
        {
            Console.WriteLine("--------------error in Proxies  (browser without proxy) !!");
        }
        return options;
    }


    public static async Task ReportInboxAsyn(string profilesDirectory)
    {
        (int, int) fromTo = MenuOpenExistingProfile(profilesDirectory);
        int from = fromTo.Item1;
        int to = fromTo.Item2;
        List<Task> tasks = new List<Task>();

        for (int i = from - 1; i < to; i++)
        {
            // Initialize ChromeDriver with the selected profile
            ChromeOptions options = new ChromeOptions();
            string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
            options.AddArgument($"--user-data-dir={profile}");
            IWebDriver driver = new ChromeDriver(options);
            string url = "https://mail.yahoo.com/d/folders/1";
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
            Console.WriteLine($"Profile '{i}' has been opened.");


            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            tasks.Add(Task.Run(() => InboxToArchive(driver)));

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        }
        // Await either all tasks to complete or a timeout of 10 minutes, whichever comes first
        Task allTasks = Task.WhenAll(tasks);
        Task delayTask = Task.Delay(TimeSpan.FromMinutes(10));

        Task completedTask = await Task.WhenAny(allTasks, delayTask);

        if (completedTask == delayTask)
        {
            Console.WriteLine("Operation timed out.");
        }
        else
        {
            Console.WriteLine("All tasks completed successfully.");
        }
    }


    //=================================================================================================================
    //=================================================================================================================
    //=================================================================================================================
    public static async Task ClearSpamProfilesAsync(string profilesDirectory)
    {
        (int, int) fromTo = MenuOpenExistingProfile(profilesDirectory);
        int from = fromTo.Item1;
        int to = fromTo.Item2;
        List<Task> tasks = new List<Task>();

        for (int i = from - 1; i < to; i++)
        {
            // Initialize ChromeDriver with the selected profile
            ChromeOptions options = new ChromeOptions();
            string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
            options.AddArgument($"--user-data-dir={profile}");
            IWebDriver driver = new ChromeDriver(options);
            string url = "https://mail.yahoo.com/d/folders/6";
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
            Console.WriteLine($"Profile '{i}' has been opened.");


            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            tasks.Add(Task.Run(() => ClearSpam(driver)));

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        }
        // Await either all tasks to complete or a timeout of 10 minutes, whichever comes first
        Task allTasks = Task.WhenAll(tasks);
        Task delayTask = Task.Delay(TimeSpan.FromMinutes(10));

        Task completedTask = await Task.WhenAny(allTasks, delayTask);

        if (completedTask == delayTask)
        {
            Console.WriteLine("Operation timed out.");
        }
        else
        {
            Console.WriteLine("All tasks completed successfully.");
        }
    }


    //=================================================================================================================
    //=================================================================================================================
    //================================================================================================================= 






    private static async Task ClearSpam(IWebDriver driver)
    {
        do
        {
            IWebElement checkboxButton = driver.FindElement(By.CssSelector("button[data-test-id='checkbox']"));
            checkboxButton.Click();
            Thread.Sleep(2000);
            IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-perm-delete']"));
            notSpamButton.Click();

        } while (true);

    }



    private static async Task ReportNotSpam(IWebDriver driver)
    {
        do
        {
            IWebElement checkboxButton = driver.FindElement(By.CssSelector("button[data-test-id='icon-btn-checkbox']"));
            checkboxButton.Click();
            Thread.Sleep(2000);
            IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-not-spam']"));
            notSpamButton.Click();

        } while (true);

    }


    private static async Task InboxToArchive(IWebDriver driver) 
    {
        do
        {
            IWebElement checkboxButton = driver.FindElement(By.CssSelector("button[data-test-id='icon-btn-checkbox']"));
            checkboxButton.Click();
            Thread.Sleep(2000);
            IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-archive']"));
            notSpamButton.Click();

        } while (true);

    }


    private static (int, int) MenuOpenExistingProfile(string profilesDirectory)
    {
        // List available profiles
        string[] profiles = Directory.GetDirectories(profilesDirectory);
        if (profiles.Length == 0)
        {
            Console.WriteLine("No profiles found.");
            FirstMenu(profilesDirectory);
        }
        

        Console.WriteLine("Available Profiles:");
        for (int i = 0; i < profiles.Length; i++)
        {
            Console.WriteLine($"{i + 1}: {Path.GetFileName(profiles[i])}");
        }

        string from=null;
        string to=null;
        

        do
        {
            Console.Write("Select a profile range \n From : ");
            from = Console.ReadLine();
        } while (!int.TryParse(from, out int input_int) || input_int <= 0 || input_int > profiles.Length);


        do
        {
            Console.Write(" To : ");
            to = Console.ReadLine();
        } while (!int.TryParse(to, out int input_int) || input_int < int.Parse(from) || input_int > profiles.Length);

        return (int.Parse(from),int.Parse(to));
    }


    private static string filePath(string filename)
    {


        // Get the current directory
        string currentDirectory = Directory.GetCurrentDirectory();

        // file
        string filePath = Path.Combine(currentDirectory, filename);
        


        return filePath;

    }
   

    private static void CreateFileIfNotExists(string filePath)
    {
        try
        {
            // Check if the file already exists
            if (File.Exists(filePath))
            {
                Console.WriteLine($"File '{Path.GetFileName(filePath)}' already exists in the following path:");
                Console.WriteLine(filePath);
            }
            else
            {
                // Create the file if it doesn't exist
                using (File.Create(filePath))

                    Console.WriteLine($"File '{Path.GetFileName(filePath)}' created successfully in the following path:");
                Console.WriteLine(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string fillLoginFields(IWebDriver driver, int index, string emailFilePath)
    {


        string email = extractfile(0, index, emailFilePath, driver);
        IWebElement loginInput = driver.FindElement(By.Id("login-username-form"));
        loginInput = driver.FindElement(By.Name("username"));
        loginInput.SendKeys(email);
        loginInput.SendKeys(Keys.Enter);
        Thread.Sleep(3000);
        try
        {
            IWebElement passInput = driver.FindElement(By.Id("login-passwd"));
            string password = extractfile(1, index, emailFilePath, driver);
            passInput.SendKeys(password);
            passInput.SendKeys(Keys.Enter);
        }
        catch (Exception)
        {

            Console.WriteLine("Password zone not found");
        }
        

        return email;
    }
    
    private static string extractfile(int zeroOrOne, int index, string filePath, IWebDriver driver = null)
    {
        string result = null;
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length > 0)
            {
                string[] parts = lines[index].Split(':');

                if (parts.Length > 1)
                {
                    result = parts[zeroOrOne];
                }
                else
                {
                    Console.WriteLine("--------------Invalid format in the file. Each line should be in the format 'text1:text2'.");
                    if (driver != null) driver.Quit();
                }
            }
            else
            {
                Console.WriteLine($"--------------The file is empty  {filePath}");
                if (driver != null) driver.Quit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("--------------An error occurred: " + ex.Message);
            if (driver != null) driver.Quit();
        }
        return result;
    }
 

    static void closeBrowser(IWebDriver driver)
    {
        if (IsWebDriverAlive(driver))
        {
            driver.Close();
            driver.Quit();
            driver = null;

        }
        driver = null;

        bool IsWebDriverAlive(IWebDriver driver)
        {
            try
            {
                // Check if the WebDriver instance is null or not closed
                return driver != null && driver.WindowHandles.Count > 0;
            }
            catch (WebDriverException)
            {
                // WebDriver is closed
                return false;
            }
        }
    }

    public static string GetRandomUserAgent()
    {
        // Dictionary or List of User Agents
        List<string> userAgents = new List<string>
        {
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_4) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_1) AppleWebKit/536.25 (KHTML, like Gecko) Version/6.0 Safari/536.25" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.2 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_0) AppleWebKit/536.25 (KHTML, like Gecko) Version/6.0 Safari/536.25" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/536.28.10 (KHTML, like Gecko) Version/6.0.4 Safari/536.28.10" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.1 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/536.26.17 (KHTML, like Gecko) Version/6.0.2 Safari/536.26.17" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_4) AppleWebKit/537.77.4 (KHTML, like Gecko) Version/7.0.3 Safari/537.77.4" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_4) AppleWebKit/536.26.17 (KHTML, like Gecko) Version/6.0.2 Safari/536.26.17" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_2) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_4) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_1) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/536.28.10 (KHTML, like Gecko) Version/6.0.4 Safari/536.28.10" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.2 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_5) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.1 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_0) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/537.77.4 (KHTML, like Gecko) Version/7.0.3 Safari/537.77.4" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.1 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_4) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A523 Safari/8536.25" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_5) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0 Safari/537.75.14" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/536.28.10 (KHTML, like Gecko) Version/6.0.4 Safari/536.28.10" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/536.26.17 (KHTML, like Gecko) Version/6.0.2 Safari/536.26.17" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_5) AppleWebKit/536.30.1 (KHTML, like Gecko) Version/6.0.5 Safari/536.30.1" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A403 Safari/8536.25" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_4) AppleWebKit/536.28.10 (KHTML, like Gecko) Version/6.0.4 Safari/536.28.10" ,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10B146 Safari/8536.25" ,
        };

        // Randomly select a user agent
        int index = random.Next(userAgents.Count);
        return userAgents[index];
    }


    static void DisplayLogo()
    {
        string logo = @"
  _____                    _       _                   _____  
 |  __ \                  | |     | |                 |  __ \ 
 | |  | |   ___    _   _  | |__   | |   ___   ______  | |  | |
 | |  | |  / _ \  | | | | | '_ \  | |  / _ \ |______| | |  | |
 | |__| | | (_) | | |_| | | |_) | | | |  __/          | |__| |
 |_____/   \___/   \__,_| |_.__/  |_|  \___|          |_____/ 
                                                              
                                                              ";
        Console.WriteLine(logo);
    }



}


