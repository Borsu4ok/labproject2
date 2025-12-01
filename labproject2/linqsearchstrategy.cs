using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace labbproject2
{
    public class LinqSearchStrategy : ISearchStrategy
    {
        public List<Book> Search(string filePath, string categoryCriteria)
        {
            var doc = XDocument.Load(filePath);

            var query = from b in doc.Descendants("Book")
                        let cat = b.Attribute("Category")?.Value ?? ""
                        where string.IsNullOrEmpty(categoryCriteria) || cat == categoryCriteria
                        select new Book
                        {
                            Category = cat,
                            Author = b.Element("Author")?.Value ?? "",
                            Title = b.Element("Title")?.Value ?? "",
                            ReaderName = b.Element("Reader")?.Element("Name")?.Value ?? "",
                            ReaderDept = b.Element("Reader")?.Element("Department")?.Value ?? ""
                        };

            return query.ToList();
        }
    }
}