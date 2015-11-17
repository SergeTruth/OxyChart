using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace iChart
{
    public partial class MainUI : Form
    {
        public void MenuUndo()
        {
            if (editing|splitContainer1.ActiveControl == diagramView1)
            {
                Undo();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.Undo();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.Undo();
            }
        }

        public void MenuRedo()
        {
            if (editing|splitContainer1.ActiveControl == diagramView1)
            {
                Redo();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.Undo();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.Redo();
            }
        }

        public void MenuCut()
        {
            if (editing|splitContainer1.ActiveControl == diagramView1)
            {
                Cut();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.Cut();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.Cut();
            }
        }

        public void MenuCopy()
        {
            if (editing|splitContainer1.ActiveControl == diagramView1)
            {
                Copy();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.Copy();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.Copy();
            }
        }

        public void MenuPaste()
        {
            if (editing|splitContainer1.ActiveControl == diagramView1)
            {
                Paste();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.Paste();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.Paste();
            }
        }

        public void MenuSelectAll()
        {
            SelectAll();
        }
    }
}
