using System;
using System.Security.Cryptography.X509Certificates;

public class TokenDTO
{
	public string Token { get; set; } = null!;

	public DateTime Expiration { get; set; }
}
