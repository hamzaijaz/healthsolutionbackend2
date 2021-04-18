using System;
using MyHealthSolution.Service.Application.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace MyHealthSolution.Service.Application.UnitTests.Common.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void When_Generating_CheckDigit_Mod10_EnsureResult_IsValid()
        {
            var result = StringExtensions.AddCheckDigit("12345678", CheckDigitRule.MOD10v5);
            
            result.Should().NotBeNullOrWhiteSpace();
            result.Length.Should().Be(9);
            result.Should().Be("123456782");
        }

        [Fact]
        public void When_Generating_CheckDigit_Mod11_EnsureResult_IsValid()
        {
            var result = StringExtensions.AddCheckDigit("12345678901234567", CheckDigitRule.MOD11v3);
            
            result.Should().NotBeNullOrWhiteSpace();
            result.Length.Should().Be(18);
            result.Should().Be("123456789012345673");

            result = StringExtensions.AddCheckDigit("83746592341234567", CheckDigitRule.MOD11v3);
            
            result.Should().NotBeNullOrWhiteSpace();
            result.Length.Should().Be(18);
            result.Should().Be("837465923412345678");
        }

        [Fact]
        public void When_Null_Reference_Should_Raise_Error()
        {
            FluentActions.Invoking(() =>
                StringExtensions.AddCheckDigit(null, CheckDigitRule.MOD10v5)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_Blank_Reference_Should_Raise_Error()
        {
            FluentActions.Invoking(() =>
                StringExtensions.AddCheckDigit("", CheckDigitRule.MOD10v5)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_Letters_In_Reference_Should_Raise_Error()
        {
            FluentActions.Invoking(() =>
                StringExtensions.AddCheckDigit("ABC", CheckDigitRule.MOD10v5)).Should().Throw<ArgumentException>();
        }
    }
}