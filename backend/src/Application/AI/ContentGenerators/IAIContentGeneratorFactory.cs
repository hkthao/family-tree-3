using backend.Application.Common.Interfaces;

namespace backend.Application.AI.ContentGenerators;

public interface IAIContentGeneratorFactory
{
    IAIContentGenerator GetContentGenerator();
}