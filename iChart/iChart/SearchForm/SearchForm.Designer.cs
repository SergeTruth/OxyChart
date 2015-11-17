namespace iChart
{
    partial class SearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchForm));
            this.butSearch = new System.Windows.Forms.Button();
            this.textSearchBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // butSearch
            // 
            this.butSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butSearch.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butSearch.Location = new System.Drawing.Point(354, 2);
            this.butSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.butSearch.Name = "butSearch";
            this.butSearch.Size = new System.Drawing.Size(127, 100);
            this.butSearch.TabIndex = 3;
            this.butSearch.Text = "Search";
            this.butSearch.UseVisualStyleBackColor = true;
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // textSearchBox
            // 
            this.textSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textSearchBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textSearchBox.Location = new System.Drawing.Point(0, 2);
            this.textSearchBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textSearchBox.Multiline = true;
            this.textSearchBox.Name = "textSearchBox";
            this.textSearchBox.Size = new System.Drawing.Size(351, 100);
            this.textSearchBox.TabIndex = 2;
            // 
            // SearchForm
            // 
            this.AcceptButton = this.butSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 102);
            this.Controls.Add(this.butSearch);
            this.Controls.Add(this.textSearchBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "SearchForm";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butSearch;
        private System.Windows.Forms.TextBox textSearchBox;
    }
}