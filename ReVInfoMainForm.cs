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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using OpenMcdf;

namespace ReVInfo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            string[] args = Environment.GetCommandLineArgs();
            string val = string.Empty;
            if (args.Length != 2) {
                // Show a message box to ask if user wants to select a file or folder
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Welcome to ReVInfo!");
                sb.AppendLine("Would you like to select a file or a folder?");
                sb.AppendLine("[Yes] to select a file");
                sb.AppendLine("[No] to select a folder, or [Cancel] to exit.");
                DialogResult result = MessageBox.Show(sb.ToString(), "Select file or folder",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    val = SelectFile();
                }
                else if (result == DialogResult.No)
                {
                    val = SelectFolder();
                }
                else
                {
                    Environment.Exit(0);
                }

                // Check if a value (file/folder) was selected
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine("No selection was made.");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine($"You selected: {val}");
                }
            } else {
                val = args[1];
            }

            FileAttributes attr = FileAttributes.Temporary;
            try
            {
                attr = File.GetAttributes(val);
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error getting file attributes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            if (attr.HasFlag(FileAttributes.Directory)) {
                InitializeComponent();
                this.Text = "RevitFileVersion <" + Path.GetFullPath(val) +">";
                ShowMultiFileInfo(GetMultiFileInfo(val));
            } else {
                ShowSingleFileInfo(GetFileInfo(val,Path.GetExtension(val)));
                Environment.Exit(0);
            }
        }
        private string SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Revit files (*.rvt, *.rfa)|*.rvt;*.rfa|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }
        private string SelectFolder()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }
            return string.Empty;
        }

        private void ShowSingleFileInfo(FileInfo fileInfo)
        {
            MessageBox.Show(fileInfo.Version, "Version info for" + fileInfo.Name);        
        }
        
        private void ShowMultiFileInfo(SortableBindingListCollection<FileInfo> fileInfoList)
        {
            this.dataGridView1.DataSource = fileInfoList;
            this.Show();
        }
        
        private SortableBindingListCollection<FileInfo> GetMultiFileInfo(string dirName)
        {
            
            SortableBindingListCollection<FileInfo> result = new SortableBindingListCollection<FileInfo>();
            string[] files = Directory.GetFiles(dirName, "*.rvt"); 
            foreach (string file in files) {
                result.Add(GetFileInfo(file, "Model"));
            }
            string[] families = Directory.GetFiles(dirName, "*.rfa"); 
            foreach (string file in families) {
                result.Add(GetFileInfo(file, "Family"));
            }
            return result;
        }
        
        private FileInfo GetFileInfo(string FileName, string type)
        {
            string versionString = string.Empty;
            CompoundFile file;
            try
            {
                file = new CompoundFile(FileName);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error processing:" + FileName + "Error:");
                System.Diagnostics.Debug.WriteLine(@"    " + ex.Message);
                return new FileInfo(GetFileName(FileName), "ERROR, Could not read file", "Unknown");
            }

            CFStream stream = null;
            try
            {
                stream = file.RootStorage.GetStream("BasicFileInfo");
            } catch(Exception ex)
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
            foreach (Match match in rgMatchLines.Matches(s)) {
                versionString += match.Value + Environment.NewLine;
            }

            s = Encoding.Unicode.GetString(stream.GetData());
            foreach (Match match in rgMatchLines.Matches(s))
            {
                versionString += match.Value + Environment.NewLine;
            }

            return new FileInfo(GetFileName(FileName), versionString, type);
        }

        private string GetFileName(string FileName)
        {
            return Path.GetFileName(FileName);  
        }
        
        private void DataGridView1ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //var selectedColumn = e.ColumnIndex;
            //dataGridView1.Sort(dataGridView1.Columns[selectedColumn],ListSortDirection.Ascending);
        }
    }
    
    public class FileInfo
    {
        public string Name
        {
            get; set;
        }
        public string Version
        {
            get; set;
        }
        public string Type
        {
            get; set;
        }
        
        public FileInfo(string name, string version, string type)
        {
            Name = name;
            Version = version;
            Type = type;
        }
    }
}
