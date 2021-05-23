﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TestApp.Properties;
using Baxendale.DataManagement.Xml;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            XDocument doc = XDocument.Parse(Resources.TextXml);

            XmlSerializer.RegisterType<TestClass>("testNode");
            TestClass test = XmlSerializer.Deserialize<TestClass>(doc.Root.Element("testNode"));

            Console.WriteLine(test);
            Console.ReadKey(true);
        }
    }
}