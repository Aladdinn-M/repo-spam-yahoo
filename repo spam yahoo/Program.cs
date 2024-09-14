    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Support.UI;
    using System;
    using System.Reflection;
    using System.Web;
    using System.Text;
    using SeleniumExtras.WaitHelpers;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using System.Numerics;


    internal class Program
    {
        private static Random random = new Random();
        static string emailFileName = "emails.txt";
        static string proxiesFileName = "proxies.txt";
        static string openedEmailFileName = "opendEmails.txt";
    
        private static async Task Main(string[] args)
        {

        
            CreateFileIfNotExists(emailFileName);
            CreateFileIfNotExists(proxiesFileName);
            CreateFileIfNotExists(openedEmailFileName);
            string profilesDirectory = createProfilesDirectory();
            await FirstMenu(profilesDirectory);
        
            Console.ReadKey();
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
            int number;
       
            do
                {
                DisplayLogo();
                Console.WriteLine("==========Menu==========");
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. open profiles");
                Console.WriteLine("2. open profiles with proxy");
                Console.WriteLine("3. Reporting Spam ");
                Console.WriteLine("4. Reporting Inbox");
                Console.WriteLine("5. Clean spam ");
                Console.WriteLine("6. Clean inbox ");
                Console.WriteLine("7. Save a new profiles");
                Console.WriteLine("========================");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        OpenProfiles(profilesDirectory);
                        break;
                    case "2":
                        await ReportNotSpamAsync(profilesDirectory);
                        Console.Clear();
                        Console.WriteLine("==================Reporting Spam ends==================");
                        FirstMenu(profilesDirectory);
                        break;
                    case "3":
                        await ReportInboxAsyn(profilesDirectory);
                        Console.Clear();
                        Console.WriteLine("==================Reporting Inbox ends==================");
                        FirstMenu(profilesDirectory);
                         break;
                    case "4":
                        await ClearSpamProfilesAsync(profilesDirectory);
                        Console.Clear();
                        Console.WriteLine("==================Spam is clean==================");
                        FirstMenu(profilesDirectory);
                    break;
                    case "5":
                         await ClearInboxProfilesAsync(profilesDirectory);
                        Console.Clear();
                        Console.WriteLine("==================Inbox is clean==================");
                        FirstMenu(profilesDirectory);
                    break;
                    case "6":
                        SaveNewProfile(profilesDirectory);
                        break;
                    
                    default:
                        Console.WriteLine("Invalid choice.");
                    break;
                }
        } while (string.IsNullOrEmpty(choice) || int.TryParse(choice, out number) || number > 1 || number < 6);


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
                    //string proxyHost = "geo.iproyal.com";
                   // int proxyPort = 12321;



                    // Set Chrome options
                    ChromeOptions options = new ChromeOptions();

                    // options.AddArguments($"--proxy-server=http://{proxyHost}:{proxyPort}");
                    string userAgent = GetRandomUserAgent();
                    options.AddArgument($"--user-agent={userAgent}");
                    options.AddArgument($"--user-data-dir={profilePath}");
                    options.AcceptInsecureCertificates = true;
    


                // Initialize the ChromeDriver with the specified options
                IWebDriver driver = new ChromeDriver(options);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                     driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts


                //open yahoo spam link 
                string url = "https://mail.yahoo.com/d/folders/6";
                    driver.Manage().Window.Maximize();
                    driver.Navigate().GoToUrl(url);
                   // string autoITScriptPath = @"login_pass_geoiproyalcom_12321.exe";
                   // Thread.Sleep(2000);
                    //Process.Start(autoITScriptPath);
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



        public static  void OpenProfiles(string profilesDirectory)
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
                options.AcceptInsecureCertificates = true;



            IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts



            string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
            }
        }

        public static void OpenProfilesProxy(string profilesDirectory)
        {
            OpenTextFile(proxiesFileName);

            (int, int) fromTo = MenuOpenExistingProfile(profilesDirectory);
            int from = fromTo.Item1;
            int to = fromTo.Item2;
            List<Task> tasks = new List<Task>();


            // Get the handle for the console window
            IntPtr consoleWindowHandle = GetConsoleWindow();


            for (int i = from - 1; i < to; i++)
            {
                // Initialize ChromeDriver with the selected profile
                ChromeOptions options = optionProxy(i);
                string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
                options.AddArgument($"--user-data-dir={profile}");


                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts

            string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
                string autoITScriptPath = @"ProxyAuth.exe";
                Thread.Sleep(2000);
                Process.Start(autoITScriptPath);
                Thread.Sleep(2000);

                // Switch focus back to the console window
                SetForegroundWindow(consoleWindowHandle);

                Console.WriteLine($"Profile '" + (i + 1).ToString() + "' has been opened.");
            }

        }

        // Import the SetForegroundWindow function from user32.dll
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Import the GetConsoleWindow function from kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();


        public static async Task ReportNotSpamAsync(string profilesDirectory)
        {
            OpenTextFile(proxiesFileName);

            (int,int) fromTo = MenuOpenExistingProfile(profilesDirectory);
            int from = fromTo.Item1;
            int to = fromTo.Item2;
            List<Task> tasks = new List<Task>();


            // Get the handle for the console window
            IntPtr consoleWindowHandle = GetConsoleWindow();


            for (int i = from-1; i < to; i++)
            {
                // Initialize ChromeDriver with the selected profile
                ChromeOptions options = optionProxy(i);
                string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
                options.AddArgument($"--user-data-dir={profile}");


                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts

            string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
                string autoITScriptPath = @"ProxyAuth.exe";
                // Thread.Sleep(2000);
                Process.Start(autoITScriptPath);
                Thread.Sleep(2000);

                // Switch focus back to the console window
                SetForegroundWindow(consoleWindowHandle);

                Console.WriteLine($"Profile '" + (i + 1).ToString() + "' has been opened.");

            //==========================================Asyn Tasks==============================================
            
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

   

        public static async Task ReportInboxAsyn(string profilesDirectory)
        {
            OpenTextFile(proxiesFileName);

            (int, int) fromTo = MenuOpenExistingProfile(profilesDirectory);
            int from = fromTo.Item1;
            int to = fromTo.Item2;
            List<Task> tasks = new List<Task>();

            // Get the handle for the console window
            IntPtr consoleWindowHandle = GetConsoleWindow();

            for (int i = from - 1; i < to; i++)
            {
                // Initialize ChromeDriver with the selected profile
                ChromeOptions options = optionProxy(i);
                string profile = Path.Combine(profilesDirectory, (i + 1).ToString());
                options.AddArgument($"--user-data-dir={profile}");


                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts

            string url = "https://mail.yahoo.com/d/folders/1";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);

                string autoITScriptPath = @"ProxyAuth.exe";
            
                Process.Start(autoITScriptPath);
                Thread.Sleep(2000);

                // Switch focus back to the console window
                SetForegroundWindow(consoleWindowHandle);

                Console.WriteLine($"Profile '" + (i + 1).ToString() + "' has been opened.");

                //==========================================Asyn Tasks==============================================
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
                options.AcceptInsecureCertificates = true;
    

            IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts

            string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);

                Console.WriteLine($"Profile '" + (i + 1).ToString() + "' has been opened.");

                //==========================================Asyn Tasks==============================================
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



        public static async Task ClearInboxProfilesAsync(string profilesDirectory)
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
                options.AcceptInsecureCertificates = true;


            IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60); // Increase page load timeout
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); // Timeout for async scripts

            string url = "https://mail.yahoo.com/d/folders/6";
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);

                Console.WriteLine($"Profile '" + (i + 1).ToString() + "' has been opened.");

                //==========================================Asyn Tasks==============================================
                //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                tasks.Add(Task.Run(() => ClearInbox(driver)));
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


   

        private static async Task ReportNotSpam(IWebDriver driver)
        {
        try
        {
            while (IsNotEmpty(driver))
            {
                // Wait for the email list to load
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("a[data-test-id='message-list-item']")));

                // Locate the specific email
                IWebElement Email = driver.FindElement(By.XPath("//a[@data-test-id='message-list-item']"));
                Email.Click();

                Thread.Sleep(2000);

                // Locate the "Not Spam" button and click it
                IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-not-spam']"));
                notSpamButton.Click();

                IWebElement spamfolder = driver.FindElement(By.CssSelector("a[data-test-folder-name='Bulk']"));
                spamfolder.Click();
            }
            driver.Close();
            driver.Quit();  // Properly close the browser
            driver.Dispose();  // Dispose of the WebDriver instance
        }
        catch (Exception ex) 
        {
            await ReportNotSpam(driver);
        }
        }

   


        private static async Task InboxToArchive(IWebDriver driver) 
        {
        try
        {
            while (IsNotEmpty(driver))
            {
                // Wait for the email list to load
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("a[data-test-id='message-list-item']")));

                // Locate the specific email using part of its subject or aria-label
                IWebElement Email = driver.FindElement(By.XPath("//a[@data-test-id='message-list-item']"));
                Email.Click();

                Thread.Sleep(2000);
                IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-archive']"));
                notSpamButton.Click();

                IWebElement inboxfolder = driver.FindElement(By.CssSelector("a[data-test-folder-name='Inbox']"));
                inboxfolder.Click();

            }
            driver.Close();
            driver.Quit();  // Properly close the browser
            driver.Dispose();  // Dispose of the WebDriver instance

        }
        catch (Exception)
        {
            await InboxToArchive(driver);
        }
            
        }

        private static async Task ClearSpam(IWebDriver driver)
        {
            while (IsNotEmpty(driver))
            {
                IWebElement checkboxButton = driver.FindElement(By.CssSelector("button[data-test-id='checkbox']"));
                checkboxButton.Click();
                Thread.Sleep(2000);
                IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-perm-delete']"));
                notSpamButton.Click();

            }
            driver.Close();
            driver.Quit();  // Properly close the browser
            driver.Dispose();  // Dispose of the WebDriver instance
        }
        private static async Task ClearInbox(IWebDriver driver)
        {
            while (IsNotEmpty(driver))
            {
                IWebElement checkboxButton = driver.FindElement(By.CssSelector("button[data-test-id='checkbox']"));
                checkboxButton.Click();
                Thread.Sleep(2000);
                IWebElement notSpamButton = driver.FindElement(By.CssSelector("button[data-test-id='toolbar-delete']"));
                notSpamButton.Click();

            }
            driver.Close();
            driver.Quit();  // Properly close the browser
            driver.Dispose();  // Dispose of the WebDriver instance
        }



        private static bool IsNotEmpty(IWebDriver driver)
        {
            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                // Check if the "Spam is empty" message exists using text
                IWebElement spamEmptyMessage = driver.FindElement(By.XPath("//span[contains(text(), 'is empty')]"));

                // If the element is found, it means the spam is empty, so return false to break the loop
                return spamEmptyMessage == null;
            }
            catch (NoSuchElementException)
            {
                // If the element is not found, it means spam is not empty, so continue the loop
                return true;
            }
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



        private static ChromeOptions optionProxy(int index)
        {
            ChromeOptions options = new ChromeOptions();
            options.AcceptInsecureCertificates = true;


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



        private static string filePath(string filename)
        {


            // Get the current directory
            string currentDirectory = Directory.GetCurrentDirectory();

            // file
            string filePath = Path.Combine(currentDirectory, filename);
        


            return filePath;

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
                driver.Quit();  // Properly close the browser
                driver.Dispose();  // Dispose of the WebDriver instance
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
                "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Safari/605.1.15            ",
                "Mozilla/5.0 (iPad; CPU OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15          ",
                "Mozilla/5.0 (iPad; CPU OS 15_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.2 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.2 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 14_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.7 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Safari/605.1.15            ",
                "Mozilla/5.0 (iPad; CPU OS 13_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1.1 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 16_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_2_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15        ",
                "Mozilla/5.0 (iPad; CPU OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.4 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.4 Safari/605.1.15          ",
                "Mozilla/5.0 (iPad; CPU OS 15_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0.1 Safari/605.1.15         ",
                "Mozilla/5.0 (iPad; CPU OS 13_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.4 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.1.2 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_2_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Safari/605.1.15          ",
                "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.1 Safari/605.1.15         ",
                "Mozilla/5.0 (iPad; CPU OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.6 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.1 Safari/605.1.15         ",
                "Mozilla/5.0 (iPad; CPU OS 16_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.2 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/10.0 Safari/602.1.50           ",
                "Mozilla/5.0 (iPad; CPU OS 15_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.4 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_2) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.0.3 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 14_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.5 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.2 Safari/605.1.15            ",
                "Mozilla/5.0 (iPad; CPU OS 12_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/9.1 Safari/601.5.17          ",
                "Mozilla/5.0 (iPad; CPU OS 16_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.4 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.1 Safari/605.1.15       ",
                "Mozilla/5.0 (iPad; CPU OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_0_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Safari/605.1.15          ",
                "Mozilla/5.0 (iPad; CPU OS 14_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.2 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/10.1 Safari/603.1.30         ",
                "Mozilla/5.0 (iPad; CPU OS 13_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.6 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/600.8.9 (KHTML, like Gecko) Version/8.0.8 Safari/600.8.9          ",
                "Mozilla/5.0 (iPad; CPU OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_6_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.1 Safari/605.1.15          ",
                "Mozilla/5.0 (iPad; CPU OS 13_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.7 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/11.0.1 Safari/605.1.15         ",
                "Mozilla/5.0 (iPad; CPU OS 15_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10) AppleWebKit/600.1.25 (KHTML, like Gecko) Version/8.0 Safari/600.1.25            ",
                "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_5) AppleWebKit/537.85.10 (KHTML, like Gecko) Version/7.0.6 Safari/537.85.10       " 
            };

            // Randomly select a user agent
            int index = random.Next(userAgents.Count);
            return userAgents[index];
        }


        static void DisplayLogo()
        {
            string logo = @"by
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


    