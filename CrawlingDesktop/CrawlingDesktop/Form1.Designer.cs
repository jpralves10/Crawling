namespace CrawlingDesktop
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.textSearch = new System.Windows.Forms.TextBox();
            this.listSearched = new System.Windows.Forms.ListView();
            this.titulo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.link = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.subtitulo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(713, 41);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 0;
            this.buttonSearch.Text = "Pesquisar";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.button1_ClickAsync);
            // 
            // textSearch
            // 
            this.textSearch.Location = new System.Drawing.Point(397, 41);
            this.textSearch.Name = "textSearch";
            this.textSearch.Size = new System.Drawing.Size(310, 22);
            this.textSearch.TabIndex = 1;
            // 
            // listSearched
            // 
            this.listSearched.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.listSearched.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.titulo,
            this.link,
            this.subtitulo});
            this.listSearched.FullRowSelect = true;
            this.listSearched.GridLines = true;
            this.listSearched.Location = new System.Drawing.Point(12, 80);
            this.listSearched.Name = "listSearched";
            this.listSearched.Size = new System.Drawing.Size(776, 316);
            this.listSearched.TabIndex = 2;
            this.listSearched.UseCompatibleStateImageBehavior = false;
            this.listSearched.View = System.Windows.Forms.View.Details;
            this.listSearched.SelectedIndexChanged += new System.EventHandler(this.listSearched_SelectedIndexChanged);
            // 
            // titulo
            // 
            this.titulo.Text = "Título";
            this.titulo.Width = 180;
            // 
            // link
            // 
            this.link.Text = "Link";
            this.link.Width = 180;
            // 
            // subtitulo
            // 
            this.subtitulo.Text = "Subtítulo";
            this.subtitulo.Width = 210;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(712, 415);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Limpar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // id
            // 
            this.id.Text = "Id";
            this.id.Width = 50;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listSearched);
            this.Controls.Add(this.textSearch);
            this.Controls.Add(this.buttonSearch);
            this.Name = "Form1";
            this.Text = "Crawling";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.TextBox textSearch;
        private System.Windows.Forms.ListView listSearched;
        private System.Windows.Forms.ColumnHeader titulo;
        private System.Windows.Forms.ColumnHeader link;
        private System.Windows.Forms.ColumnHeader subtitulo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader id;
    }
}

