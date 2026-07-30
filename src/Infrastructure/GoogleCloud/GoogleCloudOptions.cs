using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.GoogleCloud;

public record GoogleCloudOptions
{
    public string ProjectId { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public GoogleSecret Secret { get; init; }
}


public record GoogleSecret
{
    [JsonPropertyName("web")]
    public GoogleWebSecret Web { get; init; }

    [JsonPropertyName("account_service")]
    public GoogleAccountServiceSecret AccountService { get; init; }
}

public record GoogleWebSecret
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; } = string.Empty;

    [JsonPropertyName("auth_uri")]
    public string AuthUri { get; init; } = string.Empty;

    [JsonPropertyName("token_uri")]
    public string TokenUri { get; init; } = string.Empty;

    [JsonPropertyName("auth_provider_x509_cert_url")]
    public string AuthProviderX509CertUrl { get; init; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; } = string.Empty;

    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public record GoogleAccountServiceSecret
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; } = string.Empty;

    [JsonPropertyName("private_key_id")]
    public string PrivateKeyId { get; init; } = string.Empty;

    [JsonPropertyName("private_key")]
    public string PrivateKey { get; init; } = string.Empty;

    [JsonPropertyName("client_email")]
    public string ClientEmail { get; init; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("auth_uri")]
    public string AuthUri { get; init; } = string.Empty;

    [JsonPropertyName("token_uri")]
    public string TokenUri { get; init; } = string.Empty;

    [JsonPropertyName("auth_provider_x509_cert_url")]
    public string AuthProviderX509CertUrl { get; init; } = string.Empty;

    [JsonPropertyName("client_x509_cert_url")]
    public string ClientX509CertUrl { get; init; } = string.Empty;

    [JsonPropertyName("universe_domain")]
    public string UniverseDomain { get; init; } = string.Empty;

    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this);
    }
}