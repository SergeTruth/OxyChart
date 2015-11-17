using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace iChart
{
    public partial class MainUI : Form
    {
        public static int NodeWidth = 256;
        public static int NodeHeight = 48;

        public static Color NodeFillColor = Color.White;
        public static Color NodeFrameColor = Color.Black;
        public static int NodeFrameThickness = 1;

        static Color ChartBackColor = Color.White;
        static Color TextColor = Color.Black;
        static Font TextFont = new Font("Courier New", (float)10, FontStyle.Regular);

        void LoadDefaults()
        {
            String defaultsFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\defaults.xml";
            if (File.Exists(defaultsFile))
            {
                try
                {
                    XMLDefaults defaults = (XMLDefaults)LoadXML(defaultsFile, typeof(XMLDefaults));
                    ProxyDiagram pdiagram = new ProxyDiagram(diagramView1.Diagram);
                    pdiagram.NodeFillColor = defaults._NodeDefaultFillColor;
                    pdiagram.NodeFrameColor = defaults._NodeDefaultFrameColor;
                    pdiagram.NodeFrameThickness = defaults._NodeDefaultFrameThickness;
                    NodeWidth = defaults._NodeDefaultWidth;
                    NodeHeight = defaults._NodeDefaultHeight;
                    pdiagram.BackColor = defaults._ChartBackColor;
                    pdiagram.NodeTextColor = defaults._NodeTextColor;
                    pdiagram.TextFont = defaults._TextFont;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        void SaveDefaults()
        {
            try
            {
                String defaultsFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\defaults.xml";
                XMLDefaults defaults = new XMLDefaults();
                ProxyDiagram pdiagram = new ProxyDiagram(diagramView1.Diagram);
                defaults._NodeDefaultFillColor = pdiagram.NodeFillColor;
                defaults._NodeDefaultFrameColor = pdiagram.NodeFrameColor;
                defaults._NodeDefaultFrameThickness = pdiagram.NodeFrameThickness;
                defaults._NodeDefaultWidth = NodeWidth;
                defaults._NodeDefaultHeight = NodeHeight;
                defaults._ChartBackColor = pdiagram.BackColor;
                defaults._NodeTextColor = pdiagram.NodeTextColor;
                defaults._TextFont = pdiagram.TextFont;
                SaveXML(defaults, defaultsFile, typeof(XMLDefaults));
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}
