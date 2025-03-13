using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _email;
    private readonly string _password;
    private readonly string _imapServer;
    private readonly int _imapPort;
    private readonly string _smtpServer;
    private readonly int _smtpPort;

    public EmailService(IConfiguration config)
    {
        var settings = config.GetSection("EmailSettings");
        _email = settings["Email"];
        _password = settings["AppPassword"];
        _imapServer = settings["ImapServer"];
        _imapPort = int.Parse(settings["ImapPort"]);
        _smtpServer = settings["SmtpServer"];
        _smtpPort = int.Parse(settings["SmtpPort"]);

        Console.WriteLine($"Email: {_email}");
        Console.WriteLine($"AppPassword: {_password ?? "NULL"}");
    }

    public async Task CheckAndReplyEmailsAsync()
    {
        using var client = new ImapClient();
        await client.ConnectAsync(_imapServer, _imapPort, true);
        await client.AuthenticateAsync(_email, _password);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);

        var unreadMessages = await inbox.SearchAsync(SearchQuery.NotSeen);

        foreach (var uid in unreadMessages)
        {
            var message = await inbox.GetMessageAsync(uid);
            Console.WriteLine($"Nowa wiadomość od: {message.From}");

            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            Console.WriteLine($"Looking for config file at: {path}");

            if (!File.Exists(path))
            {
                Console.WriteLine("ERROR: appsettings.json not found!");
            }

            var reply = new MimeMessage();
            reply.From.Add(new MailboxAddress("AutoResponder", _email));
            reply.To.AddRange(message.From);
            reply.Subject = "Re: " + message.Subject;
            reply.Body = new TextPart("plain")
            {
                Text = "Dziękuję za wiadomość! Odpowiem tak szybko, jak to możliwe."
            };

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_smtpServer, _smtpPort, false);
            await smtpClient.AuthenticateAsync(_email, _password);
            await smtpClient.SendAsync(reply);
            await smtpClient.DisconnectAsync(true);

            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
        }

        await client.DisconnectAsync(true);
    }
}
