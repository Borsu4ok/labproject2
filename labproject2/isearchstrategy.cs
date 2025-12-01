using System.Collections.Generic;

namespace labbproject2
{
    public interface ISearchStrategy
    {
        List<Book> Search(string filePath, string categoryCriteria);
    }
}