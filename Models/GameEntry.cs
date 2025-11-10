using System;
using Avalonia.Media.Imaging;

namespace steam.Models;

public class GameEntry {
	public string Name { get; }
	public string ExecutablePath { get; }
	public string WorkingDirectory { get; }
	public string? ImagePath { get; }

	private Bitmap? _coverImage;
	public Bitmap? CoverImage {
		get {
			if (_coverImage != null || string.IsNullOrWhiteSpace(ImagePath)) {
				return _coverImage;
			}
			try {
				_coverImage = new Bitmap(ImagePath);
			} catch {
				_coverImage = null;
			}
			return _coverImage;
		}
	}

	public GameEntry(string name, string executablePath, string workingDirectory, string? imagePath) {
		Name = name;
		ExecutablePath = executablePath;
		WorkingDirectory = workingDirectory;
		ImagePath = imagePath;
	}
}


