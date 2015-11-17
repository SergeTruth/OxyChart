using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MindFusion.Diagramming;

namespace iChart
{
    public partial class SearchForm : Form
    {

        // The index of the current datagram item.
        int index = 0;

        // A reference to the main UI window.
        MainUI mainUI;

        public SearchForm(MainUI _mainUI)
        {
            InitializeComponent();
            mainUI = _mainUI;
        }

        /// <summary>
        /// The search algorithm.
        /// </summary>
        int Search(String text, int index)
        {
            // Get a reference to the diagram.
            Diagram datagram = mainUI.GetDiagramView().Diagram;

            // Check if the index needs to be wrapped around.
            if (index > datagram.Items.Count)
            {
                index = 0;
            }

            // Iterate through all the nodes and check them for the search string.
            for (int i = index; i < datagram.Items.Count; i++)
            {
                // Get a reference to a node.
                DiagramItem item = datagram.Items[i];
                if (item is ShapeNode)
                {
                    ShapeNode note = (ShapeNode)item;
                    // Check if the node contains the search string.
                    if (note.Text.ToUpper().Contains(text.ToUpper()) |
                        (note.Tag != null && note.Tag.ToString().ToUpper().Contains(text.ToUpper())))
                    {
                        mainUI.FocusOn(note);
                        return i;
                    }
                }
            }

            for (int i = 0; i < index; i++)
            {
                // Get a reference to a node.
                DiagramItem item = datagram.Items[i];
                if (item is ShapeNode)
                {
                    ShapeNode note = (ShapeNode)item;
                    // Check if the node contains the search string.
                    if (note.Text.ToUpper().Contains(text.ToUpper()) |
                        (note.Tag != null && note.Tag.ToString().ToUpper().Contains(text.ToUpper())))
                    {
                        mainUI.FocusOn(note);
                        return i;
                    }
                }
            }

            // If nothing is found, return 0.
            return 0;
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            index = Search(textSearchBox.Text, index);
            index++;
        }
    }
}