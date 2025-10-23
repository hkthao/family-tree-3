namespace backend.Application.Common.Interfaces;

public interface IConfigProvider
{
    T GetSection<T>() where T : new();
}
