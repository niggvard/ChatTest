using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static readonly string serverUrl = "http://localhost:5000";
    static byte[] key;
    static async Task Main()
    {
        Console.Write("parol: ");
        var pass = Console.ReadLine();
        key = DeriveKey(pass);

        Console.WriteLine("Type /send to send or /read X to read messages from last X days");
        while (true)
        {
            try
            {
                var input = Console.ReadLine();
                if (input.StartsWith("/send"))
                {
                    var msg = input.Replace("/send", "");
                    var encrypted = Encrypt(msg, key);
                    await SendAsync(encrypted);
                }
                else if (input.StartsWith("/read"))
                {
                    var parts = input.Split(' ');
                    int days = parts.Length > 1 && int.TryParse(parts[1], out var d) ? d : 1;
                    await ReadAsync(days);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    static byte[] DeriveKey(string password)
    {
        var salt = Encoding.UTF8.GetBytes("static_salt_here");
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(32);
    }

    static string Encrypt(string plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        byte[] combined = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, combined, aes.IV.Length, encrypted.Length);

        return Convert.ToBase64String(combined);
    }

    static string Decrypt(string cipherText, byte[] key)
    {
        byte[] combined = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] iv = new byte[16];
        byte[] ciphertext = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        Buffer.BlockCopy(combined, 16, ciphertext, 0, ciphertext.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] decrypted = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    static async Task SendAsync(string encryptedText)
    {
        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(new { text = encryptedText });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"{serverUrl}/send", content);
        Console.WriteLine(resp.IsSuccessStatusCode ? "Sent" : "Failed to send");
    }

    static async Task ReadAsync(int days)
    {
        using var client = new HttpClient();
        var resp = await client.GetAsync($"{serverUrl}/messages?days={days}");
        var body = await resp.Content.ReadAsStringAsync();

        var messages = JsonSerializer.Deserialize<Message[]>(body);
        foreach (var m in messages)
        {
            try
            {
                var text = Decrypt(m.text, key);
                Console.WriteLine($"[{m.timestamp}] {text}");
            }
            catch
            {
                Console.WriteLine($"[{m.timestamp}] ❌ Could not decrypt");
            }
        }
    }

    public class Message
    {
        public string text { get; set; }
        public string timestamp { get; set; }
    }
}