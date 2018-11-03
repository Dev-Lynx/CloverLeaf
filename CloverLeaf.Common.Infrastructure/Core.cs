using CloverLeaf.Common.Infrastructure.Services;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CloverLeaf.Common.Infrastructure
{
    public static class Core
    {
        #region Properties

        public static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        static string[] Arguments { get; set; } = new string[0];
        static bool IsMainInstance { get; set; }
        static Mutex Instance { get; set; }
        static Thread ListenServer { get; set; }
        public static bool Initialized { get; private set; }
        public static bool Load { get; private set; }
        public static string LoadPath { get; private set; }

        public static event EventHandler DisplayRequested;
        public static event EventHandler<string> LoadRequested;
        


        #region Solids
        public const string PRODUCT_NAME = "CloverLeaf";
        public const string AUTHOR = "Prince Owen";

        #region Prism Constants

        #region Regions
        public const string MAIN_REGION = "Main Region";
        #endregion

        #region Views
        public const string START_VIEW = "Start View";
        public const string CONTEST_DIMENSIONS_VIEW = "Contest Dimensions View";
        public const string HOME_VIEW = "Home View";
        public const string RUN_VIEW = "Run View";
        public const string DIVISIONS_VIEW = "Divisions View";
        public const string ABOUT_VIEW = "About View";
        #endregion

        #endregion

        #region Misc

        #region Names
        public const string SERVER_NAME = "CloverLeaf Server";
        public const string ERROR_LOG_NAME = "Errors";
        public const string CONSOLE_LOG_NAME = "console-debugger";
        public const string RIDERS_DOC_NAME = "Riders";
        public const string HORSES_DOC_NAME = "Horses";
        public const string CONTEST_FILE_NAME = "CloverLeaf Contest File";
        public const string CONTEST_EXTENSION_FILE = ".clf";

        public enum StartupArguments { DISPLAY, LOAD }


        public const string DISPLAY_ARGUMENT = "--display";
        public const string LOAD_ARGUMENT = "--load";
        public readonly static string[] STARTUP_ARGUMENTS = new string[]
        {
           DISPLAY_ARGUMENT, LOAD_ARGUMENT
        };
        #endregion

        #region Directories
        public readonly static string SYSTEM_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public readonly static string USER_DOCUMENT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public readonly static string WORK_BASE = Path.Combine(SYSTEM_DATA_DIR, PRODUCT_NAME);
        public readonly static string LOG_DIR = Path.Combine(WORK_BASE, "Logs");
        public readonly static string DATA_DIR = Path.Combine(WORK_BASE, "Data");
        public readonly static string DOCUMENT_BASE = Path.Combine(USER_DOCUMENT_DIR, PRODUCT_NAME);
        #endregion

        #region Paths
        public readonly static string COMPANY_PAGE = @"https://www.fiverr.com/s2/eb22b7af9e";
        public readonly static string BASE_PATH = System.Reflection.Assembly.GetEntryAssembly().Location;
        public const string STARTUP_REGISTRY_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public readonly static string CONFIGURATION_FILE_PATH = Path.Combine(DATA_DIR, "Configuration.config");
        public const string LOG_LAYOUT = "${longdate}|${uppercase:${level}}| ${message}";
        public static readonly string ERROR_LOG_PATH = Path.Combine(LOG_DIR, ERROR_LOG_NAME + ".log");
        public static readonly string DATABASE_PATH = Path.Combine(DATA_DIR, "Data.db");
        #endregion

        #endregion

        #endregion

        #endregion

        #region Methods
        public static async Task Initialize(string[] arguments = null)
        {
            if (Initialized) return;
            Initialized = true;

            LoadRequested += (s, e) =>
            {
                Load = true; LoadPath = e;
            };

            AnalyzeInstance();

            if (arguments != null) Arguments = arguments;
            else Arguments = new string[0];

            var args = new string[Arguments.Length + 1];

            args[0] = Core.GetStartupArgument(Core.StartupArguments.DISPLAY);

            for (int i = 1; i < args.Length; i++)
                args[i] = Arguments[i - 1];

            if (IsMainInstance) ParseArguments();
            else { await SendMessage(args); Application.Current.Shutdown(); }

            ConfigureLogger();

#if DEBUG
            // Register and Initialize the Console Debugger
            Trace.Listeners.Add(new ConsoleTraceListener(true));
            Debug.Listeners.Add(new ConsoleTraceListener(true));
            ConsoleManager.Show();

            Log.Info("Welcome to the {0} Debugger", PRODUCT_NAME);
#endif
            CreateDirectories(WORK_BASE, DATA_DIR, LOG_DIR, DOCUMENT_BASE);
        }

        static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();

#if DEBUG
            var debugConsoleTarget = new ConsoleTarget()
            {
                Name = Core.CONSOLE_LOG_NAME,
                Layout = Core.LOG_LAYOUT,
                Header = string.Format("{0} Debugger", PRODUCT_NAME)
            };

            var debugRule = new LoggingRule("*", LogLevel.Debug, debugConsoleTarget);
            config.LoggingRules.Add(debugRule);
#endif

            var errorFileTarget = new FileTarget()
            {
                Name = Core.ERROR_LOG_NAME,
                FileName = Core.ERROR_LOG_PATH,
                Layout = Core.LOG_LAYOUT
            };

            config.AddTarget(errorFileTarget);

            var errorRule = new LoggingRule("*", LogLevel.Error, errorFileTarget);
            config.LoggingRules.Add(errorRule);

            LogManager.Configuration = config;

            LogManager.ReconfigExistingLoggers();
        }

        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Log.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Log.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }

        public static void ClearDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            foreach (var file in Directory.EnumerateFiles(directory, "*"))
                try { File.Delete(file); }
                catch (Exception e) { Log.Error("Error Deleting file {0}\n{1}", file, e); }
        }

        public static void ConfigureStartup(bool runAtStartup)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_REGISTRY_PATH, true);

                if (runAtStartup)
                    key.SetValue(PRODUCT_NAME, BASE_PATH);
                else key.DeleteValue(PRODUCT_NAME);
            }
            catch (Exception e)
            {
                Log.Error(e, "An error occured while accessing the registry for startup information");
            }
        }

        #region Arguments
        public static string GetStartupArgument(StartupArguments argument)
        {
            try { return STARTUP_ARGUMENTS[(int)argument]; }
            catch (Exception) { return ""; }
        }

        static void ParseArguments(string[] arguments = null)
        {
            if (arguments == null)
                arguments = Arguments;

            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                switch (arguments[i])
                {
                    case DISPLAY_ARGUMENT:
                        DisplayRequested?.Invoke(null, EventArgs.Empty);
                        break;

                    case LOAD_ARGUMENT:
                        if (!(i <= arguments.Length - 1 && File.Exists(arguments[++i])))
                            continue;

                        LoadRequested?.Invoke(null, arguments[i]);
                        break;

                    default:
                        {
                            var ext = Path.GetExtension(argument);

                            if (ext == CONTEST_EXTENSION_FILE && File.Exists(argument))
                                LoadRequested?.Invoke(null, argument);
                            break;
                        }
                }
            }
        }
        #endregion

        #region Singleton
        static async void AnalyzeInstance()
        {
            try
            {
                Instance = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out bool newInstance);
                IsMainInstance = newInstance;

                if (IsMainInstance)
                {
                    ListenServer = new Thread(Listen);
                    ListenServer.IsBackground = true;
                    ListenServer.Start();
                }
                else
                {
                    Instance.Dispose();

                    var args = new string[Arguments.Length + 1];
                    args[0] = Core.GetStartupArgument(Core.StartupArguments.DISPLAY);

                    for (int i = 0; i < args.Length; i++)
                        args[i] = Arguments[i - 1];

                    await SendMessage(args);
                    Application.Current.Shutdown();
                }
            }
            catch { }
        }

        static async void Listen()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(Core.SERVER_NAME))
                    using (var reader = new StreamReader(server))
                    {
                        await server.WaitForConnectionAsync();

                        var args = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            string buffer = string.Empty;
                            char c = '\0';
                            bool allowSpace = false;

                            while (!reader.EndOfStream)
                            {
                                c = (char)reader.Read();

                                if (allowSpace && c == '"') allowSpace = false;
                                else if (c == '"') allowSpace = true;

                                if (c == '"') continue;
                                if (c == ' ' && !allowSpace) break;

                                buffer += c;
                            }

                            args.Add(buffer);
                        }
                        Application.Current.Dispatcher.Invoke(() => Core.ParseArguments(args.ToArray()));
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "An unexpected error occured in the server");
                }
            }
        }

        static async Task<bool> SendMessage(params string[] messages)
        {
            if (messages == null) return false;

            try
            {
                using (var mutex = Mutex.OpenExisting(Assembly.GetExecutingAssembly().GetName().Name))
                using (var client = new NamedPipeClientStream(Core.SERVER_NAME))
                using (var writer = new StreamWriter(client))
                {
                    while (!client.IsConnected)
                        await client.ConnectAsync();

                    for (int i = 0; i < messages.Length; i++)
                    {
                        writer.Write("\"{0}\"", messages[i]);

                        if (i < messages.Length - 1) writer.Write(" ");
                    }
                }
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
                return false;
            }
            return true;
        }
        #endregion

        #endregion
    }
}
