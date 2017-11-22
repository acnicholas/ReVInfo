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
using System.ComponentModel;
using System.IO;
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
            string[] args = System.Environment.GetCommandLineArgs();
            string val = string.Empty;
            if (args.Length != 2) {
                if(Directory.Exists(@"C:\Revit 2017 Local")) {
                    val = @"C:\Revit 2017 Local";
                } else {
                    Environment.Exit(0);
                }
            } else {
                val = args[1];
            }
            
            FileAttributes attr = File.GetAttributes(val);
            if (attr.HasFlag(FileAttributes.Directory)) {
                InitializeComponent();
                this.Text = "RevitFileVersion <" + Path.GetDirectoryName(val) +">";
                ShowMultiFileInfo(GetMultiFileInfo(val));
            } else {
                ShowSingleFileInfo(GetFileInfo(val,Path.GetExtension(val)));
                Environment.Exit(0);
            }
        }
        
        private void ShowSingleFileInfo(FileInfo fileInfo)
        {
            System.Windows.Forms.MessageBox.Show(fileInfo.Version, "Version info for" + fileInfo.Name);        
        }
        
        private void ShowMultiFileInfo(SortableBindingListCollection<FileInfo> fileInfoList)
        {
            this.dataGridView1.DataSource = fileInfoList;
            this.Show();
        }
        
        private SortableBindingListCollection<FileInfo> GetMultiFileInfo(string dirName)
        {
            
            SortableBindingListCollection<FileInfo> result = new SortableBindingListCollection<FileInfo>();
            string[] files = System.IO.Directory.GetFiles(dirName, "*.rvt"); 
            foreach (string file in files) {
                result.Add(GetFileInfo(file, "Model"));
            }
            string[] families = System.IO.Directory.GetFiles(dirName, "*.rfa"); 
            foreach (string file in families) {
                result.Add(GetFileInfo(file, "Family"));
            }
            return result;
        }
        
        private FileInfo GetFileInfo(string FileName, string type)
        {
            string versionString = string.Empty;
            OpenMcdf.CompoundFile file = new OpenMcdf.CompoundFile(FileName);
            CFStream stream = file.RootStorage.GetStream("BasicFileInfo");
            string s = Encoding.BigEndianUnicode.GetString(stream.GetData());
            Regex rgMatchLines = new Regex (@"Revit Build:.*\d{4} ");
            foreach (Match match in rgMatchLines.Matches(s)) {
                versionString += match.Value + System.Environment.NewLine;
            }
            
            s = Encoding.Unicode.GetString(stream.GetData());
            rgMatchLines = new Regex (@"Revit Build:.*\d{4} ");
            foreach (Match match in rgMatchLines.Matches(s)) {
                versionString += match.Value + System.Environment.NewLine;
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
