using CustomFramework.CustomSubclasses;

namespace CustomFramework.Interfaces
{
	public interface IEscapeSubclass<T> where T : CustomSubclass
	{
		T EscapeSubclass { get; set; }
	}
}
