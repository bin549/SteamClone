using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using steam.Models;

namespace steam.Services;

public static class LibraryScanner {
	private static readonly string[] ImageNames = { "cover.png", "cover.jpg", "icon.png", "icon.jpg" };
	private static readonly string[] ExeIgnorePrefixes = { "UnityCrashHandler", "unins" };
	private static readonly string[] FolderIgnoreNames = { "_CommonRedist", "MonoBleedingEdge", "redist", "vcredist" };

	public static List<GameEntry> Scan(string rootDirectory, string category) {
		var results = new List<GameEntry>();
		if (!Directory.Exists(rootDirectory)) {
			return results;
		}
		if (category == "Web") {
			var urlsJsonPath = Path.Combine(rootDirectory, "urls.json");
			if (File.Exists(urlsJsonPath)) {
				try {
					var jsonContent = File.ReadAllText(urlsJsonPath);
					var options = new JsonSerializerOptions {
						PropertyNameCaseInsensitive = true
					};
					var webApps = JsonSerializer.Deserialize<List<WebAppConfig>>(jsonContent, options);
					if (webApps != null) {
						foreach (var webApp in webApps) {
							var appDir = Path.Combine(rootDirectory, webApp.Name);
							if (Directory.Exists(appDir)) {
								var entry = TryBuildWebEntry(appDir, webApp, category);
								if (entry != null) {
									results.Add(entry);
								}
							}
						}
					}
				} catch {
				}
			}
			return results.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
		}
		foreach (var appDir in Directory.GetDirectories(rootDirectory)) {
			try {
				var game = TryBuildEntry(appDir, category);
				if (game != null) {
					results.Add(game);
				}
			} catch {
			}
		}
		return results.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	private static GameEntry? TryBuildEntry(string appDir, string category) {
		var name = new DirectoryInfo(appDir).Name;
		var preferredExe = Path.Combine(appDir, name + ".exe");
		if (File.Exists(preferredExe)) {
			return CreateEntry(name, preferredExe, appDir, category);
		}
		var candidateExe = FindCandidateExe(appDir);
		if (candidateExe == null) {
			return null;
		}
		return CreateEntry(name, candidateExe, Path.GetDirectoryName(candidateExe) ?? appDir, category);
	}

	private static string? FindCandidateExe(string appDir) {
		var topLevelExe = Directory.GetFiles(appDir, "*.exe", SearchOption.TopDirectoryOnly)
			.FirstOrDefault(p => !ExeIgnorePrefixes.Any(pref => Path.GetFileName(p).StartsWith(pref, StringComparison.OrdinalIgnoreCase)));
		if (topLevelExe != null) {
			return topLevelExe;
		}
		foreach (var sub in Directory.GetDirectories(appDir)) {
			var subName = new DirectoryInfo(sub).Name;
			if (FolderIgnoreNames.Any(x => subName.Equals(x, StringComparison.OrdinalIgnoreCase))) {
				continue;
			}
			var exe = Directory.GetFiles(sub, "*.exe", SearchOption.AllDirectories)
				.FirstOrDefault(p => !Path.GetFileName(p).StartsWith("UnityCrashHandler", StringComparison.OrdinalIgnoreCase));
			if (exe != null) {
				return exe;
			}
		}
		return null;
	}

	private static GameEntry? TryBuildWebEntry(string appDir, WebAppConfig config, string category) {
		string? imagePath = null;
		if (!string.IsNullOrWhiteSpace(config.Icon)) {
			var iconPath = Path.Combine(appDir, config.Icon);
			if (File.Exists(iconPath)) {
				imagePath = iconPath;
			}
		}
		if (imagePath == null) {
			foreach (var imgName in ImageNames) {
				var candidate = Path.Combine(appDir, imgName);
				if (File.Exists(candidate)) {
					imagePath = candidate;
					break;
				}
			}
		}
		return new GameEntry(
			config.Name,
			string.Empty, 
			Path.GetFullPath(appDir),
			imagePath != null ? Path.GetFullPath(imagePath) : null,
			category,
			config.Url);
	}

	private static GameEntry CreateEntry(string name, string exePath, string workingDir, string category) {
		string? imagePath = null;
		foreach (var imgName in ImageNames) {
			var candidate = Path.Combine(workingDir, imgName);
			if (File.Exists(candidate)) {
				imagePath = candidate;
				break;
			}
		}
		return new GameEntry(
			name,
			Path.GetFullPath(exePath),
			Path.GetFullPath(workingDir),
			imagePath != null ? Path.GetFullPath(imagePath) : null,
			category);
	}

	private class WebAppConfig {
		public string Name { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
	}
}
