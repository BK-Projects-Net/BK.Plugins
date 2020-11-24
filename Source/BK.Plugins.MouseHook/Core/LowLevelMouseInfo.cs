using BK.Plugins.PInvoke.Core;

namespace BK.Plugins.MouseHook.Core
{
	internal readonly struct LowLevelMouseInfo
	{
		public readonly MouseHookType Type;
		public readonly MSLLHOOKSTRUCT HookStruct;
		public readonly MouseParameter MouseParameter;

		public LowLevelMouseInfo(MouseHookType type, MSLLHOOKSTRUCT hookStruct, MouseParameter mouseParameter)
		{
			Type = type;
			HookStruct = hookStruct;
			MouseParameter = mouseParameter;
		}
	}

	
}