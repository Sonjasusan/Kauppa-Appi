using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kauppa_Appi
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class OhjePage : ContentPage
{
    public OhjePage()
    {
        InitializeComponent();
    }

        private void kaupassakavijoihin_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new KaupassakavijatPage()); //Navigoidaan Kaupassakävijät sivulle
        }
    }
}