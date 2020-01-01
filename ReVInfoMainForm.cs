﻿/*
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

using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReVInfo
{
    public partial class MainForm : Form
    {
        public MainForm(string title, List<FileInfo> contents)
        {
            InitializeComponent();
            this.Text = title;
            this.dataGridView1.DataSource = new SortableBindingListCollection<FileInfo>(contents);
            this.Show();
        }
    }
}
