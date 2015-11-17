using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FluentSharp.REPL.Controls;

namespace iChart
{
    public partial class ScriptUI : Form
    {
        public ScriptUI()
        {
            InitializeComponent();
            ascx_Simple_Script_Editor ScriptEditor = new ascx_Simple_Script_Editor();
            //SourceCodeEditor ScriptEditor = new SourceCodeEditor();
            ScriptEditor.Parent = this;
            ScriptEditor.Dock = DockStyle.Fill;
            ScriptEditor.Show();
        }
    }
}
