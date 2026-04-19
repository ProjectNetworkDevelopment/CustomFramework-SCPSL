using CustomFramework.CustomSubclasses;
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

		internal static void LoadDatabase()
		{
			var deserializer = new DeserializerBuilder().Build();

			var filePath = Path.Combine(PathManager.Configs.FullName, Server.Port.ToString(), "Custom Framework", "Database.yml");
			if (!File.Exists(filePath))
			{
				Database = new DatabaseModel();
				SaveDatabase();
				return;
			}

			Database = deserializer.Deserialize<DatabaseModel>(File.ReadAllText(filePath));

			foreach (var id in Database.DisabledSubclasses)
			{
				var sc = CustomSubclass.Get(id);
				if (sc != null)
					CustomSubclass.Disabled.Add(sc);
			}
		}

		public static void SaveDatabase()
		{
			var serializer = new SerializerBuilder().Build();

			var filePath = Path.Combine(PathManager.Configs.FullName, Server.Port.ToString(), "Custom Framework", "Database.yml");

			Database.DisabledSubclasses = CustomSubclass.Disabled.Select(x => x.Identifier).ToList();

			File.WriteAllText(filePath, serializer.Serialize(Database));
		}
	}

	internal class DatabaseModel
	{
		public List<string> DisabledSubclasses { get; set; } = new List<string>();
	}
}
