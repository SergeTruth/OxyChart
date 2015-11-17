using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using MindFusion.Drawing;
using MindFusion.Diagramming;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace iChart
{
    public class XMLDefaults
    {
        [XmlIgnore]
        public int _NodeDefaultWidth;
        [XmlIgnore]
        public int _NodeDefaultHeight;
        [XmlIgnore]
        public Color _NodeDefaultFillColor;
        [XmlIgnore]
        public Color _NodeDefaultFrameColor;
        [XmlIgnore]
        public int _NodeDefaultFrameThickness;
        [XmlIgnore]
        public Color _NodeTextColor;
        [XmlIgnore]
        public Font _TextFont;
        [XmlIgnore]
        public Color _ChartBackColor;

        public XMLDefaults()
        {           

        }

        /// <summary>
        /// Default note width.
        /// </summary>
        public int NodeDefaultWidth
        {
            get
            {
                return _NodeDefaultWidth;
            }
            set
            {
                _NodeDefaultWidth = value;
            }
        }

        /// <summary>
        /// Default note height.
        /// </summary>
        public int NodeDefaultHeight
        {
            get
            {
                return _NodeDefaultHeight;
            }
            set
            {
                _NodeDefaultHeight = value;
            }
        }

        /// <summary>
        /// Default fill color.
        /// </summary>
        public String NodeDefaultFillColor
        {
            get
            {
                return _NodeDefaultFillColor.ToArgb().ToString();
            }
            set
            {
                int _argb = int.Parse(value);
                Color newColor = System.Drawing.Color.FromArgb(_argb);
                _NodeDefaultFillColor = newColor;
            }
        }

        /// <summary>
        /// Default frame color.
        /// </summary>
        public String NodeDefaultFrameColor
        {
            get
            {
                return _NodeDefaultFrameColor.ToArgb().ToString();
            }
            set
            {
                int _argb = int.Parse(value);
                Color newColor = System.Drawing.Color.FromArgb(_argb);
                _NodeDefaultFrameColor = newColor;
            }
        }

        /// <summary>
        /// Default frame thickness.
        /// </summary>
        public int NodeDefaultFrameThickness
        {
            get
            {
                return _NodeDefaultFrameThickness;
            }
            set
            {
                _NodeDefaultFrameThickness = value;
            }
        }

        /// <summary>
        /// Default datagram background color.
        /// </summary>
        public String ChartBackColor
        {
            get
            {
                return _ChartBackColor.ToArgb().ToString();
            }
            set
            {
                int _argb = int.Parse(value);
                Color newColor = System.Drawing.Color.FromArgb(_argb);
                _ChartBackColor = newColor;
            }
        }

        public String NodeTextColor
        {
            get
            {
                return _NodeTextColor.ToArgb().ToString();
            }
            set
            {
                int _argb = int.Parse(value);
                Color newColor = System.Drawing.Color.FromArgb(_argb);
                _NodeTextColor = newColor;
            }
        }

        public XMLFont TextFont
        {
            get
            {
                return XMLFont.FromFont(_TextFont);
            }
            set
            {
                _TextFont = XMLFont.ToFont(value);
            }
        }
    }
}
