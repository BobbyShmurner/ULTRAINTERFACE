using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ULTRAINTERFACE {
	[StructLayout ( LayoutKind.Sequential, Pack = 4 )]
	internal struct PROPERTYKEY
	{
		public Guid fmtid;
		public uint pid;
	}

	[ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
	internal class FileOpenDialog
	{
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MULTI_QI {
		[MarshalAs(UnmanagedType.LPStruct)]  public Guid pIID;
		[MarshalAs(UnmanagedType.Interface)] public object pItf;
		public int hr;
	}

	[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4 )]
	internal struct COMDLG_FILTERSPEC
	{
		[MarshalAs ( UnmanagedType.LPWStr )]
		public string pszName;

		[MarshalAs ( UnmanagedType.LPWStr )]
		public string pszSpec;
	}
}