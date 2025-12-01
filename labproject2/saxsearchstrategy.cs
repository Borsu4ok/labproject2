using System.Collections.Generic;
using System.Xml;

namespace labbproject2
{
    public class SaxSearchStrategy : ISearchStrategy
    {
        public List<Book> Search(string filePath, string categoryCriteria)
        {
            var result = new List<Book>();
            var currentBook = new Book();
            string currentElement = "";

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        currentElement = reader.Name;
                        if (currentElement == "Book")
                        {
                            currentBook = new Book();

                            string catAttribute = reader.GetAttribute("Category");
                            if (catAttribute != null)
                            {
                                currentBook.Category = catAttribute;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        switch (currentElement)
                        {
                            case "Author": currentBook.Author = reader.Value; break;
                            case "Title": currentBook.Title = reader.Value; break;
                            case "Name": currentBook.ReaderName = reader.Value; break;
                            case "Department": currentBook.ReaderDept = reader.Value; break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Book")
                    {
                        if (string.IsNullOrEmpty(categoryCriteria) ||
                            currentBook.Category == categoryCriteria)
                        {
                            result.Add(currentBook);
                        }
                    }
                }
            }
            return result;
        }
    }
}