using Curves.ViewModels;

namespace Curves
{
    public partial class MainWindow
    {
        private MainViewModel Model { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Model = new MainViewModel();
            DataContext = Model;
        }
    }
}
