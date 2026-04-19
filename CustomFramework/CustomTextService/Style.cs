using UnityEngine;

namespace CustomFramework.CustomTextService
{
	public class Style
	{
		public static Style Default { get; } = new Style();

		public bool Italic { get; set; } = false;
		public bool Bold { get; set; } = false;
		public Alignment Alignment { get; set; } = Alignment.Center;
		public float? CharacterSpacing { get; set; } = null;
		public Color Color { get; set; } = Color.white;
		public float? FontWeight { get; set; } = null;
		public float? LineHeight { get; set; } = null;
		public float? LineIndent { get; set; } = null;
		public float? Margin { get; set; } = null;
		public Color? Mark { get; set; } = null;
		public bool Monospace { get; set; } = false;
		public bool Parse { get; set; } = true;
		public float? Position { get; set; } = null;
		public float Rotation { get; set; } = 0;
		public bool Strikethrough { get; set; } = false;
		public float Size { get; set; } = 20;
		public bool Subscript { get; set; } = false;
		public bool Superscript { get; set; } = false;
		public bool Underline { get; set; } = false;
		public float VerticalOffset { get; set; } = 0;
	}
}
