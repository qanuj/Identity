// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Identity.Test
{
    public class PasswordValidatorTest
    {
        [Flags]
        public enum Errors
        {
            None = 0,
            Length = 2,
            Alpha = 4,
            Upper = 8,
            Lower = 16,
            Digit = 32,
        }

        [Fact]
        public async Task ValidateThrowsWithNullTest()
        {
            // Setup
            var validator = new PasswordValidator<IdentityUser>();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>("password", () => validator.ValidateAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("manager", () => validator.ValidateAsync("foo", null));
        }


        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("abcde")]
        public async Task FailsIfTooShortTests(string input)
        {
            const string error = "Passwords must be at least 6 characters.";
            var manager = MockHelpers.TestUserManager<IdentityUser>();
            var valid = new PasswordValidator<IdentityUser>();
            manager.Options.Password.RequireUppercase = false;
            manager.Options.Password.RequireNonLetterOrDigit = false;
            manager.Options.Password.RequireLowercase = false;
            manager.Options.Password.RequireDigit = false;
            IdentityResultAssert.IsFailure(await valid.ValidateAsync(input, manager), error);
        }

        [Theory]
        [InlineData("abcdef")]
        [InlineData("aaaaaaaaaaa")]
        public async Task SuccessIfLongEnoughTests(string input)
        {
            var manager = MockHelpers.TestUserManager<IdentityUser>();
            var valid = new PasswordValidator<IdentityUser>();
            manager.Options.Password.RequireUppercase = false;
            manager.Options.Password.RequireNonLetterOrDigit = false;
            manager.Options.Password.RequireLowercase = false;
            manager.Options.Password.RequireDigit = false;
            IdentityResultAssert.IsSuccess(await valid.ValidateAsync(input, manager));
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aaaaaaaaaaa")]
        public async Task FailsWithoutRequiredNonAlphanumericTests(string input)
        {
            var manager = MockHelpers.TestUserManager<IdentityUser>();
            var valid = new PasswordValidator<IdentityUser>();
            manager.Options.Password.RequireUppercase = false;
            manager.Options.Password.RequireNonLetterOrDigit = true;
            manager.Options.Password.RequireLowercase = false;
            manager.Options.Password.RequireDigit = false;
            manager.Options.Password.RequiredLength = 0;
            IdentityResultAssert.IsFailure(await valid.ValidateAsync(input, manager),
                "Passwords must have at least one non letter or digit character.");
        }

        [Theory]
        [InlineData("@")]
        [InlineData("abcd@e!ld!kajfd")]
        [InlineData("!!!!!!")]
        public async Task SucceedsWithRequiredNonAlphanumericTests(string input)
        {
            var manager = MockHelpers.TestUserManager<IdentityUser>();
            var valid = new PasswordValidator<IdentityUser>();
            manager.Options.Password.RequireUppercase = false;
            manager.Options.Password.RequireNonLetterOrDigit = true;
            manager.Options.Password.RequireLowercase = false;
            manager.Options.Password.RequireDigit = false;
            manager.Options.Password.RequiredLength = 0;
            IdentityResultAssert.IsSuccess(await valid.ValidateAsync(input, manager));
        }

        [Theory]
        [InlineData("abcde", Errors.Length | Errors.Alpha | Errors.Upper | Errors.Digit)]
        [InlineData("a@B@cd", Errors.Digit)]
        [InlineData("___", Errors.Length | Errors.Digit | Errors.Lower | Errors.Upper)]
        [InlineData("a_b9de", Errors.Upper)]
        [InlineData("abcd@e!ld!kaj9Fd", Errors.None)]
        [InlineData("aB1@df", Errors.None)]
        public async Task UberMixedRequiredTests(string input, Errors errorMask)
        {
            const string alphaError = "Passwords must have at least one non letter or digit character.";
            const string upperError = "Passwords must have at least one uppercase ('A'-'Z').";
            const string lowerError = "Passwords must have at least one lowercase ('a'-'z').";
            const string digitError = "Passwords must have at least one digit ('0'-'9').";
            const string lengthError = "Passwords must be at least 6 characters.";
            var manager = MockHelpers.TestUserManager<IdentityUser>();
            var valid = new PasswordValidator<IdentityUser>();
            var errors = new List<string>();
            if ((errorMask & Errors.Length) != Errors.None)
            {
                errors.Add(lengthError);
            }
            if ((errorMask & Errors.Alpha) != Errors.None)
            {
                errors.Add(alphaError);
            }
            if ((errorMask & Errors.Digit) != Errors.None)
            {
                errors.Add(digitError);
            }
            if ((errorMask & Errors.Lower) != Errors.None)
            {
                errors.Add(lowerError);
            }
            if ((errorMask & Errors.Upper) != Errors.None)
            {
                errors.Add(upperError);
            }
            if (errors.Count == 0)
            {
                IdentityResultAssert.IsSuccess(await valid.ValidateAsync(input, manager));
            }
            else
            {
                IdentityResultAssert.IsFailure(await valid.ValidateAsync(input, manager), string.Join(" ", errors));
            }
        }
    }
}