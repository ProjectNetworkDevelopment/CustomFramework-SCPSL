using CustomFramework.CustomTextService;
using System;

namespace CustomFramework.CustomHintService
{
	public class StaticHint
	{
		public StaticHint(Style style, string text, TimeSpan? expiration)
		{
			Style = style;
			Hint = text;
			Expiration = expiration;
			StartTime = DateTime.UtcNow;
		}

		public Style Style { get; set; }
		public string Hint { get; set; }
		internal DateTime StartTime { get; set; }
		public TimeSpan? Expiration { get; set; }
	}
}
