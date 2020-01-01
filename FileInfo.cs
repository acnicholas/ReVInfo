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

namespace ReVInfo
{
    public class FileInfo
    {
        public string Name
        {
            get;  private set;
        }
        public string Version
        {
            get; private set;
        }
        public string Type
        {
            get; private set;
        }

        public FileInfo(string name, string version, string type)
        {
            Name = name;
            Version = version;
            Type = type;
        }
    }
}
