﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace Kauppa_Appi
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void kaupassakavijatsivulle_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new KaupassakavijatPage()); //Navigoidaan Kaupassakävijät sivulle

        }
    }
}
