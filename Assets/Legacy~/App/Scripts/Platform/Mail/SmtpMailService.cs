using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple mail service that sends emails using SMTP
/// This service should have no knowledge of the contents of the email.
/// </summary>
public class SmtpMailService : IMailService
{
    private SmtpClient client;
    private MailMessage formSent;

    /// <summary>
    /// Constructor
    /// </summary>
    public SmtpMailService(ICredentialsByHost credentials)
    {
        client = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = credentials,
            EnableSsl = true,
        };
        formSent = new MailMessage();
        formSent.From = new MailAddress("lablightarCSV@gmail.com", "Lab Light AR");
    }

    private void OnDestroy()
    {
        client.Dispose();
    }

    public void SendMessage(List<string> recipients, string subject, string body, string attachmentName, string attachmentFilePath)
    {
        if (recipients == null || recipients.Count == 0)
        {
            Debug.LogWarning("no email recipiants passed in");
            return;
        }
        else
        {
            formSent.Subject = subject;
            formSent.Body = body;

            if (!string.IsNullOrEmpty(attachmentName) && !string.IsNullOrEmpty(attachmentFilePath))
            {
                Debug.Log("Attaching file");

                var attachment = new System.Net.Mail.Attachment(attachmentFilePath);
                attachment.Name = attachmentName;
                formSent.Attachments.Add(attachment); //Create & name attachment
            }

            foreach (string recipient in recipients)
            {
                formSent.To.Clear();
                formSent.To.Add(new MailAddress(recipient)); //Add recepients to email
                Debug.Log(formSent.To);
                try
                {
                    client.Send(formSent);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    Debug.Log("email sent");
                }
            }
        }
    }
}
