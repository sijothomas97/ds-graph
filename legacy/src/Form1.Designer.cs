namespace TaskB
{
    partial class Form1
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
            this.tbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFirstPerson = new System.Windows.Forms.ComboBox();
            this.cbSecondPerson = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbNames = new System.Windows.Forms.ListBox();
            this.tbDirectFriends = new System.Windows.Forms.TextBox();
            this.btSave = new System.Windows.Forms.Button();
            this.btConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lbDirectFriends = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btFind = new System.Windows.Forms.Button();
            this.Names = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labEdgesCount = new System.Windows.Forms.Label();
            this.btDelete = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cbDeletePerson = new System.Windows.Forms.ComboBox();
            this.btAllFriends = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(99, 103);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(157, 22);
            this.tbName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name";
            // 
            // cbFirstPerson
            // 
            this.cbFirstPerson.FormattingEnabled = true;
            this.cbFirstPerson.Location = new System.Drawing.Point(99, 210);
            this.cbFirstPerson.Name = "cbFirstPerson";
            this.cbFirstPerson.Size = new System.Drawing.Size(157, 24);
            this.cbFirstPerson.TabIndex = 2;
            // 
            // cbSecondPerson
            // 
            this.cbSecondPerson.FormattingEnabled = true;
            this.cbSecondPerson.Location = new System.Drawing.Point(99, 249);
            this.cbSecondPerson.Name = "cbSecondPerson";
            this.cbSecondPerson.Size = new System.Drawing.Size(157, 24);
            this.cbSecondPerson.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 191);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Connect two people";
            // 
            // lbNames
            // 
            this.lbNames.FormattingEnabled = true;
            this.lbNames.ItemHeight = 16;
            this.lbNames.Location = new System.Drawing.Point(313, 103);
            this.lbNames.Name = "lbNames";
            this.lbNames.Size = new System.Drawing.Size(188, 324);
            this.lbNames.TabIndex = 5;
            // 
            // tbDirectFriends
            // 
            this.tbDirectFriends.Location = new System.Drawing.Point(562, 103);
            this.tbDirectFriends.Name = "tbDirectFriends";
            this.tbDirectFriends.Size = new System.Drawing.Size(157, 22);
            this.tbDirectFriends.TabIndex = 6;
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(181, 142);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(75, 23);
            this.btSave.TabIndex = 7;
            this.btSave.Text = "Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(181, 312);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(75, 23);
            this.btConnect.TabIndex = 8;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(559, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Name to find friends";
            // 
            // lbDirectFriends
            // 
            this.lbDirectFriends.FormattingEnabled = true;
            this.lbDirectFriends.ItemHeight = 16;
            this.lbDirectFriends.Location = new System.Drawing.Point(562, 211);
            this.lbDirectFriends.Name = "lbDirectFriends";
            this.lbDirectFriends.Size = new System.Drawing.Size(157, 212);
            this.lbDirectFriends.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(559, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 16);
            this.label4.TabIndex = 11;
            this.label4.Text = "Friends";
            // 
            // btFind
            // 
            this.btFind.Location = new System.Drawing.Point(562, 131);
            this.btFind.Name = "btFind";
            this.btFind.Size = new System.Drawing.Size(157, 23);
            this.btFind.TabIndex = 12;
            this.btFind.Text = "Direct friends";
            this.btFind.UseVisualStyleBackColor = true;
            this.btFind.Click += new System.EventHandler(this.btFind_Click);
            // 
            // Names
            // 
            this.Names.AutoSize = true;
            this.Names.Location = new System.Drawing.Point(310, 84);
            this.Names.Name = "Names";
            this.Names.Size = new System.Drawing.Size(0, 16);
            this.Names.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(310, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = "Names";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(96, 276);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 16);
            this.label6.TabIndex = 15;
            this.label6.Text = "Total connections :";
            // 
            // labEdgesCount
            // 
            this.labEdgesCount.AutoSize = true;
            this.labEdgesCount.Location = new System.Drawing.Point(212, 276);
            this.labEdgesCount.Name = "labEdgesCount";
            this.labEdgesCount.Size = new System.Drawing.Size(0, 16);
            this.labEdgesCount.TabIndex = 16;
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(181, 415);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(75, 23);
            this.btDelete.TabIndex = 18;
            this.btDelete.Text = "Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(96, 356);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 16);
            this.label7.TabIndex = 19;
            this.label7.Text = "Delete a person";
            // 
            // cbDeletePerson
            // 
            this.cbDeletePerson.FormattingEnabled = true;
            this.cbDeletePerson.Location = new System.Drawing.Point(99, 375);
            this.cbDeletePerson.Name = "cbDeletePerson";
            this.cbDeletePerson.Size = new System.Drawing.Size(157, 24);
            this.cbDeletePerson.TabIndex = 20;
            // 
            // btAllFriends
            // 
            this.btAllFriends.Location = new System.Drawing.Point(562, 160);
            this.btAllFriends.Name = "btAllFriends";
            this.btAllFriends.Size = new System.Drawing.Size(157, 23);
            this.btAllFriends.TabIndex = 21;
            this.btAllFriends.Text = "All friends";
            this.btAllFriends.UseVisualStyleBackColor = true;
            this.btAllFriends.Click += new System.EventHandler(this.button1_Click);
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(644, 429);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(75, 23);
            this.btClear.TabIndex = 22;
            this.btClear.Text = "Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 492);
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.btAllFriends);
            this.Controls.Add(this.cbDeletePerson);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btDelete);
            this.Controls.Add(this.labEdgesCount);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Names);
            this.Controls.Add(this.btFind);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbDirectFriends);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btConnect);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.tbDirectFriends);
            this.Controls.Add(this.lbNames);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbSecondPerson);
            this.Controls.Add(this.cbFirstPerson);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbName);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbFirstPerson;
        private System.Windows.Forms.ComboBox cbSecondPerson;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbNames;
        private System.Windows.Forms.TextBox tbDirectFriends;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lbDirectFriends;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btFind;
        private System.Windows.Forms.Label Names;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labEdgesCount;
        private System.Windows.Forms.Button btDelete;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbDeletePerson;
        private System.Windows.Forms.Button btAllFriends;
        private System.Windows.Forms.Button btClear;
    }
}

