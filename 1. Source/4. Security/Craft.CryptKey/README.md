# Craft.Cryptography - Obsolete

> The `Craft.Cryptography` library is now obsolete. Please use the `Craft.CryptKey` library for all new development.

## Overview

`Craft.Cryptography` was a cryptography library for .NET, providing simple methods for common tasks like password hashing, data encryption, and HMAC generation. It aimed to simplify the use of .NET's cryptography features with a more accessible API.

## Status

This library is no longer maintained. It is recommended to migrate to `Craft.CryptKey` or use .NET's built-in cryptography features directly.

## Features

- **Password Hashing**: Simple methods for hashing passwords with PBKDF2.
- **Data Encryption/Decryption**: Encrypt and decrypt data using AES.
- **HMAC Generation**: Generate HMACs for data integrity verification.

## Installation

Install the NuGet package:

```bash
dotnet add package Craft.Cryptography
```

## Usage

### Password Hashing

```csharp
using Craft.Cryptography;

string password = "mySecurePassword";
string hashed = password.HashPassword(); // Hash the password

bool isValid = hashed.VerifyPassword(password); // Verify password
```

### Data Encryption/Decryption

```csharp
using Craft.Cryptography;

byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
byte[] key = GenerateRandom.Key(32); // Generate a random 256-bit key

// Encrypt the data
byte[] encrypted = data.EncryptAes(key);

// Decrypt the data
byte[] decrypted = encrypted.DecryptAes(key);
string result = Encoding.UTF8.GetString(decrypted); // "Hello, World!"
```

### HMAC Generation

```csharp
using Craft.Cryptography;

byte[] key = GenerateRandom.Key(32); // Generate a random 256-bit key
byte[] data = Encoding.UTF8.GetBytes("Some important data");

// Generate HMAC
byte[] hmac = data.ComputeHmac(key);
```

## Security Considerations

- Always use a unique salt for password hashing, stored securely per user.
- For HMAC, use a secret key of appropriate length (e.g., 256 bits for SHA-256).
- Regularly update and review cryptographic keys and secrets.

## Licensing

This library is licensed under the MIT License. See the LICENSE file for more information.

---

For more advanced key generation and obfuscation needs, including reversible transformations, please refer to the `Craft.CryptKey` library. It offers a more comprehensive solution for secure ID mapping and is built on top of the widely used Hashids algorithm.
