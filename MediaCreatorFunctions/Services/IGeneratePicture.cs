using System;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Services
{
    public interface IGeneratePictureService
    {
        bool DimensionsOk(int width, int height);
        Task<string> GeneratePicture(string prompt, string folderPath, int width, int height, Guid userId, string filename = "picture.png");
    }
}
