using System.Collections.Generic;
using System.Xml;

namespace labbproject2
{
    public class DomSearchStrategy : ISearchStrategy
    {
        public List<Book> Search(string filePath, string categoryCriteria)
        {
            var result = new List<Book>();
            var doc = new XmlDocument();
            doc.Load(filePath);

            XmlNodeList nodes = doc.SelectNodes("//Book");
            if (nodes == null) return result;

            foreach (XmlNode node in nodes)
            {
                var book = new Book();

                if (node.Attributes["Category"] != null)
                {
                    book.Category = node.Attributes["Category"].Value;
                }

                book.Author = node["Author"]?.InnerText ?? "";
                book.Title = node["Title"]?.InnerText ?? "";

                var reader = node["Reader"];
                if (reader != null)
                {
                    book.ReaderName = reader["Name"]?.InnerText ?? "";
                    book.ReaderDept = reader["Department"]?.InnerText ?? "";
                }

                if (string.IsNullOrEmpty(categoryCriteria) || book.Category == categoryCriteria)
                {
                    result.Add(book);
                }
            }
            return result;
        }
    }
}