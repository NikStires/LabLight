using System;
using System.Threading.Tasks;

/// <summary>
/// Interface for uploading of files to server
/// </summary>
public interface IFileUploadHandler
{
    public Task UploadFile(string filePath);

    public Task UploadMediaFiles(string protocolName, DateTime start, DateTime end);
}