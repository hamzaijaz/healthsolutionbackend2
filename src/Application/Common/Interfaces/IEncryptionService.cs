using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string input);
        string Decrypt(string cipherText);
    }
}
