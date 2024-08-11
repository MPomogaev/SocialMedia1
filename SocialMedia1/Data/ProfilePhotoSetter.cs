using SocialMedia1.Models;

namespace SocialMedia1.Data {

    public class ProfilePhotoSetter {
        static string defaultPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "defaultProfilePhoto.bmp");

        static public void SetPhotoOrDefault(ref Account acc) {
            if (acc.ProfilePhoto == null) {
                acc.ProfilePhoto = File.ReadAllBytes(defaultPhotoPath);
            }
        }

    }
}
