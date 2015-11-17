using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using MindFusion.Drawing;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;

namespace iChart
{
    class ProxyNode
    {
        ShapeNode node;
        /// <summary>
        /// The default constructor.
        /// </summary>
        public ProxyNode(ShapeNode note)
        {
            node = note;
        }
        

        #region Properties
        /// <summary>
        /// The font of the selected note.
        /// </summary>
        [Category("Node(s)")]
        public Font TextFont
        {
            get
            {
                return node.Font;
            }
            set
            {
                node.Font = value;
            }
        }

        /// <summary>
        /// The color of the selected note.
        /// </summary>
        [Category("Node(s)")]
        public Color FillColor
        {
            get
            {
                //return UtilNotes.GetNoteFillColor(node);
                MindFusion.Drawing.SolidBrush tempBrush =
                    (MindFusion.Drawing.SolidBrush)node.Brush;
                return tempBrush.Color;
            }
            set
            {
                //UtilNotes.SetNoteFillColor(node, value);
                node.Brush = new MindFusion.Drawing.SolidBrush(value);
            }
        }

        /// <summary>
        /// The frame color of the selected note.
        /// </summary>
        [Category("Node(s)")]
        public Color FrameColor
        {
            get
            {
                //return UtilNotes.GetNoteFrameColor(node);
                return node.Pen.Color;
            }
            set
            {
                //UtilNotes.SetNoteFrameColor(node, value);
                float penWidth = node.Pen.Width;
                MindFusion.Drawing.Pen newPen = new MindFusion.Drawing.Pen(value, penWidth);
                node.Pen = newPen;
            }
        }

        /// <summary>
        /// The font color of the selected note.
        /// </summary>
        [Category("Node(s)")]
        public Color TextColor
        {
            get
            {
                return node.TextColor;
            }
            set
            {
                node.TextColor = value;
            }
        }

        /// <summary>
        /// The width of the selected note.
        /// </summary>
        [Category("Node(s)")]
        [DisplayName("Width")]
        public int Width
        {
            get
            {
                //return UtilNotes.GetNoteWidth(node);
                return (int)node.Bounds.Width;
            }
            set
            {
                //UtilNotes.SetNoteWidth(node, (float)value);
                RectangleF size = node.Bounds;
                float x = size.X;
                float y = size.Y;
                float height = size.Height;
                node.Bounds = new RectangleF(x, y, (float)value, height);
            }
        }

        /// <summary>
        /// The height of a selected note.
        /// </summary>
        [Category("Node(s)")]
        public int Height
        {
            get
            {
                //return UtilNotes.GetNoteHeight(node);
                return (int)node.Bounds.Height;
            }
            set
            {
                //UtilNotes.SetNoteHeight(node, (float)value);
                RectangleF size = node.Bounds;
                float x = size.X;
                float y = size.Y;
                float width = size.Width;
                node.Bounds = new RectangleF(x, y, width, (float)value);
            }
        }


        /// <summary>
        /// The thickness of the frame of the selected note.
        /// </summary>
        [Category("Node(s)")]
        //[Browsable(false)]
        public int FrameThickness
        {
            get
            {
                //return UtilNotes.GetNoteFrameThickness(node);
                return (int)node.Pen.Width;
            }
            set
            {
                //UtilNotes.SetNoteFrameThickness(node, value);
                Color penColor = node.Pen.Color;
                MindFusion.Drawing.Pen newPen = new MindFusion.Drawing.Pen(penColor, value);
                node.Pen = newPen;
            }
        }

        [Category("Node(s)")]
        [Browsable(false)]
        public int X
        {
            get
            {
                return (int)node.Bounds.X;
            }
            set
            {
                RectangleF size = node.Bounds;
                float x = value;
                float y = size.Y;
                float width = size.Width;
                float height = size.Height;
                node.Bounds = new RectangleF(x, y, width, height);
            }
        }

        [Category("Node(s)")]
        [Browsable(false)]
        public int Y
        {
            get
            {
                return (int)node.Bounds.Y;
            }
            set
            {
                RectangleF size = node.Bounds;
                float x = size.X;
                float y = value;
                float width = size.Width;
                float height = size.Height;
                node.Bounds = new RectangleF(x, y, width, height);
            }
        }   

        [Category("Node(s)")]
        [Browsable(false)]
        public String Text
        {
            get
            {
                return node.Text;
            }
            set
            {
                node.Text = value;
            }
        }

        /// <summary>
        /// The Z-Index of the selected note.
        /// </summary>
        [Category("Node(s)")]
        public int ZIndex
        {
            get
            {
                return node.ZIndex; 
            }
            set
            {
                node.ZIndex = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Snap the node to the grid.
        /// </summary>
        public void SnapToGrid()
        {
            float X = node.Bounds.X;
            float Y = node.Bounds.Y;
            float grid = node.Parent.GridSizeX;
            float halfGrid = grid / 2;
            float newX = X - (X % grid) + halfGrid;
            float newY = Y - (Y % grid) + halfGrid;
            node.Move(newX, newY);
        }


        /// <summary>
        /// Is note1 higher than note2?
        /// </summary>
        public bool Higher(ShapeNode node2)
        {
            if (node.Bounds.Y < node2.Bounds.Y) return true;
            else return false;
        }


        /// <summary>
        /// Is note1 to the right of note2?
        /// </summary>
        public bool Right(ShapeNode node2)
        {
            if (node.Bounds.X > node2.Bounds.X) return true;
            else return false;
        }


        /// <summary>
        /// Increase the note size to fit the grid.
        /// </summary>
        public void IncreaseToFitGrid()
        {
            float Width = node.Bounds.Width;
            float Height = node.Bounds.Height;
            float grid = node.Parent.GridSizeX;
            float newWidth = Width + grid - (Width % grid);
            float newHeight = Height + grid - (Height % grid);
            node.Resize(newWidth, newHeight);
        }


        /// <summary>
        /// Decrease the note width by one grid.
        /// </summary>
        public void DecreaseWidthByGrid()
        {
            float Width = node.Bounds.Width;
            float Height = node.Bounds.Height;
            float grid = node.Parent.GridSizeX;
            float newWidth = Width - grid - (Width % grid);
            node.Resize(newWidth, Height);
        }


        /// <summary>
        /// Increase the note width by one grid.
        /// </summary>
        public void IncreaseWidthByGrid()
        {
            float Width = node.Bounds.Width;
            float Height = node.Bounds.Height;
            float grid = node.Parent.GridSizeX;
            float newWidth = Width + grid - (Width % grid);
            node.Resize(newWidth, Height);
        }
        #endregion
    }
}
