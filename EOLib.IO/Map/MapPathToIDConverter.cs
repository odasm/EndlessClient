﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;

namespace EOLib.IO.Map
{
    public class MapPathToIDConverter
    {
        public int ConvertFromPathToID(string pathToMapFile)
        {
            var lastSlash = pathToMapFile.LastIndexOf('\\') < 0 ? pathToMapFile.LastIndexOf('/') : -1;
            if (lastSlash < 0)
                throw new IOException();

            var strID = pathToMapFile.Substring(lastSlash + 1, 5);
            return int.Parse(strID);
        }
    }
}
