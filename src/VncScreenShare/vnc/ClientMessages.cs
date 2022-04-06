namespace VncScreenShare.Vnc
{
	public enum ClientMessages : byte
	{
		SetPixelFormat = 0,
		ReadColorMapEntries = 1,
		SetEncodings = 2,
		FramebufferUpdateRequest = 3,
		KeyEvent = 4,
		PointerEvent = 5,
		ClientCutText = 6,
	}
}
