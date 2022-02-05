using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LolAutoLogin
{
    class LoLLogin
    {
        static String LolPath = "";
        static String LolExeName = "LeagueClient.exe";

        static String LoginScriptFileName = "LolAutoLoginScript.vbs";
        static String LoginScript = @"
                        dim script, accountId, accountPw, lolPath
                        set script = wscript.CreateObject(""WScript.Shell"")
                        accountId = WScript.Arguments(0)
                        accountPW = WScript.Arguments(1)
                        lolPath = WScript.Arguments(2)
                        Eval(Login)
                        WScript.Quit
                        Function Login()
                            set oExec = script.Exec(lolPath)
                            Do
                                WScript.Sleep 10
                            Loop Until oExec.Status = 1
                            script.AppActivate ""Riot Client""
                            WScript.Sleep 500
                            script.sendkeys accountId
                            script.sendkeys ""{TAB}""
                            script.sendkeys accountPw
                            script.sendkeys ""{ENTER}""
                        End Function
                        ";

        public static void setLolPath(String lolPath)
        {
            LoLLogin.LolPath = lolPath;
        }

        public static String getLolPath()
        {
            return LoLLogin.LolPath;
        }

        public static String getLolExeName()
        {
            return LoLLogin.LolExeName;
        }

        public static String getLolExeFullPath()
        {
            return LolPath + "\\" + LolExeName;
        }

        public static void Login(String username, String password)
        {
            KillRunningLolProcesses();
            loadAndRunLoginScript(username, password);
        }

        private static void KillRunningLolProcesses()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Equals("LeagueClient") ||
                   process.ProcessName.Equals("RiotClientServices"))
                {
                    process.Kill();
                    Thread.Sleep(500);
                }
            }
        }

        private static void loadAndRunLoginScript(String username, String password)
        {
            File.WriteAllText(Path.GetTempPath() + "\\" + LoginScriptFileName, LoginScript);

            String s = Path.GetTempPath() + "\\" + LoginScriptFileName;

            Process scriptProc = new Process();
            scriptProc.StartInfo.FileName = @"cscript";
            scriptProc.StartInfo.WorkingDirectory = Path.GetTempPath();
            scriptProc.StartInfo.Arguments = "//B //Nologo " + LoginScriptFileName + " " + username + " " + password + " \"" + getLolExeFullPath() + "\"";
            scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            scriptProc.Start();
            scriptProc.WaitForExit();
            scriptProc.Close();
        }
    }
}
