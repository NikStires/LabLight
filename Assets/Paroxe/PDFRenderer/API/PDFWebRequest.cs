#define USE_UNITYWEBREQUEST
using UnityEngine.Networking;

using System;
using System.Collections;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// WWW is deprecated in recent version of Unity but it's not available on older version like Unity 5.3
    /// This class only acts like a shim supporting the right implementation depending on which version of 
    /// Unity being use.
    /// </summary>
    public sealed class PDFWebRequest : UnityWebRequest, IEnumerator
    {
        public PDFWebRequest(string url) : base(url)
        {
            downloadHandler = new DownloadHandlerBuffer();
            disposeDownloadHandlerOnDispose = true;
        }

        public float progress
        {
            get { return downloadProgress; }
        }

        public byte[] bytes
        {
            get { return downloadHandler.data; }
        }

        object IEnumerator.Current
        {
            get { return null; }
        }

        bool IEnumerator.MoveNext()
        {
            return !isDone;
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }
    }
}
