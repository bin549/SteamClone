using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using steam.Models;
using steam.Services;
using steam.Utils;

namespace steam.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;

	private void RaisePropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private string _libraryPath = ResolveDefaultLibraryPath();
	public string LibraryPath {
		get => _libraryPath;
		set {
			if (_libraryPath == value) return;
			_libraryPath = value;
			RaisePropertyChanged();
		}
	}

	private string _searchText = string.Empty;
	public string SearchText {
		get => _searchText;
		set {
			if (_searchText == value) return;
			_searchText = value;
			RaisePropertyChanged();
			RaisePropertyChanged(nameof(FilteredGames));
		}
	}

	public List<string> Categories { get; } = new() { "全部", "Game", "Desktop", "Web" };

	private string _selectedCategory = "全部";
	public string SelectedCategory {
		get => _selectedCategory;
		set {
			if (_selectedCategory == value) return;
			_selectedCategory = value;
			RaisePropertyChanged();
			RaisePropertyChanged(nameof(FilteredGames));
		}
	}

	public ObservableCollection<GameEntry> Games { get; } = new();

	public IEnumerable<GameEntry> FilteredGames {
		get {
			var filtered = Games.AsEnumerable();
			
			if (SelectedCategory != "全部") {
				filtered = filtered.Where(g => g.Category == SelectedCategory);
			}
			
			if (!string.IsNullOrWhiteSpace(SearchText)) {
				filtered = filtered.Where(g => g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
					|| g.ExecutablePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
			}
			
			return filtered;
		}
	}

	public ICommand RefreshCommand { get; }
	public ICommand OpenCommand { get; }

	public MainViewModel() {
		RefreshCommand = new RelayCommand(Refresh);
		OpenCommand = new RelayCommand<GameEntry>(OpenGame);
	}

	public void Refresh() {
		Games.Clear();
		var categories = new[] { "Game", "Desktop", "Web" };
		foreach (var category in categories) {
			var categoryPath = Path.Combine(LibraryPath, category);
			if (Directory.Exists(categoryPath)) {
				var entries = LibraryScanner.Scan(categoryPath, category);
				foreach (var e in entries) {
					Games.Add(e);
				}
			}
		}
		RaisePropertyChanged(nameof(FilteredGames));
	}

	private void OpenGame(GameEntry? entry) {
		if (entry == null) return;
		try {
			if (!string.IsNullOrWhiteSpace(entry.Url)) {
				var psi = new ProcessStartInfo {
					FileName = entry.Url,
					UseShellExecute = true
				};
				Process.Start(psi);
			} else {
				var psi = new ProcessStartInfo(entry.ExecutablePath) {
					WorkingDirectory = entry.WorkingDirectory,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		}
	}

	private static string ResolveDefaultLibraryPath() {
		var env = Environment.GetEnvironmentVariable("MG_LIBRARY_PATH");
		if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env)) {
			return env;
		}
		var defaultPath = @"F:\Apps";
		if (Directory.Exists(defaultPath)) {
			return defaultPath;
		}
		var candidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Game"));
		return Directory.Exists(candidate) ? candidate : defaultPath;
	}
}


