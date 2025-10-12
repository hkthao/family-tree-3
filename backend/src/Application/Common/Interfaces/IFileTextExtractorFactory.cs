using System;

namespace backend.Application.Common.Interfaces
{
    public interface IFileTextExtractorFactory
    {
        IFileTextExtractor GetExtractor(string fileExtension);
    }
}
