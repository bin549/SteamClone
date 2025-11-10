using Avalonia.Controls;
using steam.ViewModels;

namespace steam;

public partial class MainWindow : Window {
	public MainWindow() {
		InitializeComponent();
		DataContext = new MainViewModel();
		if (DataContext is MainViewModel vm) {
			vm.Refresh();
		}
	}
}