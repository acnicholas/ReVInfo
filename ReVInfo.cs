/*
This file is part of ReVInfo.

ReVInfo is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReVInfo is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReVInfo.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OpenMcdf;
using System.Collections.Generic;

namespace ReVInfo
{
    internal sealed class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            //if any arg is found, *presume* that the first arg is a file or directory.
            var path = args.Length == 1 ? args[0] : string.Empty;
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    var fileType = Path.GetExtension(path) == ".rvt" ? "Model" : "Family";
                    ShowSingleFileInfo(GetFileInfo(path, fileType));
                    Environment.Exit(0);
                }
                if (Directory.Exists(path))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm(Path.GetFullPath(path), GetMultipleFileInfo(path)));
                    Environment.Exit(0);
                }
            }
            Environment.Exit(1);
        }

        private static void ShowSingleFileInfo(FileInfo fileInfo)
        {
            MessageBox.Show(fileInfo.Version, "Version info for: " + fileInfo.Name);
        }

        private static List<FileInfo> GetMultipleFileInfo(string dirName)
        {

            List<FileInfo> result = new List<FileInfo>();
            string[] files = Directory.GetFiles(dirName, "*.rvt");
            foreach (string file in files)
            {
                result.Add(GetFileInfo(file, "Model"));
            }
            string[] families = Directory.GetFiles(dirName, "*.rfa");
            foreach (string file in families)
            {
                result.Add(GetFileInfo(file, "Family"));
            }
            return result;
        }

        private static FileInfo GetFileInfo(string FileName, string type)
        {
            string versionString = string.Empty;
            CompoundFile file;
            try
            {
                file = new CompoundFile(FileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error processing:" + FileName + "Error:");
                System.Diagnostics.Debug.WriteLine(@"    " + ex.Message);
                return new FileInfo(GetFileName(FileName), "ERROR, Could not read file", "Unknown");
            }

            CFStream stream = null;
            try
            {
                stream = file.RootStorage.GetStream("BasicFileInfo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error getting BasicFileInfo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Could not read BasicFileInfo, report an error...
            if (stream == null)
            {
                return new FileInfo(GetFileName(FileName), "ERROR, Could not read file", "Unknown");
            }

            string s = Encoding.BigEndianUnicode.GetString(stream.GetData());
            Regex rgMatchLines = new Regex(@"Revit Build:.*\d{4} |F o r m a t:  \d \d \d \d |Format:.*\d{4}");
            foreach (Match match in rgMatchLines.Matches(s))
            {
                versionString += match.Value + Environment.NewLine;
            }

            s = Encoding.Unicode.GetString(stream.GetData());
            foreach (Match match in rgMatchLines.Matches(s))
            {
                versionString += match.Value + Environment.NewLine;
            }

            return new FileInfo(GetFileName(FileName), versionString, type);
        }

        private static string GetFileName(string FileName)
        {
            return Path.GetFileName(FileName);
        }
    }
}
