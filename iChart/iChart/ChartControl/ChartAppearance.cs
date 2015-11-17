using System;
using System.Collections.Generic;
using System.Text;
using MindFusion.Diagramming;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace iChart
{
    public partial class MainUI : Form
    {


        public static bool editing = false;
        public Control editControl;
        int dX = 0;
        int dY = 0;


        public void ApplyHardcodedDefaults()
        {
            diagramView1.Diagram.AllowSelfLoops = false;
            diagramView1.Diagram.DynamicLinks = true;
            diagramView1.Diagram.EnableStyledText = false;
            diagramView1.Diagram.MeasureUnit = GraphicsUnit.Pixel;
            diagramView1.Diagram.AutoResize = AutoResize.RightAndDown;
            diagramView1.Diagram.TextFormat.Trimming = StringTrimming.Word;
            diagramView1.Diagram.UndoManager.UndoEnabled = true;
            diagramView1.Diagram.ShapeBrush = new MindFusion.Drawing.SolidBrush(NodeFillColor);
            diagramView1.Diagram.ShapePen = new MindFusion.Drawing.Pen(NodeFrameColor, (float)1);
            diagramView1.Diagram.DefaultShape = Shapes.Rectangle;
            diagramView1.Diagram.BackBrush = new MindFusion.Drawing.SolidBrush(BackColor);
            diagramView1.Diagram.TextColor = TextColor;
            diagramView1.Diagram.GridColor = Color.LightBlue;
            diagramView1.Diagram.GridStyle = GridStyle.Lines;
            diagramView1.Diagram.GridSizeX = 16;
            diagramView1.Diagram.GridSizeY = 16;
            diagramView1.Diagram.ShadowsStyle = ShadowsStyle.None;
            diagramView1.Diagram.ShowAnchors = ShowAnchors.Selected;
            diagramView1.Diagram.AdjustmentHandlesSize = 5;
            diagramView1.Diagram.Font = TextFont;
            diagramView1.Diagram.TextFormat.Alignment = StringAlignment.Near;
            diagramView1.Diagram.TextFormat.LineAlignment = StringAlignment.Near;
            //ShapeHandlesStyle = HandlesStyle.HatchHandles2;
            diagramView1.Diagram.ShapeHandlesStyle = HandlesStyle.DashFrame;
            //LinkCrossings = LinkCrossings.Arcs;
            //CrossingRadius = 5;
            diagramView1.Diagram.BoundsPen = new MindFusion.Drawing.Pen("1/#FF000000/2/0/0//0/0/10/");
            diagramView1.ContextMenuStrip = menuChart;

            System.Drawing.RectangleF newSize = new System.Drawing.RectangleF(0, 0, 800, 600);
            diagramView1.Diagram.Bounds = newSize;


            diagramView1.Behavior = Behavior.Modify;
            diagramView1.AllowInplaceEdit = true;
            diagramView1.AllowDrop = false;
            diagramView1.ControlMouseAction = ControlMouseAction.SelectNode;
            diagramView1.DelKeyAction = DelKeyAction.DeleteActiveItem;
            diagramView1.MiddleButtonActions = MouseButtonActions.Pan;
            diagramView1.ModificationStart = ModificationStart.SelectedOnly;
            diagramView1.RightButtonActions = MouseButtonActions.Cancel;
            diagramView1.InplaceEditFont = TextFont;
            diagramView1.ControlHandlesStyle = HandlesStyle.HatchHandles;

            diagramView1.MouseWheel += new System.Windows.Forms.MouseEventHandler(MouseWheelHandler);

            diagramView1.EnterInplaceEditMode +=
                new MindFusion.Diagramming.WinForms.InPlaceEditEventHandler(EnterInplaceEditMode);
            diagramView1.LeaveInplaceEditMode +=
                new MindFusion.Diagramming.WinForms.InPlaceEditEventHandler(LeaveInplaceEditMode);
            diagramView1.KeyDown +=
                new System.Windows.Forms.KeyEventHandler(KeyDownHandler);

            diagramView1.Diagram.NodeSelected +=
                new MindFusion.Diagramming.NodeEventHandler(NoteSelectedHandler);

            diagramView1.Diagram.NodeDeselected +=
                new MindFusion.Diagramming.NodeEventHandler(NoteDeselectedHandler);

            diagramView1.Diagram.NodeClicked +=
                new MindFusion.Diagramming.NodeEventHandler(NodeSingleClickHandler);

            //NodeDoubleClicked += new MindFusion.Diagramming.NodeEventHandler(NodeDoubleClickHandler);

            //NodeStartModifying += new MindFusion.Diagramming.NodeValidationEventHandler(NodeStartModifyingHandler);

            diagramView1.Diagram.NodeModified +=
                new MindFusion.Diagramming.NodeEventHandler(NodeModifiedHandler);

            diagramView1.Diagram.DoubleClicked +=
                new MindFusion.Diagramming.DiagramEventHandler(DiagramDoubleClickHandler);

            diagramView1.Diagram.BoundsChanged +=
                new System.EventHandler(SizeChangedHandler);

            diagramView1.Diagram.Clicked +=
                new DiagramEventHandler(DiagramClickedHandler);
        }
    }
}
