﻿using System;

namespace Baxendale.DataManagement.Xml
{
    public class XmlSerializeAttribute : Attribute
    {
        public string Name { get; set; }
        public object DefaultValue { get; set; }

        public XmlSerializeAttribute()
        {
        }
    }
}