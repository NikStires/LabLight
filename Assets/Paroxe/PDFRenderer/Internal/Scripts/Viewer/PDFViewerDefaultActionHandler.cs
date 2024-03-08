using UnityEngine;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    class PDFViewerDefaultActionHandler : IPDFDeviceActionHandler
    {
        public void HandleGotoAction(IPDFDevice device, int pageIndex)
        {
            device.GoToPage(pageIndex);
        }

        public void HandleLaunchAction(IPDFDevice device, string filePath)
        {
            if (filePath.Trim().Substring(filePath.Length - 4).ToLower().Contains("pdf"))
            {
                device.LoadDocumentFromFile(filePath, "", 0);
            }
        }

        public string HandleRemoteGotoActionPasswordResolving(IPDFDevice device, string resolvedFilePath)
        {
            return "";
        }

        public string HandleRemoteGotoActionPathResolving(IPDFDevice device, string filePath)
        {
            return filePath;
        }

        public void HandleRemoteGotoActionResolved(IPDFDevice device, PDFDocument document, int pageIndex)
        {
            device.LoadDocument(document, "", pageIndex);
        }

        public void HandleRemoteGotoActionUnresolved(IPDFDevice device, string resolvedFilePath)
        {
            // ...
        }

        public void HandleUnsupportedAction(IPDFDevice device)
        {
            // ...
        }

        public void HandleUriAction(IPDFDevice device, string uri)
        {
            if (uri.Trim().Substring(uri.Length - 4).ToLower().Contains("pdf"))
            {
                device.LoadDocumentFromWeb(uri, "", 0);
            }
            else if (device.AllowOpenURL)
            {
                if (uri.Trim().ToLowerInvariant().StartsWith("http:")
                    || uri.Trim().ToLowerInvariant().StartsWith("https:")
                    || uri.Trim().ToLowerInvariant().StartsWith("ftp:"))
                {
                    Application.OpenURL(uri);
                }
            }
        }
    }
}