using Curves.ViewModels;

namespace Curves
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Model = new MainViewModel();
            DataContext = Model;
        }

        private MainViewModel Model { get; }
    }
}