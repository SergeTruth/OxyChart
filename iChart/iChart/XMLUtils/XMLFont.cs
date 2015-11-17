using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

// From http://msdn.microsoft.com/en-us/library/system.drawing.font%28VS.85%29.aspx

namespace iChart
{
    public class XMLFont
    {
        private String m_fontName;
        public String FontName
        {
            get { return m_fontName; }
            set { m_fontName = value; }
        }

        private Single m_fontSize;
        public Single FontSize
        {
            get { return m_fontSize; }
            set { m_fontSize = value; }
        }

        private FontStyle m_fontStyle;
        public FontStyle FontStyle
        {
            get { return m_fontStyle; }
            set { m_fontStyle = value; }
        }

        public static XMLFont FromFont(Font font)
        {
            XMLFont xmlSerializableFont = new XMLFont();
            xmlSerializableFont.FontName = font.Name;
            xmlSerializableFont.FontSize = font.Size;
            xmlSerializableFont.FontStyle = font.Style;
            return xmlSerializableFont;
        }

        public Font ToFont()
        {
            return XMLFont.ToFont(this);
        }

        public static Font ToFont(XMLFont xmlSerializableFont)
        {
            Font font = new Font(xmlSerializableFont.FontName, xmlSerializableFont.FontSize, xmlSerializableFont.FontStyle);
            return font;
        }
    }
}
