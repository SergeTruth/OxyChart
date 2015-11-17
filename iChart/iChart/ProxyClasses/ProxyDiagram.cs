using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using MindFusion.Drawing;
using MindFusion.Diagramming;
using System.Windows.Forms;

namespace iChart
{
    class ProxyDiagram
    {
        Diagram diagram;

        public ProxyDiagram(Diagram _diagram)
        {
            diagram = _diagram;
        }

        /// <summary>
        /// The default font.
        /// </summary>
        [Category("Diagram")]
        public Font TextFont
        {
            get
            {
                return diagram.Font;
            }
            set
            {
                diagram.Font = value;
            }
        }

        /// <summary>
        /// The background color of the datagram.
        /// </summary>
        [Category("Diagram")]
        public Color BackColor
        {
            get
            {
                MindFusion.Drawing.SolidBrush brush =
                    (MindFusion.Drawing.SolidBrush)diagram.BackBrush;
                return brush.Color;
            }
            set
            {
                MindFusion.Drawing.SolidBrush brush =
                    new MindFusion.Drawing.SolidBrush(value);
                diagram.BackBrush = brush;
            }
        }

        /// <summary>
        /// The width of the datagram.
        /// </summary>
        [Category("Diagram")]
        public int Width
        {
            get
            {
                return (int)diagram.Bounds.Width;
            }
            set
            {
                float x = diagram.Bounds.X;
                float y = diagram.Bounds.Y;
                RectangleF newSize = new RectangleF(x, y, (float)value, (float)Height);

                diagram.Bounds = newSize;
            }
        }

        /// <summary>
        /// The height of the datagram.
        /// </summary>
        [Category("Diagram")]
        public int Height
        {
            get
            {
                return (int)diagram.Bounds.Height;
            }
            set
            {
                float x = diagram.Bounds.X;
                float y = diagram.Bounds.Y;
                RectangleF newSize = new RectangleF(x, y, (float)Width, (float)value);
                diagram.Bounds = newSize;
            }
        }

        /// <summary>
        /// The default fill color of the new notes.
        /// </summary>
        [Category("Diagram")]
        public Color NodeFillColor
        {
            get
            {
                return MainUI.NodeFillColor;
            }
            set
            {
                MainUI.NodeFillColor = value;
            }
        }


        /// <summary>
        /// The default frame color of the new notes.
        /// </summary>
        [Category("Diagram")]
        public Color NodeFrameColor
        {
            get
            {
                return MainUI.NodeFrameColor;
            }
            set
            {
                MainUI.NodeFrameColor = value;
            }
        }

        /// <summary>
        /// The default text color.
        /// </summary>
        [Category("Diagram")]
        public Color NodeTextColor
        {
            get
            {
                return diagram.TextColor;
            }
            set
            {
                diagram.TextColor = value;
            }
        }

        int _NodeFrameThickness;
        public int NodeFrameThickness
        {
            get
            {
                return MainUI.NodeFrameThickness;
            }
            set
            {
                MainUI.NodeFrameThickness = value;
            }
        }

        public int DefaultNodeWidth
        {
            get
            {
                return MainUI.NodeWidth;
            }
            set
            {
                MainUI.NodeWidth = value;
            }
        }

        public int DefaultNodeHeight
        {
            get
            {
                return MainUI.NodeHeight;
            }
            set
            {
                MainUI.NodeHeight = value;
            }
        }
    }
}
