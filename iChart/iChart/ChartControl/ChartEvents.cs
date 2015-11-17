using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;

namespace iChart
{
    public partial class MainUI : Form
    {
        public void DiagramClickedHandler(Object sender, DiagramEventArgs e)
        {
            ProxyDiagram dia = new ProxyDiagram(diagramView1.Diagram);
            propertyGrid1.SelectedObject = dia;
        }


        /// <summary>
        /// React to the note being selected.
        /// </summary>
        public void NoteSelectedHandler(object sender, NodeEventArgs e)
        {
            if (sender is Diagram)
            {
                Diagram datagram = (Diagram)sender;
                if (e.Node is ShapeNode)
                {
                    //check if multiple notes are selected.                
                    if (datagram.Selection.Nodes.Count > 0)
                    {

                        ProxyNode[] selectedNotes = new ProxyNode[datagram.Selection.Nodes.Count + 1];
                        for (int i = 0; i < datagram.Selection.Nodes.Count; i++)
                        {
                            ShapeNode note = (ShapeNode)datagram.Selection.Nodes[i];                            
                            ProxyNode selectedNoteProxy = new ProxyNode(note);
                            selectedNotes[i] = selectedNoteProxy;
                        }
                        selectedNotes[datagram.Selection.Nodes.Count] = new ProxyNode((ShapeNode)e.Node);

                        propertyGrid1.SelectedObjects = selectedNotes;
                        ClearNodeEditor();
                        return;
                    }

                    else
                    {
                        //Load the proxy into the property grid.
                        if (e.Node is ShapeNode)
                        {
                            ShapeNode note = (ShapeNode)e.Node;
                            ProxyNode selectedNoteProxy = new ProxyNode(note);
                            propertyGrid1.SelectedObject = selectedNoteProxy;
                            //mainUI.nodeEditor1.TagBoxLoadText(note);
                            LoadFromNode(note);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// React to the note being deselected.
        /// </summary>
        public void NoteDeselectedHandler(object sender, NodeEventArgs e)
        {
            propertyGrid1.SelectedObject = null;
            //TODO: Clear and save the node editor text boxes.
            SaveToNode();
            ClearNodeEditor();
        }


        /// <summary>
        /// React to the select note dimensions being changed.
        /// </summary>
        public void NodeModifiedHandler(object sender, MindFusion.Diagramming.NodeEventArgs e)
        {
            // Reload the property grid.
            propertyGrid1.Refresh();
        }


        /// <summary>
        /// React to the datagram size being changed.
        /// </summary>
        public void SizeChangedHandler(object sender, EventArgs e)
        {
            // Reload the property grid
            propertyGrid1.Refresh();
        }


        /// <summary>
        /// React to the note being clicked.
        /// </summary>
        public void NodeSingleClickHandler(object sender, NodeEventArgs e)
        {
            e.Node.Selected = true;
            /*if (e.Node is ShapeNode)
            {
                clickedNote = (ShapeNode)e.Node;
            }*/
            if (e.MouseButton == MouseButton.Right)
            {
                int offsetX = (int)diagramView1.ScrollX;
                int offsetY = (int)diagramView1.ScrollY;
                menuNote.Show(
                    diagramView1,
                    (int)((e.MousePosition.X - offsetX) * (diagramView1.ZoomFactor / 100)),
                    (int)((e.MousePosition.Y - offsetY) * (diagramView1.ZoomFactor / 100)));
            }
        }


        /// <summary>
        /// Double click event handler.
        /// </summary>
        public void DiagramDoubleClickHandler(object sender, DiagramEventArgs e)
        {
            float x = e.MousePosition.X - 20 - (e.MousePosition.X % 8);
            float y = e.MousePosition.Y - 20 - (e.MousePosition.Y % 8);
            MakeNode(x, y);
        }

        /// <summary>
        /// React to the wheel being scrolled by zooming in or out.
        /// </summary>
        public void MouseWheelHandler(object sender, MouseEventArgs args)
        {
            DiagramView datagramView = sender as DiagramView;
            Diagram datagram = datagramView.Diagram;


            float zoom = datagramView.ZoomFactor + args.Delta / 16;
            if (zoom > 1 && zoom <= 100)
            {
                float midX;
                float midY;
                midX = datagramView.Bounds.Width / 2;
                midY = datagramView.Bounds.Height / 2;
                midX /= datagramView.ZoomFactor / 100;
                midY /= datagramView.ZoomFactor / 100;
                midX += datagramView.ScrollX;
                midY += datagramView.ScrollY;
                datagramView.ZoomFactor = zoom;
                CenterPoint(midX, midY);
            }
        }


        /// <summary>
        /// In-place Edit.
        /// </summary>
        public void EnterInplaceEditMode(object sender, InPlaceEditEventArgs e)
        {

            //a redundant check
            if (e.EditControl is TextBox)
            {
                editing = true;
                editControl = e.EditControl;
                TextBox editBox = (TextBox)e.EditControl;
                //remove the border from the textbox
                editBox.BorderStyle = BorderStyle.None;
                //make sure we are editing a note
                if (e.Node is ShapeNode)
                {
                    ShapeNode note = (ShapeNode)e.Node;
                    //set the edit box text alignment
                    if (note.TextFormat.Alignment == StringAlignment.Center)
                    {
                        editBox.TextAlign = HorizontalAlignment.Center;
                    }
                    else if (note.TextFormat.Alignment == StringAlignment.Far)
                    {
                        editBox.TextAlign = HorizontalAlignment.Right;
                    }
                    else if (note.TextFormat.Alignment == StringAlignment.Near)
                    {
                        editBox.TextAlign = HorizontalAlignment.Left;
                    }
                    //set the edit box font size
                    float zoom = diagramView1.ZoomFactor / 100;
                    System.Drawing.Font font = new Font(note.Font.FontFamily, note.Font.Size * zoom);
                    editBox.Font = font;
                    editBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }
        }

        private void LeaveInplaceEditMode(object sender, InPlaceEditEventArgs e)
        {
            editing = false;
            editControl = null;
        }


        public void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
            {
                Cut();
                return;
            }
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                Copy();
                return;
            }
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                Paste();
                return;
            }
            if (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control)
            {
                diagramView1.Diagram.UndoManager.Undo();
                return;
            }
            if (e.KeyCode == Keys.Y && e.Modifiers == Keys.Control)
            {
                diagramView1.Diagram.UndoManager.Redo();
                return;
            }
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                SelectAll();
                return;
            }
            if (e.KeyCode == Keys.Back)
            {
                if (diagramView1.Diagram.Selection.Nodes.Count > 0)
                {
                    DiagramNode node = diagramView1.Diagram.Selection.Nodes[0];
                    diagramView1.Diagram.Nodes.Remove(node);
                }                
                return;
            }

            DiagramItemCollection selectedItems = diagramView1.Diagram.Selection.Items;


            if (e.KeyCode == Keys.S)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        float selectedBoxNewX, selectedBoxNewY;
                        selectedBoxNewX = selectedBox.Bounds.X;
                        selectedBoxNewY = selectedBox.Bounds.Y + 8;
                        selectedBox.Move(selectedBoxNewX, selectedBoxNewY);
                    }
                }
                return;
            }
            if (e.KeyCode == Keys.W)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        float selectedBoxNewX, selectedBoxNewY;
                        selectedBoxNewX = selectedBox.Bounds.X;
                        selectedBoxNewY = selectedBox.Bounds.Y - 8;
                        selectedBox.Move(selectedBoxNewX, selectedBoxNewY);
                        //if auto resize is enabled and the new position is outside document bounds
                        //resize the document to fit the moved item
                        //if(selectedBoxNewY>datagram.
                        //datagram.ResizeToFitItem(selectedBox);

                    }
                }
                return;
            }
            if (e.KeyCode == Keys.A)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        float selectedBoxNewX, selectedBoxNewY;
                        selectedBoxNewX = selectedBox.Bounds.X - 8;
                        selectedBoxNewY = selectedBox.Bounds.Y;
                        selectedBox.Move(selectedBoxNewX, selectedBoxNewY);
                        //if auto resize is enabled and the new position is outside document bounds
                        //resize the document to fit the moved item
                        //if(selectedBoxNewY>datagram.
                        //datagram.ResizeToFitItem(selectedBox);
                    }
                }
                return;
            }
            if (e.KeyCode == Keys.D)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        float selectedBoxNewX, selectedBoxNewY;
                        selectedBoxNewX = selectedBox.Bounds.X + 8;
                        selectedBoxNewY = selectedBox.Bounds.Y;
                        selectedBox.Move(selectedBoxNewX, selectedBoxNewY);
                        //if auto resize is enabled and the new position is outside document bounds
                        //resize the document to fit the moved item
                        //if(selectedBoxNewY>datagram.
                        //datagram.ResizeToFitItem(selectedBox);
                    }
                }
                return;
            }

            if (e.KeyCode == Keys.Oemplus)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        ProxyNode node = new ProxyNode(selectedBox);
                        node.SnapToGrid();
                        node.IncreaseWidthByGrid();
                    }
                }
                return;
            }

            if (e.KeyCode == Keys.OemMinus)
            {
                foreach (DiagramItem selectedItem in selectedItems)
                {
                    if (selectedItem is ShapeNode)
                    {
                        ShapeNode selectedBox = (ShapeNode)selectedItem;
                        ProxyNode node = new ProxyNode(selectedBox);
                        node.SnapToGrid();
                        node.DecreaseWidthByGrid();
                    }
                }
                return;
            }
        }
    }
}
