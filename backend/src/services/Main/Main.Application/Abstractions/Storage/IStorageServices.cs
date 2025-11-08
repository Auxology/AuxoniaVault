namespace Main.Application.Abstractions.Storage;

public interface IStorageServices
{
    Task<string> StartMultiPartUploadAsync(string fileName, string contentType, CancellationToken cancellationToken);
}