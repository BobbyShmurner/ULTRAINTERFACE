using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ULTRAINTERFACE {
	internal static class UnmanagedFunctions {
    	/// <returns>If function succeeds, it returns 0(S_OK). Otherwise, it returns an error code.</returns>
    	[DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    	internal static extern int CoInitializeEx(
        	[In, Optional] IntPtr pvReserved,
			[In] COINIT dwCoInit //DWORD
        );

		[DllImport( "ole32.dll" )] 
		internal static extern uint CoCreateInstance(
			[ In ] ref Guid rclsid,
			[ MarshalAs( UnmanagedType.IUnknown )] Object pUnkOuter,
			int dwClsContext,
			[ In ] ref Guid riid,
			[ MarshalAs( UnmanagedType.IUnknown )] out Object ppv
		);

		[DllImport("ole32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int CoUninitialize();
	}
}