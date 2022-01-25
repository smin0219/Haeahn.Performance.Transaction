using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    // The example creates a file named "log.txt" and writes the following lines to it,
    // or appends them to the existing "log.txt" file:

    // Log Entry : <current long time string> <current long date string>
    //  :
    //  :Test1
    // -------------------------------

    // Log Entry : <current long time string> <current long date string>
    //  :
    //  :Test2
    // -------------------------------

    // It then writes the contents of "log.txt" to the console.
    class Log
    {
        internal static void WriteToFile(string message, string fileDir = "", string fileName = "")
        {
            try
            {
                if (fileDir == "")
                {
                    //string projectDirectory = Directory.GetCurrentDirectory();
                    fileDir = "C:\\Users\\SungjaeMin\\Desktop\\";
                }

                string logFolderDirectory = fileDir + "\\Logs\\";
                string logFileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                string logFileDirectory = logFolderDirectory + logFileName;

                //프로젝트 파일이 저장되는 폴더내에 Logs 폴더가 없으면 생성
                if (!Directory.Exists(logFolderDirectory))
                {
                    DirectoryInfo di = Directory.CreateDirectory(logFolderDirectory);
                }

                //Logs 폴더 안에 오늘 날짜의 파일로 로그파일 저장.
                using (StreamWriter writer = File.AppendText(logFileDirectory))
                {
                    WriteLogMessage(message, writer);
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
        }
        private static void WriteLogMessage(string Message, TextWriter writer)
        {
            writer.Write("Log Entry : ");
            writer.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            writer.WriteLine($"  {Message}");
            writer.WriteLine("-------------------------------------------------------");
        }
    }
}
