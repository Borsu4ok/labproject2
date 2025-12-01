using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;

namespace labbproject2
{
    public class Form1 : Form
    {
        private RadioButton rbSax;
        private RadioButton rbDom;
        private RadioButton rbLinq;
        private ComboBox cmbCategories;
        private Button btnSearch;
        private Button btnClear;
        private Button btnTransform;
        private Button btnExit;
        private RichTextBox rtbResults;
        private Label lblCategory;
        private GroupBox gbStrategy;

        private const string XmlPath = "library.xml";
        private const string XslPath = "style.xsl";
        private const string HtmlPath = "output.html";

        public Form1()
        {
            this.Text = "Laboratory Work 2";
            this.Size = new Size(600, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateInterface();
            LoadCategories();
        }

        private void CreateInterface()
        {
            gbStrategy = new GroupBox();
            gbStrategy.Text = "Strategy";
            gbStrategy.Location = new Point(20, 20);
            gbStrategy.Size = new Size(540, 70);
            gbStrategy.Font = new Font(this.Font, FontStyle.Bold);

            rbSax = new RadioButton { Text = "SAX", Location = new Point(20, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            rbDom = new RadioButton { Text = "DOM", Location = new Point(190, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            rbLinq = new RadioButton { Text = "LINQ", Location = new Point(350, 30), AutoSize = true, Checked = true, Font = new Font(this.Font, FontStyle.Regular) };

            gbStrategy.Controls.Add(rbSax);
            gbStrategy.Controls.Add(rbDom);
            gbStrategy.Controls.Add(rbLinq);
            this.Controls.Add(gbStrategy);

            lblCategory = new Label { Text = "Select Category (Attribute from File):", Location = new Point(20, 110), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            cmbCategories = new ComboBox();
            cmbCategories.Location = new Point(20, 135);
            cmbCategories.Size = new Size(540, 25);
            cmbCategories.DropDownStyle = ComboBoxStyle.DropDownList;

            this.Controls.Add(lblCategory);
            this.Controls.Add(cmbCategories);

            btnSearch = new Button { Text = "Search", Location = new Point(20, 180), Size = new Size(120, 40), BackColor = Color.LightBlue };
            btnClear = new Button { Text = "Clear", Location = new Point(150, 180), Size = new Size(120, 40), BackColor = Color.LightSalmon };
            btnTransform = new Button { Text = "HTML Export", Location = new Point(280, 180), Size = new Size(120, 40), BackColor = Color.LightGreen };
            btnExit = new Button { Text = "Exit", Location = new Point(440, 180), Size = new Size(120, 40), BackColor = Color.IndianRed, ForeColor = Color.White };

            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;
            btnTransform.Click += BtnTransform_Click;
            btnExit.Click += BtnExit_Click;

            this.Controls.Add(btnSearch);
            this.Controls.Add(btnClear);
            this.Controls.Add(btnTransform);
            this.Controls.Add(btnExit);

            rtbResults = new RichTextBox();
            rtbResults.Location = new Point(20, 240);
            rtbResults.Size = new Size(540, 450);
            rtbResults.ReadOnly = true;
            rtbResults.BorderStyle = BorderStyle.Fixed3D;
            rtbResults.BackColor = Color.WhiteSmoke;

            this.Controls.Add(rtbResults);
        }

        private void LoadCategories()
        {
            try
            {
                var doc = XDocument.Load(XmlPath);
                var categories = doc.Descendants("Book")
                                   .Select(b => b.Attribute("Category")?.Value)
                                   .Where(c => c != null)
                                   .Distinct()
                                   .ToArray();

                cmbCategories.Items.Clear();
                cmbCategories.Items.AddRange(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show("Are you sure you want to exit the application?",
                                             "Confirm Exit",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            rtbResults.Clear();
            cmbCategories.SelectedIndex = -1;
            rbLinq.Checked = true;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            rtbResults.Clear();

            if (cmbCategories.SelectedItem == null)
            {
                MessageBox.Show("Please select a category first.");
                return;
            }

            string category = cmbCategories.SelectedItem.ToString();

            ISearchStrategy strategy = null;
            if (rbSax.Checked) strategy = new SaxSearchStrategy();
            else if (rbDom.Checked) strategy = new DomSearchStrategy();
            else if (rbLinq.Checked) strategy = new LinqSearchStrategy();

            try
            {
                List<Book> results = strategy.Search(XmlPath, category);
                DisplayResults(results);
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Error: " + ex.Message);
            }
        }

        private void DisplayResults(List<Book> books)
        {
            if (books.Count == 0)
            {
                rtbResults.Text = "No books found.";
                return;
            }

            rtbResults.AppendText($"Found: {books.Count}\n\n");
            foreach (var book in books)
            {
                rtbResults.AppendText($"Category: {book.Category}\n");
                rtbResults.AppendText($"Title: {book.Title}\n");
                rtbResults.AppendText($"Author: {book.Author}\n");
                rtbResults.AppendText($"Reader: {book.ReaderName} ({book.ReaderDept})\n");
            }
        }

        private void BtnTransform_Click(object sender, EventArgs e)
        {
            try
            {
                var transformer = new HtmlTransformer();
                transformer.Transform(XmlPath, XslPath, HtmlPath);

                var result = MessageBox.Show("HTML created successfully. Open it now?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(Path.GetFullPath(HtmlPath)) { UseShellExecute = true };
                    p.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transform Error: " + ex.Message);
            }
        }
    }
}