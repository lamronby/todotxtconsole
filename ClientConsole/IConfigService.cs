namespace ClientConsole
{
    public interface IConfigService
    {
        string GetValue(string key);
        void SetValue(string key, string value);
    }
}