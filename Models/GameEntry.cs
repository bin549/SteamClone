using System;
using Avalonia.Media.Imaging;

namespace steam.Models;

public class GameEntry {
	public string Name { get; }
	public string ExecutablePath { get; }
	public string WorkingDirectory { get; }
	public string? ImagePath { get; }

	private Bitmap? coverImage;
	public Bitmap? CoverImage {
		get {
			if (this.coverImage != null || string.IsNullOrWhiteSpace(ImagePath)) {
				return this.coverImage;
			}
			try {
				this.coverImage = new Bitmap(ImagePath);
			} catch {
				this.coverImage = null;
			}
			return this.coverImage;
		}
	}

	public GameEntry(string name, string executablePath, string workingDirectory, string? imagePath) {
		Name = name;
		ExecutablePath = executablePath;
		WorkingDirectory = workingDirectory;
		ImagePath = imagePath;
	}
}
