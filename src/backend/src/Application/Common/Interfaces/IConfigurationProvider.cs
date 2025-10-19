namespace FamilyTree.Application.Common.Interfaces;

public interface IConfigurationProvider
{
    T GetValue<T>(string key, T defaultValue);
    T GetValue<T>(string key);
}
