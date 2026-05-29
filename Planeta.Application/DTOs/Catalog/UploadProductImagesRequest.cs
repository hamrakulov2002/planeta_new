using Microsoft.AspNetCore.Http;

namespace Planeta.Application.DTOs.Catalog;

public class UploadProductImagesRequest
{
    public IFormFile? MainImage { get; set; }
    public List<IFormFile>? ExtraImages { get; set; }
}