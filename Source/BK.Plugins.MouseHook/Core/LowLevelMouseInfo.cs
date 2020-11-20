using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Core
{
	internal readonly struct LowLevelMouseInfo
	{
		public readonly MouseHookType Type;
		public readonly MSLLHOOKSTRUCT HookStruct;

		public LowLevelMouseInfo(MouseHookType type, MSLLHOOKSTRUCT hookStruct)
		{
			Type = type;
			HookStruct = hookStruct;
		}
	}

	
}