using System.Xml.Xsl;

namespace labbproject2
{
    public class HtmlTransformer
    {
        public void Transform(string xmlPath, string xslPath, string outputPath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var xsl = new XslCompiledTransform();
            xsl.Load(xslPath);
            xsl.Transform(xmlPath, outputPath);
        }
    }
}