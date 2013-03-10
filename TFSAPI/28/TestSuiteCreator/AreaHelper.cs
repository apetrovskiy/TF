// Written by Shai Raiten: http://blogs.microsoft.co.il/blogs/shair/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSuiteCreator
{
    public static class AreaHelper
    {
        //Remove bad char from Area Path
        public static string RemoveBadChars(this string path)
        {
            return path.Replace('/', '-').Replace("&", " and ").
                Replace("#", "_").Replace("$", "_").Replace("%", "_").
                Replace("*", "_").Replace("\"", "'").Replace("\n", " ").Replace("\t", " ");
        }
    }
}
