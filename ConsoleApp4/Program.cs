
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
class Program
{
    static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        Console.WriteLine("Email: " + config["EmailSettings:Email"]);
        Console.WriteLine("IMAP Server: " + config["EmailSettings:ImapServer"]);
        Console.WriteLine("SMTP Server: " + config["EmailSettings:SmtpServer"]);
        Console.WriteLine(File.ReadAllText("appsettings.json"));

        var emailService = new EmailService(config);

        while (true)
        {
            Console.WriteLine("Sprawdzanie nowych maili...");
            await emailService.CheckAndReplyEmailsAsync();
            Console.WriteLine("Oczekiwanie 60 sekund...");
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }
}
