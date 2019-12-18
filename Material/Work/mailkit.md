#### `MailKit` 发送邮件问题

1. 问题： 乱码,Excel文件变为xxx.dat文件,文件名过长出现截断问题

```
private const string KitCharset = "GB18030";

#region Creat Email Message
var message = new MimeMessage();
message.From.Add(new MailboxAddress(from.DisplayName, from.Address));
foreach(var toitem in to)
{
    message.To.Add(new MailboxAddress(toitem.DisplayName, toitem.Address));
}
message.Subject = subject;
if (null != bcc)
{
    foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
    {
        message.Bcc.Add(new MailboxAddress(address.Trim()));
    }
}
if (null != cc)
{
    foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
    {
        message.Cc.Add(new MailboxAddress(address.Trim()));
    }
}
#endregion

#region Email message
var multipart = new Multipart("mixed");
multipart.Add(new TextPart(TextFormat.Html)
{
    Text = body
});
//attaches
if (attaches != null && attaches.Count > 0)
{
    for (int i = 0; i < attaches.Count; i++)
    {
        var attachment = new MimePart()
        {
            Content = new MimeContent(attaches[i].stream),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64  //乱码
        };                    
        var fileName = attaches[i].FileName;
        attachment.ContentType.Parameters.Add(KitCharset, "name", fileName); //乱码
        attachment.ContentDisposition.Parameters.Add(KitCharset, "filename", fileName);//乱码
        foreach (var param in attachment.ContentDisposition.Parameters)
            param.EncodingMethod = ParameterEncodingMethod.Rfc2047; //文件名过长
        foreach (var param in attachment.ContentType.Parameters)
            param.EncodingMethod = ParameterEncodingMethod.Rfc2047; //文件名过长
        multipart.Add(attachment);
    }
}
message.Body = multipart;
#endregion

#region Send Email
using (var client = new MailKit.Net.Smtp.SmtpClient())
{
    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
    client.Connect(SmtpConfig.Instance.Host, SmtpConfig.Instance.Port, SmtpConfig.Instance.EnableSsl);
    if (!string.IsNullOrEmpty(SmtpConfig.Instance.UserName))
    {
        client.Authenticate(SmtpConfig.Instance.UserName, SmtpConfig.Instance.Password);
    }                
    client.Send(message);
    client.Disconnect(true);
}
#endregion
```
