using Paroxe.PdfRenderer.Internal;
using System.Text;

namespace Paroxe.PdfRenderer
{
	public sealed class PDFLibrary
	{
		public enum ErrorCode
		{
			ErrSuccess = 0,     // No error.
			ErrUnknown = 1,     // Unknown error.
			ErrFile = 2,        // File not found or could not be opened.
			ErrFormat = 3,      // File not in PDF format or corrupted.
			ErrPassword = 4,    // Password required or incorrect password.
			ErrSecurity = 5,    // Unsupported security scheme.
			ErrPage = 6         // Page not found or content error.
		}

        public static readonly Encoding Encoding = new UnicodeEncoding(false, false, false);

		public static readonly object nativeLock = typeof(NativeMethods);

		private static PDFLibrary m_Instance;
        public const string PLUGIN_ASSEMBLY = "__Internal";
		
		private readonly NativeDisposeCoordinator m_DisposeCoordinator;

		public bool IsInitialized
		{
			get { return m_DisposeCoordinator.IsLibraryInitialized; }
		}

		public void EnsureInitialized()
		{
			Instance.DisposeCoordinator.EnsureNativeLibraryIsInitialized();
		}

		private PDFLibrary()
		{
			m_DisposeCoordinator = new NativeDisposeCoordinator();
		}

		public static ErrorCode GetLastError()
		{
			Instance.DisposeCoordinator.EnsureNativeLibraryIsInitialized();

			return (ErrorCode)NativeMethods.FPDF_GetLastError();
		}

		public static PDFLibrary Instance
		{
			get { return m_Instance ?? (m_Instance = new PDFLibrary()); }
		}

		public NativeDisposeCoordinator DisposeCoordinator
		{
			get { return m_DisposeCoordinator; }
		}
	}
}