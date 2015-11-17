using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using MindFusion.Diagramming;
using MindFusion.Drawing;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace iChart
{
    [Serializable]
    public class XMLChart
    {
        private Diagram chart;
        private ProxyDiagram pchart;

        float _DocumentWidth;
        public float DocumentWidth
        {
            get
            {
                return _DocumentWidth;
            }
            set
            {
                _DocumentWidth = value;
            }
        }

        float _DocumentHeight;
        public float DocumentHeight
        {
            get
            {
                return _DocumentHeight;
            }
            set
            {
                _DocumentHeight = value;
            }
        }

        String _BackgroundColor;
        public String BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                _BackgroundColor = value;
            }
        }

        String _TextColor;
        public String TextColor
        {
            get
            {
                return _TextColor;
            }
            set
            {
                _TextColor = value;
            }
        }


        XMLFont _Font;
        public XMLFont Font
        {
            get
            {
                return _Font;
            }
            set
            {
                _Font = value;
            }
        }

        int _NodeWidth;
        public int NodeWidth
        {
            get
            {
                return _NodeWidth;
            }
            set
            {
                _NodeWidth = value;
            }
        }

        int _NodeHeight;
        public int NodeHeight
        {
            get
            {
                return _NodeHeight;
            }
            set
            {
                _NodeHeight = value;
            }
        }

        int _NodeFrameThickness;
        public int NodeFrameThickness
        {
            get
            {
                return _NodeFrameThickness;
            }
            set
            {
                _NodeFrameThickness = value;
            }
        }

        String _NodeFillColor;
        public String NodeFillColor
        {
            get
            {
                return _NodeFillColor;
            }
            set
            {
                _NodeFillColor = value;
            }
        }

        String _NodeFrameColor;
        public String NodeFrameColor
        {
            get
            {
                return _NodeFrameColor;
            }
            set
            {
                _NodeFrameColor = value;
            }
        }

        ArrayList _Nodes = new ArrayList();
        [XmlArrayItem("XMLNode",typeof(XMLNode))]
        //[XmlElement(Type = typeof(XMLNode))]
        public ArrayList Nodes
        {
            get
            {
                return _Nodes;
            }
            set
            {
                _Nodes = value;
            }
        }
        

        public XMLChart()
        {
            //Nodes = new ArrayList();
        }

        public XMLChart(Diagram _chart)
        {
            //Nodes = new ArrayList();

            chart = _chart;
            pchart = new ProxyDiagram(chart);

            DocumentWidth = pchart.Width;
            DocumentHeight = pchart.Height;
            BackgroundColor = System.Drawing.ColorTranslator.ToHtml(pchart.BackColor);
            TextColor = System.Drawing.ColorTranslator.ToHtml(pchart.NodeTextColor);
            Font = XMLFont.FromFont(pchart.TextFont);

            NodeWidth = MainUI.NodeWidth;
            NodeHeight = MainUI.NodeHeight;

            NodeFrameThickness = pchart.NodeFrameThickness;
            NodeFillColor = System.Drawing.ColorTranslator.ToHtml(pchart.NodeFillColor);
            NodeFrameColor = System.Drawing.ColorTranslator.ToHtml(pchart.NodeFrameColor);
        }

        public void SaveChart(String fileName)
        {

            foreach (DiagramNode box in chart.Nodes)
            {
                if (box is ShapeNode)
                {
                    XMLNode node = new XMLNode((ShapeNode)box);
                    Nodes.Add(node);
                }
            }

            // First serialize the document to an in-memory stream
            MemoryStream memXmlStream = new MemoryStream();

            XmlSerializer serializer =
                new XmlSerializer(typeof(XMLChart));

            try
            {
                serializer.Serialize(memXmlStream, this);
            }
            catch (Exception serError)
            {
                MessageBox.Show("ERROR SERIALIZING");
            }

            // Now create a document with
            // the stylesheet processing instruction
            XmlDocument xmlDoc = new XmlDocument();

            // Now load the in-memory stream
            // XML data into the XMl document
            memXmlStream.Seek(0, SeekOrigin.Begin);
            xmlDoc.Load(memXmlStream);

            // Now add the stylesheet processing
            // instruction to the XML document
            XmlProcessingInstruction newPI;
            String PItext = "type='text/xsl' href='ichart.xsl'";
            newPI = xmlDoc.CreateProcessingInstruction("xml-stylesheet", PItext);

            xmlDoc.InsertAfter(newPI, xmlDoc.FirstChild);

            // Now write the document
            // out to the final output stream
            XmlTextWriter xmlWriter = new XmlTextWriter(fileName,
                                      System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = 2;
            xmlDoc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            xmlWriter.Close();
            //UtilIO.SaveXML(this, fileName, typeof(XmlChart));
        }

        public void LoadChart(Diagram _chart)
        {
            chart = _chart;
            pchart = new ProxyDiagram(_chart);
            chart.AutoResize = AutoResize.None;
            pchart.BackColor = System.Drawing.ColorTranslator.FromHtml(BackgroundColor);
            chart.TextColor = System.Drawing.ColorTranslator.FromHtml(TextColor);
            chart.Font = Font.ToFont();
            pchart.NodeFillColor = System.Drawing.ColorTranslator.FromHtml(NodeFillColor);
            pchart.NodeFrameColor = System.Drawing.ColorTranslator.FromHtml(NodeFrameColor);
            pchart.NodeFrameThickness = NodeFrameThickness;
            foreach (XMLNode node in Nodes)
            {
                ShapeNode box = MakeNode(node.X, node.Y, node.Width, node.Height);
                ProxyNode pnode = new ProxyNode(box);
                box.ZIndex = node.ZIndex;
                box.Text = node.Text;
                box.HyperLink = node.HyperLink;
                //box.Tag = node.Tag;
                box.ToolTip = node.Tooltip;
                box.Font = node.NodeFont.ToFont();
                box.TextColor = System.Drawing.ColorTranslator.FromHtml(node.TextColor);
                pnode.FillColor = System.Drawing.ColorTranslator.FromHtml(node.FillColor);
                pnode.FrameColor = System.Drawing.ColorTranslator.FromHtml(node.FrameColor);
                pnode.FrameThickness = node.FrameThickness;
                pnode.ZIndex = node.ZIndex;
            }
            pchart.Width = (int)DocumentWidth;
            pchart.Height = (int)DocumentHeight;
            MainUI.NodeWidth = NodeWidth;
            MainUI.NodeHeight = NodeHeight;
        }

        public ShapeNode MakeNode(float x, float y, float width, float height)
        {
            ShapeNode note = chart.Factory.CreateShapeNode(x, y,
                width,
                height);
            note.Tag = "";
            note.HyperLink = "";
            note.Selected = true;
            //adjust the box properties
            ProxyNode node = new ProxyNode(note);
            node.FillColor = pchart.NodeFillColor;
            node.FrameColor = pchart.NodeFrameColor;
            node.FrameThickness = pchart.NodeFrameThickness;
            node.SnapToGrid();
            return note;
        }
    }
}
