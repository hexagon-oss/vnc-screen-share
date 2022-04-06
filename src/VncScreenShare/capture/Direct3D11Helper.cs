using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

namespace VncScreenShare.Capture
{
	internal static class Direct3D11Helper
	{
		static readonly Guid ID3D11Device = new ("db6f6ddb-ac77-4e88-8253-819df9bbf140");
		static readonly Guid ID3D11Texture2D = new ("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

		[ComImport]
		[Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComVisible(true)]
		interface IDirect3DDxgiInterfaceAccess
		{
			IntPtr GetInterface([In] ref Guid iid);
		};


		[DllImport("d3d11")]
		private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr pUnk);

		public static IDirect3DDevice CreateDevice()
		{
			return CreateDevice(false);
		}

		public static IDirect3DDevice CreateDevice(bool useWARP)
		{
			var d3dDevice = new SharpDX.Direct3D11.Device(
				useWARP ? SharpDX.Direct3D.DriverType.Software : SharpDX.Direct3D.DriverType.Hardware,
				SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);
			var device = CreateDirect3DDeviceFromSharpDXDevice(d3dDevice);
			return device;
		}

		public static IDirect3DDevice CreateDirect3DDeviceFromSharpDXDevice(SharpDX.Direct3D11.Device d3dDevice)
		{
			IDirect3DDevice device = null;
			uint hr = CreateDirect3D11DeviceFromDXGIDevice(d3dDevice.NativePointer, out var punk);
			if (hr == 0)
			{
				return WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(punk);
			}
			return device;
		}
		
		public static SharpDX.Direct3D11.Device CreateSharpDXDevice(IDirect3DDevice device)
		{
			var access = device.As<IDirect3DDxgiInterfaceAccess>();
			var d3dPointer = access.GetInterface(ID3D11Device);
			var d3dDevice = new SharpDX.Direct3D11.Device(d3dPointer);
			return d3dDevice;
		}

		public static SharpDX.Direct3D11.Texture2D CreateSharpDXTexture2D(IDirect3DSurface surface)
		{
			var access = surface.As<IDirect3DDxgiInterfaceAccess>();
			var d3dPointer = access.GetInterface(ID3D11Texture2D);
			var d3dSurface = new SharpDX.Direct3D11.Texture2D(d3dPointer);
			return d3dSurface;
		}
	}
}
