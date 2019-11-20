namespace projekt
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Wektory");
            this.pictureBoxWczytanyObraz = new System.Windows.Forms.PictureBox();
            this.buttonZPliku = new System.Windows.Forms.Button();
            this.buttonZKamery = new System.Windows.Forms.Button();
            this.groupBoxWczytywanieObrazu = new System.Windows.Forms.GroupBox();
            this.pictureBoxPoSegmentacji = new System.Windows.Forms.PictureBox();
            this.labelWczytanyObraz = new System.Windows.Forms.Label();
            this.labelSegmentacjaObraz = new System.Windows.Forms.Label();
            this.groupBoxSegmentacja = new System.Windows.Forms.GroupBox();
            this.labelOpisElementów3 = new System.Windows.Forms.Label();
            this.labelOpisElementów2 = new System.Windows.Forms.Label();
            this.labelProg = new System.Windows.Forms.Label();
            this.labelOpisElementów1 = new System.Windows.Forms.Label();
            this.numericUpDownProg = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWyborElementu = new System.Windows.Forms.NumericUpDown();
            this.buttonKopiujElement = new System.Windows.Forms.Button();
            this.buttonRozpocznijSegmentacje = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBoxRuchPilki = new System.Windows.Forms.GroupBox();
            this.listViewListaWektorow = new System.Windows.Forms.ListView();
            this.Wektory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonPokazSciezke = new System.Windows.Forms.Button();
            this.buttonPokazWektory = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWczytanyObraz)).BeginInit();
            this.groupBoxWczytywanieObrazu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoSegmentacji)).BeginInit();
            this.groupBoxSegmentacja.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWyborElementu)).BeginInit();
            this.groupBoxRuchPilki.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxWczytanyObraz
            // 
            this.pictureBoxWczytanyObraz.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBoxWczytanyObraz.Location = new System.Drawing.Point(16, 33);
            this.pictureBoxWczytanyObraz.Name = "pictureBoxWczytanyObraz";
            this.pictureBoxWczytanyObraz.Size = new System.Drawing.Size(456, 272);
            this.pictureBoxWczytanyObraz.TabIndex = 0;
            this.pictureBoxWczytanyObraz.TabStop = false;
            // 
            // buttonZPliku
            // 
            this.buttonZPliku.Location = new System.Drawing.Point(24, 21);
            this.buttonZPliku.Name = "buttonZPliku";
            this.buttonZPliku.Size = new System.Drawing.Size(150, 35);
            this.buttonZPliku.TabIndex = 1;
            this.buttonZPliku.Text = "Z pliku";
            this.buttonZPliku.UseVisualStyleBackColor = true;
            this.buttonZPliku.Click += new System.EventHandler(this.buttonZPliku_Click);
            // 
            // buttonZKamery
            // 
            this.buttonZKamery.Location = new System.Drawing.Point(180, 21);
            this.buttonZKamery.Name = "buttonZKamery";
            this.buttonZKamery.Size = new System.Drawing.Size(150, 35);
            this.buttonZKamery.TabIndex = 2;
            this.buttonZKamery.Text = "Z kamery";
            this.buttonZKamery.UseVisualStyleBackColor = true;
            this.buttonZKamery.Click += new System.EventHandler(this.buttonZKamery_Click);
            // 
            // groupBoxWczytywanieObrazu
            // 
            this.groupBoxWczytywanieObrazu.Controls.Add(this.buttonZKamery);
            this.groupBoxWczytywanieObrazu.Controls.Add(this.buttonZPliku);
            this.groupBoxWczytywanieObrazu.Location = new System.Drawing.Point(494, 33);
            this.groupBoxWczytywanieObrazu.Name = "groupBoxWczytywanieObrazu";
            this.groupBoxWczytywanieObrazu.Size = new System.Drawing.Size(351, 68);
            this.groupBoxWczytywanieObrazu.TabIndex = 3;
            this.groupBoxWczytywanieObrazu.TabStop = false;
            this.groupBoxWczytywanieObrazu.Text = "Wczytywanie obrazu";
            // 
            // pictureBoxPoSegmentacji
            // 
            this.pictureBoxPoSegmentacji.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBoxPoSegmentacji.Location = new System.Drawing.Point(16, 341);
            this.pictureBoxPoSegmentacji.Name = "pictureBoxPoSegmentacji";
            this.pictureBoxPoSegmentacji.Size = new System.Drawing.Size(456, 272);
            this.pictureBoxPoSegmentacji.TabIndex = 4;
            this.pictureBoxPoSegmentacji.TabStop = false;
            // 
            // labelWczytanyObraz
            // 
            this.labelWczytanyObraz.AutoSize = true;
            this.labelWczytanyObraz.Location = new System.Drawing.Point(13, 12);
            this.labelWczytanyObraz.Name = "labelWczytanyObraz";
            this.labelWczytanyObraz.Size = new System.Drawing.Size(109, 17);
            this.labelWczytanyObraz.TabIndex = 5;
            this.labelWczytanyObraz.Text = "Wczytany obraz";
            // 
            // labelSegmentacjaObraz
            // 
            this.labelSegmentacjaObraz.AutoSize = true;
            this.labelSegmentacjaObraz.Location = new System.Drawing.Point(13, 321);
            this.labelSegmentacjaObraz.Name = "labelSegmentacjaObraz";
            this.labelSegmentacjaObraz.Size = new System.Drawing.Size(138, 17);
            this.labelSegmentacjaObraz.TabIndex = 5;
            this.labelSegmentacjaObraz.Text = "Segmentacja obrazu";
            // 
            // groupBoxSegmentacja
            // 
            this.groupBoxSegmentacja.Controls.Add(this.labelOpisElementów3);
            this.groupBoxSegmentacja.Controls.Add(this.labelOpisElementów2);
            this.groupBoxSegmentacja.Controls.Add(this.labelProg);
            this.groupBoxSegmentacja.Controls.Add(this.labelOpisElementów1);
            this.groupBoxSegmentacja.Controls.Add(this.numericUpDownProg);
            this.groupBoxSegmentacja.Controls.Add(this.numericUpDownWyborElementu);
            this.groupBoxSegmentacja.Controls.Add(this.buttonKopiujElement);
            this.groupBoxSegmentacja.Controls.Add(this.buttonRozpocznijSegmentacje);
            this.groupBoxSegmentacja.Location = new System.Drawing.Point(494, 107);
            this.groupBoxSegmentacja.Name = "groupBoxSegmentacja";
            this.groupBoxSegmentacja.Size = new System.Drawing.Size(351, 192);
            this.groupBoxSegmentacja.TabIndex = 6;
            this.groupBoxSegmentacja.TabStop = false;
            this.groupBoxSegmentacja.Text = "Segmentacja";
            // 
            // labelOpisElementów3
            // 
            this.labelOpisElementów3.AutoSize = true;
            this.labelOpisElementów3.Location = new System.Drawing.Point(21, 103);
            this.labelOpisElementów3.Name = "labelOpisElementów3";
            this.labelOpisElementów3.Size = new System.Drawing.Size(54, 17);
            this.labelOpisElementów3.TabIndex = 3;
            this.labelOpisElementów3.Text = "3- piłka";
            // 
            // labelOpisElementów2
            // 
            this.labelOpisElementów2.AutoSize = true;
            this.labelOpisElementów2.Location = new System.Drawing.Point(21, 86);
            this.labelOpisElementów2.Name = "labelOpisElementów2";
            this.labelOpisElementów2.Size = new System.Drawing.Size(65, 17);
            this.labelOpisElementów2.TabIndex = 3;
            this.labelOpisElementów2.Text = "2- ściany";
            // 
            // labelProg
            // 
            this.labelProg.AutoSize = true;
            this.labelProg.Location = new System.Drawing.Point(207, 21);
            this.labelProg.Name = "labelProg";
            this.labelProg.Size = new System.Drawing.Size(38, 17);
            this.labelProg.TabIndex = 3;
            this.labelProg.Text = "Próg";
            // 
            // labelOpisElementów1
            // 
            this.labelOpisElementów1.AutoSize = true;
            this.labelOpisElementów1.Location = new System.Drawing.Point(21, 69);
            this.labelOpisElementów1.Name = "labelOpisElementów1";
            this.labelOpisElementów1.Size = new System.Drawing.Size(62, 17);
            this.labelOpisElementów1.TabIndex = 3;
            this.labelOpisElementów1.Text = "1- droga";
            // 
            // numericUpDownProg
            // 
            this.numericUpDownProg.Location = new System.Drawing.Point(210, 42);
            this.numericUpDownProg.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownProg.Name = "numericUpDownProg";
            this.numericUpDownProg.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownProg.TabIndex = 2;
            this.numericUpDownProg.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // numericUpDownWyborElementu
            // 
            this.numericUpDownWyborElementu.Location = new System.Drawing.Point(24, 123);
            this.numericUpDownWyborElementu.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownWyborElementu.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWyborElementu.Name = "numericUpDownWyborElementu";
            this.numericUpDownWyborElementu.Size = new System.Drawing.Size(120, 22);
            this.numericUpDownWyborElementu.TabIndex = 2;
            this.numericUpDownWyborElementu.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonKopiujElement
            // 
            this.buttonKopiujElement.Location = new System.Drawing.Point(24, 151);
            this.buttonKopiujElement.Name = "buttonKopiujElement";
            this.buttonKopiujElement.Size = new System.Drawing.Size(150, 35);
            this.buttonKopiujElement.TabIndex = 1;
            this.buttonKopiujElement.Text = "Kopiuj element";
            this.buttonKopiujElement.UseVisualStyleBackColor = true;
            this.buttonKopiujElement.Click += new System.EventHandler(this.buttonKopiujElement_Click);
            // 
            // buttonRozpocznijSegmentacje
            // 
            this.buttonRozpocznijSegmentacje.Location = new System.Drawing.Point(24, 21);
            this.buttonRozpocznijSegmentacje.Name = "buttonRozpocznijSegmentacje";
            this.buttonRozpocznijSegmentacje.Size = new System.Drawing.Size(150, 43);
            this.buttonRozpocznijSegmentacje.TabIndex = 0;
            this.buttonRozpocznijSegmentacje.Text = "Rozpocznij segmentacje";
            this.buttonRozpocznijSegmentacje.UseVisualStyleBackColor = true;
            this.buttonRozpocznijSegmentacje.Click += new System.EventHandler(this.buttonRozpocznijSegmentacje_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // groupBoxRuchPilki
            // 
            this.groupBoxRuchPilki.Controls.Add(this.listViewListaWektorow);
            this.groupBoxRuchPilki.Controls.Add(this.buttonPokazSciezke);
            this.groupBoxRuchPilki.Controls.Add(this.buttonPokazWektory);
            this.groupBoxRuchPilki.Location = new System.Drawing.Point(494, 305);
            this.groupBoxRuchPilki.Name = "groupBoxRuchPilki";
            this.groupBoxRuchPilki.Size = new System.Drawing.Size(351, 309);
            this.groupBoxRuchPilki.TabIndex = 8;
            this.groupBoxRuchPilki.TabStop = false;
            this.groupBoxRuchPilki.Text = "Ruch piłki";
            // 
            // listViewListaWektorow
            // 
            this.listViewListaWektorow.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Wektory});
            this.listViewListaWektorow.HideSelection = false;
            this.listViewListaWektorow.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listViewListaWektorow.Location = new System.Drawing.Point(24, 89);
            this.listViewListaWektorow.Name = "listViewListaWektorow";
            this.listViewListaWektorow.Size = new System.Drawing.Size(306, 294);
            this.listViewListaWektorow.TabIndex = 2;
            this.listViewListaWektorow.UseCompatibleStateImageBehavior = false;
            // 
            // Wektory
            // 
            this.Wektory.Text = "Wektory";
            // 
            // buttonPokazSciezke
            // 
            this.buttonPokazSciezke.Location = new System.Drawing.Point(180, 21);
            this.buttonPokazSciezke.Name = "buttonPokazSciezke";
            this.buttonPokazSciezke.Size = new System.Drawing.Size(150, 35);
            this.buttonPokazSciezke.TabIndex = 1;
            this.buttonPokazSciezke.Text = "Pokaż ścieżke";
            this.buttonPokazSciezke.UseVisualStyleBackColor = true;
            this.buttonPokazSciezke.Click += new System.EventHandler(this.buttonPokazSciezke_Click);
            // 
            // buttonPokazWektory
            // 
            this.buttonPokazWektory.Location = new System.Drawing.Point(24, 21);
            this.buttonPokazWektory.Name = "buttonPokazWektory";
            this.buttonPokazWektory.Size = new System.Drawing.Size(150, 35);
            this.buttonPokazWektory.TabIndex = 1;
            this.buttonPokazWektory.Text = "Pokaż wektory";
            this.buttonPokazWektory.UseVisualStyleBackColor = true;
            this.buttonPokazWektory.Click += new System.EventHandler(this.buttonPokazWektory_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 626);
            this.Controls.Add(this.groupBoxRuchPilki);
            this.Controls.Add(this.groupBoxSegmentacja);
            this.Controls.Add(this.labelSegmentacjaObraz);
            this.Controls.Add(this.labelWczytanyObraz);
            this.Controls.Add(this.pictureBoxPoSegmentacji);
            this.Controls.Add(this.groupBoxWczytywanieObrazu);
            this.Controls.Add(this.pictureBoxWczytanyObraz);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWczytanyObraz)).EndInit();
            this.groupBoxWczytywanieObrazu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPoSegmentacji)).EndInit();
            this.groupBoxSegmentacja.ResumeLayout(false);
            this.groupBoxSegmentacja.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownProg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWyborElementu)).EndInit();
            this.groupBoxRuchPilki.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxWczytanyObraz;
        private System.Windows.Forms.Button buttonZPliku;
        private System.Windows.Forms.Button buttonZKamery;
        private System.Windows.Forms.GroupBox groupBoxWczytywanieObrazu;
        private System.Windows.Forms.PictureBox pictureBoxPoSegmentacji;
        private System.Windows.Forms.Label labelWczytanyObraz;
        private System.Windows.Forms.Label labelSegmentacjaObraz;
        private System.Windows.Forms.GroupBox groupBoxSegmentacja;
        private System.Windows.Forms.Label labelOpisElementów3;
        private System.Windows.Forms.Label labelOpisElementów2;
        private System.Windows.Forms.Label labelOpisElementów1;
        private System.Windows.Forms.NumericUpDown numericUpDownWyborElementu;
        private System.Windows.Forms.Button buttonKopiujElement;
        private System.Windows.Forms.Button buttonRozpocznijSegmentacje;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.GroupBox groupBoxRuchPilki;
        private System.Windows.Forms.ListView listViewListaWektorow;
        private System.Windows.Forms.ColumnHeader Wektory;
        private System.Windows.Forms.Button buttonPokazSciezke;
        private System.Windows.Forms.Button buttonPokazWektory;
        private System.Windows.Forms.Label labelProg;
        private System.Windows.Forms.NumericUpDown numericUpDownProg;
    }
}

