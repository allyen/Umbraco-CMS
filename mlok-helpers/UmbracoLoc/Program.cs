using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace UmbracoLoc {
    class Program {
        static void Main(string[] args)
        {
            XmlDocument cs = new XmlDocument();
            cs.Load("cs.xml");
            //XmlDocument cs5 = new XmlDocument();
            //cs5.Load("cs5.xml");
            //cs5.PreserveWhitespace = false;
            XmlDocument en = new XmlDocument();
            en.Load("en.xml");
            //XmlDocument en5 = new XmlDocument();
            //en5.Load("en5.xml");

            XmlNodeList keysEn = en.SelectNodes("//key");
            int count = 0;
            foreach (XmlNode keyEn in keysEn)
            {
                XmlNode keyCs = cs.SelectSingleNode("//key[@alias = '" + keyEn.Attributes["alias"].InnerText + "']");
                //if (keyEn.InnerText.Contains("\n") || keyEn.InnerText.Contains("<") || keyEn.InnerText.Contains("'")) continue;
                if (keyCs == null)
                {
                    XmlNode csParent = keyEn.ParentNode.Name == "area" ?
                        cs.SelectSingleNode("//area[@alias = '" + keyEn.ParentNode.Attributes["alias"].Value + "']") :
                        cs.DocumentElement;
                    if (csParent == null)
                        throw new Exception("V cs verzi není area, kde alias = " + keyEn.ParentNode.Attributes["alias"].Value);

                    keyCs = cs.ImportNode(keyEn, true);
                    csParent.AppendChild(keyCs);
                    count++;
                }
                /*foreach (XmlNode keyEn5 in en5.SelectNodes("//Text[text() = '" + keyEn.InnerText + "']"))
                {
                    foreach (XmlNode keyCs5 in cs5.SelectNodes("//Text[@Key = '" + keyEn5.Attributes["Key"].Value + "']"))
                    {
                        keyCs5.InnerText = keyCs.InnerText;
                        count++;
                    }
                }*/
            }

            Console.WriteLine("missing: " + count);
            Console.ReadLine();
            cs.Save("cs_merged.xml");
        }
    }
}
