using CustomFramework.CustomSubclasses;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Paths;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace CustomFramework
{
	public class DatabaseHandler
	{
		internal static DatabaseModel Database { get; set; } = new DatabaseModel();
		internal static string DatabasePath { get; set; } = Path.Combine(PathManager.Configs.FullName, "Custom Framework", $"{Server.Port}.yml");

		internal static void LoadDatabase()
		{
			var deserializer = new DeserializerBuilder().Build();

			if (!Directory.Exists(Path.Combine(DatabasePath, "../")))
			{
				Directory.CreateDirectory(Path.Combine(DatabasePath, "../"));
			}
			if (!File.Exists(DatabasePath))
			{
				Database = new DatabaseModel();
				SaveDatabase();
				return;
			}

			Database = deserializer.Deserialize<DatabaseModel>(File.ReadAllText(DatabasePath));
		}

		public static void SaveDatabase()
		{
			var serializer = new SerializerBuilder().Build();

			if (!Directory.Exists(Path.Combine(DatabasePath, "../")))
				Directory.CreateDirectory(Path.Combine(DatabasePath, "../"));

			Database.DisabledSubclasses = CustomSubclass.Disabled.Select(x => x.Identifier).ToList();

			File.WriteAllText(DatabasePath, serializer.Serialize(Database));
		}
	}

	internal class DatabaseModel
	{
		public List<string> DisabledSubclasses { get; set; } = new List<string>();
	}
}
