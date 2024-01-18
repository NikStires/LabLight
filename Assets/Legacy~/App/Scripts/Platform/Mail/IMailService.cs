using System.Collections.Generic;

public interface IMailService
{
    public void SendMessage(List<string> recipients, string subject, string body, string attachmentName, string attachmentFilePath);
}