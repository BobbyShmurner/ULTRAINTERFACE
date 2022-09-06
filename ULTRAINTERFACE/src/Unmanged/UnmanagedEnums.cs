using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ULTRAINTERFACE {
	internal enum FDE_OVERWRITE_RESPONSE
	{
		FDEOR_DEFAULT = 0x00000000,
		FDEOR_ACCEPT = 0x00000001,
		FDEOR_REFUSE = 0x00000002
	}

	internal enum SIATTRIBFLAGS
	{
		SIATTRIBFLAGS_AND = 1,
		SIATTRIBFLAGS_APPCOMPAT = 3,
		SIATTRIBFLAGS_OR = 2
	}

	[Flags]
	internal enum FOS : uint
	{
		FOS_OVERWRITEPROMPT = 0x00000002,
		FOS_STRICTFILETYPES = 0x00000004,
		FOS_NOCHANGEDIR = 0x00000008,
		FOS_PICKFOLDERS = 0x00000020,
		FOS_FORCEFILESYSTEM = 0x00000040, // Ensure that items returned are filesystem items.
		FOS_ALLNONSTORAGEITEMS = 0x00000080, // Allow choosing items that have no storage.
		FOS_NOVALIDATE = 0x00000100,
		FOS_ALLOWMULTISELECT = 0x00000200,
		FOS_PATHMUSTEXIST = 0x00000800,
		FOS_FILEMUSTEXIST = 0x00001000,
		FOS_CREATEPROMPT = 0x00002000,
		FOS_SHAREAWARE = 0x00004000,
		FOS_NOREADONLYRETURN = 0x00008000,
		FOS_NOTESTFILECREATE = 0x00010000,
		FOS_HIDEMRUPLACES = 0x00020000,
		FOS_HIDEPINNEDPLACES = 0x00040000,
		FOS_NODEREFERENCELINKS = 0x00100000,
		FOS_DONTADDTORECENT = 0x02000000,
		FOS_FORCESHOWHIDDEN = 0x10000000,
		FOS_DEFAULTNOMINIMODE = 0x20000000
	}

	internal enum FDE_SHAREVIOLATION_RESPONSE
	{
		FDESVR_DEFAULT = 0x00000000,
		FDESVR_ACCEPT = 0x00000001,
		FDESVR_REFUSE = 0x00000002
	}

	internal enum SIGDN : uint {
		NORMALDISPLAY = 0,
		PARENTRELATIVEPARSING = 0x80018001,
		PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
		DESKTOPABSOLUTEPARSING = 0x80028000,
		PARENTRELATIVEEDITING = 0x80031001,
		DESKTOPABSOLUTEEDITING = 0x8004c000,
		FILESYSPATH = 0x80058000,
		URL = 0x80068000,
		/// <summary>
		/// Returns the path relative to the parent folder.
		/// </summary>
		PARENTRELATIVE = 0x80080001,
		/// <summary>
		/// Introduced in Windows 8.
		/// </summary>
		PARENTRELATIVEFORUI = 0x80094001
	}

	internal enum COINIT : uint {
        COINIT_MULTITHREADED 		= 0x0, //Initializes the thread for multi-threaded object concurrency.
        COINIT_APARTMENTTHREADED 	= 0x2, //Initializes the thread for apartment-threaded object concurrency
        COINIT_DISABLE_OLE1DDE 		= 0x4, //Disables DDE for OLE1 support
        COINIT_SPEED_OVER_MEMORY 	= 0x8, //Trade memory for speed
    }

	[Flags]
	internal enum CLSCTX : uint {
		CLSCTX_INPROC_SERVER      		= 0x1,
		CLSCTX_INPROC_HANDLER     		= 0x2,
		CLSCTX_LOCAL_SERVER       		= 0x4,
		CLSCTX_INPROC_SERVER16    		= 0x8,
		CLSCTX_REMOTE_SERVER      		= 0x10,
		CLSCTX_INPROC_HANDLER16   		= 0x20,
		CLSCTX_RESERVED1          		= 0x40,
		CLSCTX_RESERVED2          		= 0x80,
		CLSCTX_RESERVED3          		= 0x100,
		CLSCTX_RESERVED4          		= 0x200,
		CLSCTX_NO_CODE_DOWNLOAD   		= 0x400,
		CLSCTX_RESERVED5          		= 0x800,
		CLSCTX_NO_CUSTOM_MARSHAL  		= 0x1000,
		CLSCTX_ENABLE_CODE_DOWNLOAD		= 0x2000,
		CLSCTX_NO_FAILURE_LOG     		= 0x4000,
		CLSCTX_DISABLE_AAA        		= 0x8000,
		CLSCTX_ENABLE_AAA         		= 0x10000,
		CLSCTX_FROM_DEFAULT_CONTEXT   	= 0x20000,
		CLSCTX_ACTIVATE_32_BIT_SERVER 	= 0x40000,
		CLSCTX_ACTIVATE_64_BIT_SERVER 	= 0x80000,
		CLSCTX_INPROC        			= CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
		CLSCTX_SERVER        			= CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
		CLSCTX_ALL           			= CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
	}

	internal enum HRESULT : uint {
		S_FALSE = 0x0001,
		S_OK = 0x0000,
		E_INVALIDARG = 0x80070057,
		E_OUTOFMEMORY = 0x8007000E
	}
}