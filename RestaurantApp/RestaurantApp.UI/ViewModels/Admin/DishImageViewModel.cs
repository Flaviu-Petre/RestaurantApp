using RestaurantApp.UI.Infrastructure;
using System.Windows.Media.Imaging;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class DishImageViewModel : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private int _dishId;
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }
}