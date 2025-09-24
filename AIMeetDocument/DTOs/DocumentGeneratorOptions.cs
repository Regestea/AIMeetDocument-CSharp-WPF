using System.Collections.Generic;
using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs
{
    public class DocumentGeneratorOptions : GeneratorOptionsBase
    {
        public OperationType OperationType { get; set; }
        public PagePreRequest PagePreRequest { get; set; }
        public List<PageRange> PageRanges { get; set; }
        public string PdfFilePath { get; set; }
    }
}
