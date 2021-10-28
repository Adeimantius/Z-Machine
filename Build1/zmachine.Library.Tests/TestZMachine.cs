using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace zmachine.Library.Tests;

[TestClass]
public class TestZMachine
{
    public static string ZorkPath
    {
        get
        {
            string filePath = AppContext.BaseDirectory; //returns path "C:\..\bin\debug"
            int pos = filePath.IndexOf("zmachine.Library.Tests");
            if (pos == -1)
            {
                throw new Exception();
            }

            string pathSubstr = filePath.Substring(0, pos);

            return Path.Join(pathSubstr, "zmachine", "zmachine", "ZORK1.DAT");
        }
    }
}