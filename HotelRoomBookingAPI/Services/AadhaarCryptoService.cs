using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace HotelRoomBookingAPI.Services;

public interface IAadhaarCryptoService
{
    byte[] Encrypt(string aadhaarNumber);
    string Decrypt(byte[] encryptedData);
    byte[] Hash(string aadhaarNumber);
    string Mask(string aadhaarNumber);
}

public class AadhaarCryptoService : IAadhaarCryptoService
{
    private readonly IDataProtector _protector;

    public AadhaarCryptoService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("HotelRoomBookingAPI.Services.AadhaarCryptoService");
    }

    public byte[] Encrypt(string aadhaarNumber)
    {
        if (string.IsNullOrEmpty(aadhaarNumber))
            throw new ArgumentNullException(nameof(aadhaarNumber));

        var plainBytes = Encoding.UTF8.GetBytes(aadhaarNumber);
        return _protector.Protect(plainBytes);
    }

    public string Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            throw new ArgumentNullException(nameof(encryptedData));

        var plainBytes = _protector.Unprotect(encryptedData);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] Hash(string aadhaarNumber)
    {
        if (string.IsNullOrEmpty(aadhaarNumber))
            throw new ArgumentNullException(nameof(aadhaarNumber));

        return SHA256.HashData(Encoding.UTF8.GetBytes(aadhaarNumber));
    }

    public string Mask(string aadhaarNumber)
    {
        if (string.IsNullOrEmpty(aadhaarNumber) || aadhaarNumber.Length < 4)
            return "XXXX-XXXX-XXXX";

        // Assuming 12 digit aadhaar roughly. Format: XXXX-XXXX-1234
        var last4 = aadhaarNumber.Substring(aadhaarNumber.Length - 4);
        return $"XXXX-XXXX-{last4}";
    }
}
