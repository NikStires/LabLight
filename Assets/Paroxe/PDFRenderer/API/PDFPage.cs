using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections.Generic;
using Paroxe.PdfRenderer.Internal;
using UnityEngine;
using System.Collections; // For WebGL

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Represents a PDF page inside document.
    /// </summary>
    public sealed class PDFPage : IDisposable, IEquatable<PDFPage>, ICoordinatedNativeDisposable
    {
        private IntPtr m_NativePointer;
        private PDFDocument m_Document;
        private int m_PageIndex;

        /// <summary>
        /// Rotations are clockwise
        /// </summary>
        public enum PageRotation
        {
            Normal = 0,
            Rotate90 = 1,
            Rotate180 = 2,
            Rotate270 = 3
        };

        public static PDFJS_Promise<PDFPage> LoadPageAsync(PDFDocument document, int pageIndex)
        {
            PDFJS_Promise<PDFPage> pagePromise = new PDFJS_Promise<PDFPage>();

            pagePromise.HasFinished = true;
            pagePromise.HasSucceeded = true;
            pagePromise.HasReceivedJSResponse = true;
            pagePromise.Result = document.GetPage(pageIndex);

            return pagePromise;
        }

        public PDFPage(PDFDocument document, int pageIndex)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex");

            m_Document = document;
            m_PageIndex = pageIndex;

            PDFLibrary.Instance.DisposeCoordinator.EnsureNativeLibraryIsInitialized();

            lock (PDFLibrary.nativeLock)
            {
	            m_NativePointer = NativeMethods.FPDF_LoadPage(document.NativePointer, m_PageIndex);

	            if (m_NativePointer != IntPtr.Zero)
						PDFLibrary.Instance.DisposeCoordinator.AddReference(this);
            }
        }

        ~PDFPage()
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
	        if (m_NativePointer == IntPtr.Zero)
		        return;

            PDFLibrary.Instance.DisposeCoordinator.RemoveReference(this);
            m_NativePointer = IntPtr.Zero;
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        public PDFDocument Document
        {
            get { return m_Document; }
        }

        public int PageIndex
        {
            get { return m_PageIndex; }
        }

        public Vector2 GetPageSize(float scale = 1.0f)
        {
            return m_Document.GetPageSize(m_PageIndex) * scale;
        }

        /// <summary>
        /// Return an instance of PDFTextPage that give access the the current page text content
        /// </summary>
        /// <returns></returns>
        public PDFTextPage GetTextPage()
        {
	        return new PDFTextPage(this);
        }

        public PDFLink GetLinkAtPoint(Vector2 pagePoint)
        {
            IntPtr linkPtr = NativeMethods.FPDFLink_GetLinkAtPoint(m_NativePointer, pagePoint.x, pagePoint.y);
            if (linkPtr != IntPtr.Zero)
                return new PDFLink(this, linkPtr);
            return null;
        }

        public PDFLink GetLinkAtPoint(double pageX, double pageY)
        {
            IntPtr linkPtr = NativeMethods.FPDFLink_GetLinkAtPoint(m_NativePointer, pageX, pageY);
            if (linkPtr != IntPtr.Zero)
                return new PDFLink(this, linkPtr);
            return null;
        }

        public Vector2 DeviceToPage(int startX, int startY, int sizeX, int sizeY, PageRotation rotation, int deviceX, int deviceY)
        {
            double pageX;
            double pageY;
            NativeMethods.FPDF_DeviceToPage(m_NativePointer, startX, startY, sizeX, sizeY, (int)rotation, deviceX, deviceY, out pageX,
                out pageY);

            return new Vector2((float)pageX, (float)pageY);
        }

        public Vector2 PageToDevice(int startX, int startY, int sizeX, int sizeY, PageRotation rotation, int pageX, int pageY)
        {
            int deviceX;
            int deviceY;
            NativeMethods.FPDF_PageToDevice(m_NativePointer, startX, startY, sizeX, sizeY, (int)rotation, pageX, pageY, out deviceX,
                out deviceY);

            return new Vector2(deviceX, deviceY);
        }

        public Vector2 ConvertPagePositionToUnityUIDevicePosition(Vector2 pagePoint, Vector2 devicePageSize)
        {
            pagePoint = pagePoint.x / (devicePageSize.y / devicePageSize.x) * Vector2.right + pagePoint.y * Vector2.up;

            int device_x;
            int device_y;

            NativeMethods.FPDF_PageToDevice(m_NativePointer, 0, 0, (int)devicePageSize.y, (int)devicePageSize.y, 0, pagePoint.x,
                pagePoint.y, out device_x, out device_y);

            return new Vector2(device_x, device_y);
        }

        public Rect ConvertPageRectToDeviceRect(Rect pageRect, Vector2 devicePageSize)
        {
            Vector2 min = ConvertPagePositionToUnityUIDevicePosition(pageRect.min, devicePageSize);
            float mx = pageRect.max.x;
            float my = (pageRect.min - (pageRect.max - pageRect.min)).y;
            Vector2 max = ConvertPagePositionToUnityUIDevicePosition(new Vector2(mx, my), devicePageSize);
            Rect rect = new Rect();
            rect.min = min;
            rect.max = max;
            return rect;
        }

        public Vector2 ConvertUnityUIDevicePositionToPagePosition(Vector2 devicePoint, Vector2 devicePageSize)
        {
            devicePoint = devicePoint.x * (devicePageSize.y / devicePageSize.x) * Vector2.right + devicePoint.y * Vector2.up;

            double page_x;
            double page_y;

            NativeMethods.FPDF_DeviceToPage(m_NativePointer, 0, 0, (int)devicePageSize.y, (int)devicePageSize.y, 0,
                (int)devicePoint.x, (int)devicePoint.y, out page_x, out page_y);

            return new Vector2((float)page_x, (float)page_y);
        }

        public bool Equals(PDFPage other)
        {
	        if (other == null)
		        return false;

            return (m_NativePointer != IntPtr.Zero && m_NativePointer == other.m_NativePointer);
        }

        IntPtr ICoordinatedNativeDisposable.NativePointer
        {
	        get { return NativePointer; }
        }

        ICoordinatedNativeDisposable ICoordinatedNativeDisposable.NativeParent
        {
	        get { return m_Document; }
        }

        Action<IntPtr> ICoordinatedNativeDisposable.GetDisposeMethod()
		{
            return NativeMethods.FPDF_ClosePage;
        }
    }
}