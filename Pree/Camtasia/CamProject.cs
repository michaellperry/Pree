using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace Pree.Camtasia
{
    public class CamProject
    {
        private XmlDocument _document;

        private CamProject(XmlDocument document)
        {
            _document = document;
        }

        public static CamProject Load(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            return new CamProject(document);
        }

        public Source GetTimeline()
        {
            var sources = _document.SelectNodes("/Project_Data/CSMLData/GoProject/Project/SourceBin/Source")
                .OfType<XmlNode>()
                .Select(n => new Source(int.Parse(GetAttribute(n, "id")), GetAttribute(n, "src")));

            return sources.Where(s => s.Src.EndsWith("_time.wav")).FirstOrDefault();
        }

        public void Write(string filename)
        {
            using (var writer = XmlWriter.Create(filename))
            {
                _document.WriteTo(writer);
            }
        }

        private static string GetAttribute(XmlNode n, string src)
        {
            return n.Attributes.OfType<XmlAttribute>().Where(a => a.Name == src).Single().Value;
        }
    }
}
