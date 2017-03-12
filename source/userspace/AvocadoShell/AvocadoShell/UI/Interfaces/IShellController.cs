namespace AvocadoShell.Interfaces
{
    public interface IShellController
    {
        void EditFile(string path);
        void RunForeground(string path);
    }
}