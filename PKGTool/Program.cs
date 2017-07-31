using HedgeLib.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PKGTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string fileName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            if (args.Length == 0)
            {
                Print($"{fileName} {{File Path / Directory Path}}");
                return;
            }
            else if (new FileInfo(args[0]).Exists)
            {
                ExtractPKG(args[0]);
            }
            else if (new DirectoryInfo(args[0]).Exists)
            {
                RepackPKG(args[0]);
            }
            else
            {
                PrintError("File or Directory does not exist!");
            }
        }

        public static void ExtractPKG(string filePath)
        {
            // Directory
            string dirPath = Path.GetFileNameWithoutExtension(filePath);
            dirPath = Path.Combine(Path.GetDirectoryName(filePath), dirPath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var archive = new PKGArchive();
            
            // Loads the archive
            Print("Loading...");
            archive.Load(filePath);
            
            // Extract the archive
            Print("Extracting...");
            // Extracts all the files with percent output
            for (int i = 0; i < archive.Data.Count; ++i)
            {
                var data = archive.Data[i];
                data.Extract(Path.Combine(dirPath, data.Name));
                int percent = (int)(((float)i / archive.Data.Count) * 100f);
                Print($"{percent}%\r", false);
            }

            Print("Done!");
        }

        public static void RepackPKG(string dirPath)
        {
            // Archive
            var archive = new PKGArchive();
            // Adds Files
            Print("Adding Files...");
            archive.AddDirectory(dirPath, false);
            // Saving Archive
            Print("Saving...");
            archive.Save(dirPath + ".pkg");

            Print("Done!");
        }


        // Other Functions
        public static void Print(string text, bool newLine = true)
        {
            if (newLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }

        public static void PrintWarning(string text)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] {0}\n", text);
            Console.ForegroundColor = color;
        }

        public static void PrintError(string text)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] {0}\n", text);
            Console.ForegroundColor = color;
        }

        public static void PrintDebug(string text)
        {
            #if DEBUG
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[DEBUG] {0}", text);
            Console.ForegroundColor = color;
            #endif
        }

        /// <summary>
        /// Asks a Yes/No question to the user
        /// </summary>
        /// <param name="msg">The Question</param>
        /// <returns>The Answer</returns>
        public static bool YesNo(string msg)
        {
            Console.Write($"{msg} <Y/N>: ");

            // Pause until the user enters either Y or N
            var keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Y && keyInfo.Key != ConsoleKey.N)
                keyInfo = Console.ReadKey(true);

            Console.WriteLine(keyInfo.Key == ConsoleKey.Y ? "Yes" : "No");
            return (keyInfo.Key == ConsoleKey.Y);
        }
    }
}
