using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 0108

namespace ULTRAINTERFACE {
	[ComImport, Guid ( "B63EA76D-1F85-456F-A19C-48159EFA858B" ), InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
	internal interface IShellItemArray
	{
		// Not supported: IBindCtx
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void BindToHandler ( [In, MarshalAs ( UnmanagedType.Interface )] IntPtr pbc, [In] ref Guid rbhid,
					[In] ref Guid riid, out IntPtr ppvOut );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetPropertyStore ( [In] int Flags, [In] ref Guid riid, out IntPtr ppv );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetPropertyDescriptionList ( [In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetAttributes ( [In] SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetCount ( out uint pdwNumItems );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetItemAt ( [In] uint dwIndex, [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		// Not supported: IEnumShellItems (will use GetCount and GetItemAt instead)
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void EnumItems ( [MarshalAs ( UnmanagedType.Interface )] out IntPtr ppenumShellItems );
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
	internal interface IShellItem {
		void BindToHandler(IntPtr pbc,
			[MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
			[MarshalAs(UnmanagedType.LPStruct)]Guid riid,
			out IntPtr ppv);

		void GetParent(out IShellItem ppsi);

		void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

		void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

		void Compare(IShellItem psi, uint hint, out int piOrder);
	};

	[ComImport, Guid ( "973510DB-7D7F-452B-8975-74A85828D354" ), InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface IFileDialogEvents {
		// NOTE: some of these callbacks are cancelable - returning S_FALSE means that
		// the dialog should not proceed (e.g. with closing, changing folder); to
		// support this, we need to use the PreserveSig attribute to enable us to return
		// the proper HRESULT
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
		HRESULT OnFileOk ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
		HRESULT OnFolderChanging ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
					[In, MarshalAs ( UnmanagedType.Interface )] IShellItem psiFolder );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void OnFolderChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void OnSelectionChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void OnShareViolation ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
					[In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi,
					out FDE_SHAREVIOLATION_RESPONSE pResponse );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void OnTypeChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void OnOverwrite ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
				[In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi,
				out FDE_OVERWRITE_RESPONSE pResponse );
    }

	[ComImport, Guid ( "d57c7288-d4ad-4768-be02-9d969532d960" ), InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
	internal unsafe interface IFileOpenDialog : IFileDialog {
		// Defined on IModalWindow - repeated here due to requirements of COM interop layer
		// --------------------------------------------------------------------------------
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
		int Show ( [In] IntPtr parent );

		// Defined on IFileDialog - repeated here due to requirements of COM interop layer
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileTypes([In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec);

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileTypeIndex ( [In] uint iFileType );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFileTypeIndex ( out uint piFileType );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Advise ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialogEvents pfde, out uint pdwCookie );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Unadvise ( [In] uint dwCookie );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetOptions ( [In] FOS fos );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetOptions ( out FOS pfos );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, uint fdap );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetClientGuid ( [In] ref Guid guid );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void ClearClientData ( );

		// Not supported:  IShellItemFilter is not defined, converting to IntPtr
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );

		// Defined by IFileOpenDialog
		// ---------------------------------------------------------------------------------
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetResults ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppenum );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetSelectedItems ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppsai );
	}

	[ComImport, Guid ( "b4db1657-70d7-485e-8e3e-6fcb5a5c1802" ), InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
	internal interface IModalWindow
	{
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
		int Show ( [In] IntPtr parent );
	}

	[ComImport, Guid ( "42f85136-db7e-439c-85f1-e4075d135fc8" ), InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
	internal interface IFileDialog : IModalWindow {
		// Defined on IModalWindow - repeated here due to requirements of COM interop layer
		// --------------------------------------------------------------------------------
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
		int Show ( [In] IntPtr parent );

		// IFileDialog-Specific interface members
		// --------------------------------------------------------------------------------
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileTypes ( [In] uint cFileTypes,
					[In, MarshalAs ( UnmanagedType.LPArray )] COMDLG_FILTERSPEC[] rgFilterSpec );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileTypeIndex ( [In] uint iFileType );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFileTypeIndex ( out uint piFileType );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Advise ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialogEvents pfde, out uint pdwCookie );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Unadvise ( [In] uint dwCookie );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetOptions ( [In] FOS fos );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetOptions ( out FOS pfos );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, uint fdap );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetClientGuid ( [In] ref Guid guid );

		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void ClearClientData ( );

		// Not supported:  IShellItemFilter is not defined, converting to IntPtr
		[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
		void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );
	}
}