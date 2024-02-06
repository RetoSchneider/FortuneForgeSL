namespace FortuneForgeSL
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}
