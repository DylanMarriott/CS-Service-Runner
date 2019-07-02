using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Configuration.Install;
using System.Timers;
using System.Configuration;
using IniParser;
using IniParser.Model;
using System.Windows.Forms;
namespace CS_Service_Runner
{
    public partial class Service1 : ServiceBase
      {

      
        public Service1()
        {
            InitializeComponent();

        }
        private System.Timers.Timer restartTimer;

        string logPath = "C:/Service/" + DateTime.Today.ToString("ddMMyyyy") + ".txt";
        Process proc = new Process();
        int restartCount = 0;
        FileIniDataParser fileIniData = new FileIniDataParser();
       
        public void StartProgram()
        {
            File.AppendAllText("C:/Temp/log.log", "Starting " + Path.GetDirectoryName(Application.ExecutablePath));
            fileIniData.Parser.Configuration.CommentString = "#";
            IniData parsedData = fileIniData.ReadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"/Settings.ini");
            string exeName = parsedData["GeneralConfiguration"]["exeName"];
            foreach (var process in Process.GetProcessesByName(exeName)) //Close executable if already running
            {
                process.WaitForInputIdle();
                process.CloseMainWindow();
            }
            restartTimer = new System.Timers.Timer(10000); // make restart timer new timer, low interval so it runs event once
            restartTimer.Elapsed += new ElapsedEventHandler(StartRestart); // set restart timer to run this event

            restartTimer.Enabled = true; // enable timer
            if (!File.Exists(parsedData["GeneralConfiguration"]["exePath"]))
            {
                File.AppendAllText("C:/temp/Service Error.txt", "Issue with exe path: " + parsedData["GeneralConfiguration"]["exePath"]);
                // make it do something here soon
                Environment.Exit(0);
            }
            if (!Directory.Exists(parsedData["GeneralConfiguration"]["logPath"]))
            {
                try
                {
                    Directory.CreateDirectory(parsedData["GeneralConfiguration"]["logPath"]);
                }
                catch
                {
                    File.AppendAllText("C:/temp/Service Error.txt", "Issue with log path: " + parsedData["GeneralConfiguration"]["logPath"]);
                    Environment.Exit(0);
                }
            }
           File.AppendAllText("C:/temp/CS.txt", "Enable Timer" + Environment.NewLine);

            // Create a timer with a ten second interval.

        }
        protected override void OnStart(string[] args)
        {
            Thread thread = new Thread(StartProgram);
            thread.Start();
        }

        protected override void OnStop()
        {
            fileIniData.Parser.Configuration.CommentString = "#";
            IniData parsedData = fileIniData.ReadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"/Settings.ini");
            string exeName = parsedData["GeneralConfiguration"]["exeName"];
            foreach (var process in Process.GetProcessesByName(exeName))
            {

                process.WaitForInputIdle();
                process.CloseMainWindow();
            }

            Environment.Exit(0);
        }

        public void StartRestart(object source, ElapsedEventArgs e)
        { 
            fileIniData.Parser.Configuration.CommentString = "#";
            IniData parsedData = fileIniData.ReadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"/Settings.ini");
            logPath = parsedData["GeneralConfiguration"]["logPath"] + DateTime.Today.ToString("ddMMyyyy") + ".txt";
            int restartTime = Int32.Parse(parsedData["GeneralConfiguration"]["restartTimer"]) * 60000; // turn value into minutes and save 
            restartTimer.Interval = restartTime;
            if (restartCount == 0)
            {
                File.AppendAllText(logPath, System.DateTime.Now + ": Restart Timer = " + parsedData["GeneralConfiguration"]["restartTimer"] + Environment.NewLine);
                File.AppendAllText(logPath, System.DateTime.Now + ": Log Path = " + logPath + Environment.NewLine);
                File.AppendAllText(logPath, System.DateTime.Now + ": Starting Exe on " + parsedData["GeneralConfiguration"]["exePath"] + Environment.NewLine);
            }
            else
            {
                restartCount++;
               // File.AppendAllText("C:/temp/cs.txt", "Fell into Else Statement" + Environment.NewLine);
                File.AppendAllText(logPath, System.DateTime.Now + ": Restart Number " + restartCount + Environment.NewLine);
                proc.WaitForInputIdle();
                proc.CloseMainWindow();
            }
            restartCount++;
            proc.StartInfo.FileName = (parsedData["GeneralConfiguration"]["exePath"]);
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            if (proc.HasExited == false)
            {
              File.AppendAllText(logPath, System.DateTime.Now + ": Started Sucessfully" + Environment.NewLine);
            } else
            {
                File.AppendAllText(logPath, System.DateTime.Now + ": Executable Closed it's self on path: " + parsedData["GeneralConfiguration"]["exePath"] + Environment.NewLine);
            }
            
        }
    }
}
