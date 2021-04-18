using MyHealthSolution.Service.Infrastructure.Services;
using Xunit;

namespace MyHealthSolution.Service.Infrastructure.UnitTests.Services
{
    public class EncryptionTests
    {
        private readonly EncryptionConfig _encryptionConfig;

        public EncryptionTests()
        {
            _encryptionConfig = new EncryptionConfig() { EncryptionKey = "CF46C8DF279CD5931069B500E695BAI9" };
        }

        [Fact]
        public void EncryptionTest_When_SRN_provided_decrypt_it()
        {
            // arrange
            var srn = "I0070003406";
            var encryptionService = new EncryptionService(_encryptionConfig);

            //act
            var encryptedResult = encryptionService.Encrypt(srn);
            var decryptedResult = encryptionService.Decrypt(encryptedResult);

            //asert
            decryptedResult.Equals(srn);
        }
    }
}