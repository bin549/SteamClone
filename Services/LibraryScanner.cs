using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using steam.Models;

namespace steam.Services;

public static class LibraryScanner {
	private static readonly string[] ImageNames = { "cover.png", "cover.jpg", "icon.png", "icon.jpg" };
	private static readonly string[] ExeIgnorePrefixes = { "UnityCrashHandler", "unins" };
	private static readonly string[] FolderIgnoreNames = { "_CommonRedist", "MonoBleedingEdge", "redist", "vcredist" };

	public static List<GameEntry> Scan(string rootDirectory) {
		var results = new List<GameEntry>();
		if (!Directory.Exists(rootDirectory)) {
			return results;
		}
		foreach (var appDir in Directory.GetDirectories(rootDirectory)) {
			try {
				var game = TryBuildEntry(appDir);
				if (game != null) {
					results.Add(game);
				}
			} catch {
			}
		}
		return results.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	private static GameEntry? TryBuildEntry(string appDir) {
		var name = new DirectoryInfo(appDir).Name;
		var preferredExe = Path.Combine(appDir, name + ".exe");
		if (File.Exists(preferredExe)) {
			return CreateEntry(name, preferredExe, appDir);
		}
		var candidateExe = FindCandidateExe(appDir);
		if (candidateExe == null) {
			return null;
		}
		return CreateEntry(name, candidateExe, Path.GetDirectoryName(candidateExe) ?? appDir);
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

	private static GameEntry CreateEntry(string name, string exePath, string workingDir) {
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
			imagePath != null ? Path.GetFullPath(imagePath) : null);
	}
}
