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
        private Button btnLoadFile;
        private RichTextBox rtbResults;
        private Label lblCategory;
        private GroupBox gbStrategy;
        private Label lblFileStatus;

        private string currentXmlPath = "";
        private const string XslPath = "style.xsl";
        private const string HtmlPath = "output.html";
        private const string TempXmlPath = "filtered_temp.xml";

        private List<Book> currentSearchResults = new List<Book>();

        public Form1()
        {
            this.Text = "Laboratory Work 2";
            this.Size = new Size(600, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateInterface();
        }

        private void CreateInterface()
        {
            btnLoadFile = new Button { Text = "Select XML File", Location = new Point(20, 10), Size = new Size(150, 30) };
            lblFileStatus = new Label { Text = "No file selected", Location = new Point(180, 15), AutoSize = true, ForeColor = Color.Red };

            btnLoadFile.Click += BtnLoadFile_Click;
            this.Controls.Add(btnLoadFile);
            this.Controls.Add(lblFileStatus);

            gbStrategy = new GroupBox();
            gbStrategy.Text = "Strategy";
            gbStrategy.Location = new Point(20, 50);
            gbStrategy.Size = new Size(540, 70);
            gbStrategy.Font = new Font(this.Font, FontStyle.Bold);

            rbSax = new RadioButton { Text = "SAX", Location = new Point(20, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            rbDom = new RadioButton { Text = "DOM", Location = new Point(190, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            rbLinq = new RadioButton { Text = "LINQ", Location = new Point(350, 30), AutoSize = true, Checked = true, Font = new Font(this.Font, FontStyle.Regular) };

            gbStrategy.Controls.Add(rbSax);
            gbStrategy.Controls.Add(rbDom);
            gbStrategy.Controls.Add(rbLinq);
            this.Controls.Add(gbStrategy);

            lblCategory = new Label { Text = "Select Category (Attribute from File):", Location = new Point(20, 140), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            cmbCategories = new ComboBox();
            cmbCategories.Location = new Point(20, 165);
            cmbCategories.Size = new Size(540, 25);
            cmbCategories.DropDownStyle = ComboBoxStyle.DropDownList;

            this.Controls.Add(lblCategory);
            this.Controls.Add(cmbCategories);

            btnSearch = new Button { Text = "Search", Location = new Point(20, 210), Size = new Size(120, 40), BackColor = Color.LightBlue };
            btnClear = new Button { Text = "Clear", Location = new Point(150, 210), Size = new Size(120, 40), BackColor = Color.LightSalmon };
            btnTransform = new Button { Text = "HTML Export", Location = new Point(280, 210), Size = new Size(120, 40), BackColor = Color.LightGreen };
            btnExit = new Button { Text = "Exit", Location = new Point(440, 210), Size = new Size(120, 40), BackColor = Color.IndianRed, ForeColor = Color.White };

            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;
            btnTransform.Click += BtnTransform_Click;
            btnExit.Click += BtnExit_Click;

            this.Controls.Add(btnSearch);
            this.Controls.Add(btnClear);
            this.Controls.Add(btnTransform);
            this.Controls.Add(btnExit);

            rtbResults = new RichTextBox();
            rtbResults.Location = new Point(20, 270);
            rtbResults.Size = new Size(540, 450);
            rtbResults.ReadOnly = true;
            rtbResults.BorderStyle = BorderStyle.Fixed3D;
            rtbResults.BackColor = Color.WhiteSmoke;

            this.Controls.Add(rtbResults);
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentXmlPath = openFileDialog.FileName;
                    lblFileStatus.Text = Path.GetFileName(currentXmlPath);
                    lblFileStatus.ForeColor = Color.Green;
                    LoadCategories();
                }
            }
        }

        private void LoadCategories()
        {
            if (string.IsNullOrEmpty(currentXmlPath)) return;

            try
            {
                var doc = XDocument.Load(currentXmlPath);
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
            currentSearchResults.Clear();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            rtbResults.Clear();
            currentSearchResults.Clear();

            if (string.IsNullOrEmpty(currentXmlPath))
            {
                MessageBox.Show("Please select an XML file first.");
                return;
            }

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
                currentSearchResults = strategy.Search(currentXmlPath, category);
                DisplayResults(currentSearchResults);
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
                rtbResults.AppendText("--------------------\n");
            }
        }

        private void BtnTransform_Click(object sender, EventArgs e)
        {
            if (currentSearchResults.Count == 0)
            {
                MessageBox.Show("No data to export. Please perform a search first.");
                return;
            }

            try
            {
                GenerateFilteredXml(TempXmlPath, currentSearchResults);

                var transformer = new HtmlTransformer();
                transformer.Transform(TempXmlPath, XslPath, HtmlPath);

                var result = MessageBox.Show("HTML report created based on filtered data. Open it now?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
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

        private void GenerateFilteredXml(string path, List<Book> books)
        {
            var doc = new XDocument(
                new XElement("Library",
                    from book in books
                    select new XElement("Book",
                        new XAttribute("Category", book.Category),
                        new XElement("Author", book.Author),
                        new XElement("Title", book.Title),
                        new XElement("Reader",
                            new XElement("Name", book.ReaderName),
                            new XElement("Department", book.ReaderDept)
                        )
                    )
                )
            );
            doc.Save(path);
        }
    }
}