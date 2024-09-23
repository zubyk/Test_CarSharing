using System.Windows;

namespace CarSharing.Abstractions
{
    internal interface IWindowsCreator
    {
        public T GetWindow<T>() where T : Window;
    }
}
