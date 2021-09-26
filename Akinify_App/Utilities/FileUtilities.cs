using Newtonsoft.Json;
using System;
using System.IO;

namespace Akinify_App {
	public static class FileUtilities {
		public static T LoadJson<T>(string fileName) where T : class {
			string path = FileToAppDataPath($"{fileName}.json");
			if (!FileExists(path)) return null;

			string json = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static void SaveJson<T>(string fileName, T obj) where T : class {
			string path = FileToAppDataPath($"{fileName}.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(obj));
		}

		private static bool FileExists(string path) {
			return File.Exists(path);
		}

		private static string FileToAppDataPath(string fileName) {
			string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string akinifyFolder = Path.Combine(appDataFolder, "Akinify");
			if (!Directory.Exists(akinifyFolder)) {
				Directory.CreateDirectory(akinifyFolder);
			}
			return Path.Combine(akinifyFolder, fileName);
		}
	}
}
