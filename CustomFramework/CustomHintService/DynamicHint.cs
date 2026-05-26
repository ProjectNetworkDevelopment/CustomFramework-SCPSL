using CustomFramework.CustomTextService;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomFramework.CustomHintService
{
	public class DynamicHint
	{
		public DynamicHint(Style style, Func<Player, string> method)
		{
			Style = style;
			HintMethod = method;
		}

		public Style Style { get; set; }
		public TimeSpan UpdateTime { get; set; }
		public Func<Player, string> HintMethod { get; set; }
		internal Dictionary<Player, string> hints = new Dictionary<Player, string>();

		private string GetSytles(out string end)
		{
			string str = "";
			end = "";
			if (!Style.Parse) end += "</noparse>";
			if (Style.Monospace)
			{
				str += "<mspace>";
				end += "</mspace";
			}
			if (Style.Italic)
			{
				str += "<i>";
				end += "</i>";
			}
			if (Style.Bold)
			{
				str += "<b>";
				end += "</b>";
			}
			if (Style.Strikethrough)
			{
				str += "<strikethrough>";
				end += "</strikethrough>";
			}
			if (Style.Underline)
			{
				str += "<u>";
				end += "</u>";
			}
			if (Style.Subscript)
			{
				str += "<sub>";
				end += "</sub>";
			}
			if (Style.Superscript)
			{
				str += "<sup>";
				end += "</sup>";
			}
			if (Style.CharacterSpacing != null)
			{
				str += $"<cspace={Style.CharacterSpacing}>";
				end += "</cspace>";
			}
			if (Style.FontWeight != null)
			{
				str += $"<font-weight={Style.FontWeight.Value}";
				end += "</font-weight>";
			}
			if (Style.LineHeight != null)
			{
				str += $"<line-height={Style.LineHeight.Value}";
				end += "</line-height>";
			}
			if (Style.LineIndent != null)
			{
				str += $"<line-indent={Style.LineIndent.Value}";
				end += "</line-indent>";
			}
			if (Style.Margin != null)
			{
				str += $"<margin={Style.Margin.Value}>";
				end += "</margin>";
			}
			if (Style.Mark != null)
			{
				byte mcolorR = BitConverter.GetBytes(Style.Mark.Value.r).ElementAt(0);
				byte mcolorG = BitConverter.GetBytes(Style.Mark.Value.g).ElementAt(0);
				byte mcolorB = BitConverter.GetBytes(Style.Mark.Value.b).ElementAt(0);
				str += $"<mark=#{mcolorR:X2}{mcolorG:X2}{mcolorB:X2}>";
				end += "</mark>";
			}
			if (Style.Position != null)
			{
				str += $"<pos={Style.Position.Value}>";
				end += "</pos>";
			}
			if (Style.Rotation != 0)
			{
				str += $"<rotate={Style.Rotation}>";
				end += "</rotate>";
			}
			if (Style.VerticalOffset != 0)
			{
				str += $"<voffset={Style.VerticalOffset}";
				end += "</voffset>";
			}

			//byte colorR = BitConverter.GetBytes(Style.Color.r).ElementAt(0);
			//byte colorG = BitConverter.GetBytes(Style.Color.g).ElementAt(0);
			//byte colorB = BitConverter.GetBytes(Style.Color.b).ElementAt(0);
			//byte alpha = BitConverter.GetBytes(Style.Color.a).ElementAt(0);
			//str += $"<color=#{colorR:X2}{colorG:X2}{colorB:X2}><size={Style.Size}>";
			//end += "</color></size>";
			str += $"<size={Style.Size}>";
			end += "</size>";

			if (!Style.Parse) str += "<noparse>";
			return str;
		}

		public void Update()
		{
			var stylebegin = GetSytles(out var styleend);
			foreach (var player in Player.ReadyList)
			{
				hints[player] = stylebegin + HintMethod(player) + styleend;
			}
		}

		public void Update(Player player)
		{
			var stylebegin = GetSytles(out var styleend);
			hints[player] = stylebegin + HintMethod(player) + styleend;
		}
	}
}
