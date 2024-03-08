using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Paroxe.PdfRenderer.Internal;
using System.IO;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Represents a PDF document. This class is the entry point of all functionalities.
    /// </summary>
    public sealed class PDFDocument : IDisposable, ICoordinatedNativeDisposable
    {
	    private IntPtr m_NativePointer;
        private GCHandle m_PinnedBytes;
        private byte[] m_DocumentBuffer;
        private bool m_ValidDocument;
        private PDFRenderer m_Renderer;
        private PDFBookmark m_RootBookmark;

        public static PDFJS_Promise<PDFDocument> LoadDocumentFromUrlAsync(string url)
        {
            PDFJS_Promise<PDFDocument> documentPromise = new PDFJS_Promise<PDFDocument>();

            PDFJS_Library.Instance.PreparePromiseCoroutine(LoadDocumentFromWWWCoroutine, documentPromise, url).Start();
            return documentPromise;
        }
        private static IEnumerator LoadDocumentFromWWWCoroutine(PDFJS_PromiseCoroutine promiseCoroutine, IPDFJS_Promise promise, object urlString)
        {
            PDFJS_Promise<PDFDocument> documentPromise = promise as PDFJS_Promise<PDFDocument>;

            PDFLibrary.Instance.EnsureInitialized();
            while (!PDFLibrary.Instance.IsInitialized)
                yield return null;

            string url = urlString as string;

            PDFWebRequest www = new PDFWebRequest(url);
            www.SendWebRequest();

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                documentPromise.HasFinished = true;
                documentPromise.HasSucceeded = true;
                documentPromise.HasReceivedJSResponse = true;
                documentPromise.Result = new PDFDocument(www.bytes);

                promiseCoroutine.ExecuteThenAction(true, documentPromise.Result);
            }
            else
            {
                documentPromise.HasFinished = true;
                documentPromise.HasSucceeded = false;

                promiseCoroutine.ExecuteThenAction(false, null);
            }

            www.Dispose();
            www = null;
        }

        public static PDFJS_Promise<PDFDocument> LoadDocumentFromBytesAsync(byte[] bytes)
        {
            PDFJS_Promise<PDFDocument> documentPromise = new PDFJS_Promise<PDFDocument>();

            documentPromise.HasFinished = true;
            documentPromise.HasSucceeded = true;
            documentPromise.HasReceivedJSResponse = true;
            documentPromise.Result = new PDFDocument(bytes);
            return documentPromise;
        }

		public PDFDocument(IntPtr nativePointer)
		{
			m_NativePointer = nativePointer;
			m_ValidDocument = true;
		}
		/// <summary>
		/// Open PDF Document with the specified byte array.
		/// </summary>
		/// <param name="buffer"></param>
		public PDFDocument(byte[] buffer)
            : this(buffer, "")
        { }

        /// <summary>
        /// Open PDF Document with the specified byte array.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="password">Can be null or empty</param>
        public PDFDocument(byte[] buffer, string password)
        {
	        CommonInit(buffer, password);
        }

        /// <summary>
        /// Open PDF Document located at the specified path
        /// </summary>
        /// <param name="filePath"></param>
        public PDFDocument(string filePath)
            : this(filePath, "")
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password">Can be null or empty</param>
        public PDFDocument(string filePath, string password)
        {
	        CommonInit(File.ReadAllBytes(filePath), password);
        }

	    ~PDFDocument()
        {
	        Close();
        }

	    public void Dispose()
	    {
		    Close();

		    GC.SuppressFinalize(this);
	    }

        private void Close()
        {
	        if (m_NativePointer == IntPtr.Zero && m_DocumentBuffer == null)
		        return;

	        if (m_NativePointer != IntPtr.Zero)
	        {
                PDFLibrary.Instance.DisposeCoordinator.RemoveReference(this);
                m_NativePointer = IntPtr.Zero;
	        }

	        if (m_DocumentBuffer != null)
	        {
		        m_PinnedBytes.Free();

		        m_DocumentBuffer = null;
	        }
        }

        /// <summary>
        /// Return a convenience PDFRenderer instance. 
        /// </summary>
        public PDFRenderer Renderer
        {
            get
            {
                if (m_Renderer == null)
                    m_Renderer = new PDFRenderer();
                return m_Renderer;
            }
        }

        /// <summary>
        /// The byte array of the document.
        /// </summary>
        public byte[] DocumentBuffer
        {
            get { return m_DocumentBuffer; }
        }

        /// <summary>
        /// Return if the document is valid. The document can be invalid if the password is invalid or if the
        /// document itseft is corrupted. See PDFLibrary.GetLastError.
        /// </summary>
        public bool IsValid
        {
            get { return m_ValidDocument; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

		public int GetPageCount()
        {
            return NativeMethods.FPDF_GetPageCount(m_NativePointer);
        }

        public Vector2 GetPageSize(int pageIndex)
        {
            double width;
            double height;

            NativeMethods.FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return new Vector2((float)width, (float)height);
        }

        public int GetPageWidth(int pageIndex)
        {
            double width;
            double height;

            NativeMethods.FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return (int)width;
        }

        public int GetPageHeight(int pageIndex)
        {
            double width;
            double height;

            NativeMethods.FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return (int)height;
        }

	    /// <summary>
	    /// Return the root bookmark of the document.
	    /// </summary>
	    /// <returns></returns>
        public PDFBookmark GetRootBookmark()
        {
            if (m_RootBookmark == null)
	            m_RootBookmark = new PDFBookmark(this, null, IntPtr.Zero);
            return m_RootBookmark;
        }

        public PDFPage GetPage(int index)
        {
	        return new PDFPage(this, index);
        }

        public PDFJS_Promise<PDFPage> GetPageAsync(int index)
        {
            return PDFPage.LoadPageAsync(this, index);
        }

        private void CommonInit(byte[] buffer, string password)
        {
            m_DocumentBuffer = buffer;

            if (m_DocumentBuffer != null)
            {

	            PDFLibrary.Instance.DisposeCoordinator.EnsureNativeLibraryIsInitialized();

                m_PinnedBytes = GCHandle.Alloc(m_DocumentBuffer, GCHandleType.Pinned);

                m_NativePointer = NativeMethods.FPDF_LoadMemDocument(m_PinnedBytes.AddrOfPinnedObject(), m_DocumentBuffer.Length, password);

                if (m_NativePointer != IntPtr.Zero)
					PDFLibrary.Instance.DisposeCoordinator.AddReference(this);

                m_ValidDocument = (m_NativePointer != IntPtr.Zero);
            }
            else
            {
                m_ValidDocument = false;
            }
        }

        IntPtr ICoordinatedNativeDisposable.NativePointer
        {
	        get { return m_NativePointer; }
        }

        ICoordinatedNativeDisposable ICoordinatedNativeDisposable.NativeParent
        {
	        get { return null; }
        }

        Action<IntPtr> ICoordinatedNativeDisposable.GetDisposeMethod()
		{
            return NativeMethods.FPDF_CloseDocument;
        }
	}
}