using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using Kauppa_Appi.Models;

namespace Kauppa_Appi
{
    public partial class KaupassakavijatPage : ContentPage
    {
        ObservableCollection<Kaupassakavijat> dataa = new ObservableCollection<Kaupassakavijat>();

        HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        public KaupassakavijatPage()
        {
            InitializeComponent();
            
            //Kutsutaan alempana määriteltyä funktiota kun ohjelma käynnistyy
            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            {
                kavija_lataus.Text = "Ladataan kaupassakävijöitä. . .";

                try
                {
                    HttpClientHandler GetInsecureHandler()
                    {
                        HttpClientHandler handler = new HttpClientHandler();
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            if (cert.Issuer.Equals("CN=localhost"))
                                return true;
                            return errors == System.Net.Security.SslPolicyErrors.None;
                        };
                        return handler;
                    }

#if DEBUG
                    HttpClientHandler insecureHandler = GetInsecureHandler(); //Lokaalia ajoa varten
                    HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                    string json = await client.GetStringAsync("api/kaupassakavijat");

                    IEnumerable<Kaupassakavijat> kaupassakavijat = JsonConvert.DeserializeObject<Kaupassakavijat[]>(json);
                    // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto,
                    // pääsee siihen käsiksi.
                    // asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:
                    ObservableCollection<Kaupassakavijat> dataa2 = new ObservableCollection<Kaupassakavijat>(kaupassakavijat);
                    dataa = dataa2;

                    // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                    kaList.ItemsSource = dataa;

                    // Tyhjennetään latausilmoitus label
                    kavija_lataus.Text = "";

                }

                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

                }
            }

        }

        // Hakutoiminto - haetaan kaupassakävijöitä
        private void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;
            searchBar.TextChanged += OnTextChanged; //teksti muuttuu

            kaList.ItemsSource = dataa.Where(x => x.Nimi.ToLower().Contains(searchText.ToLower()));

        }

        //Hakukenttä palautuu aiempaan näkymään (jossa näkyi kaikki kaupassakävijät)
        void OnTextChanged(object sender, EventArgs e) //OnTextChanged
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;
            kaList.ItemsSource = dataa.Where(x => x.Nimi.ToLower().Contains(searchText.ToLower()));
        }

        async void navbutton_Clicked(object sender, EventArgs e) //buttoni kauppaostoksiin
        {
            Kaupassakavijat kaup = (Kaupassakavijat)kaList.SelectedItem;
            if (kaup == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse kaupassakävijä jatkaaksesi!", "OK");
                return;

            }
            else
            {
                int kaupId = kaup.IdKavija;
                await Navigation.PushAsync(new KauppaostoksetPage(kaupId)); //Navigoidaan KauppaOstoksetPagelle
            }
        }

        //LISÄTÄÄN KAUPASSAKAVIJA
        async private void Lisaa_Clicked(object sender, EventArgs e)
        {
            try
            {
                string nimi = await DisplayPromptAsync("Nimi", "Anna kaupassakävijän nimi (etunimi riittää)");

                Kaupassakavijat kaupassakavija = new Kaupassakavijat()
                {
                    //Käyttäjä syöttää
                    Nimi = nimi,

                   //Tulee automaattisesti
                    Active = true,
                    CreatedAt = DateTime.Now,
                };

                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                client.BaseAddress = new Uri("https://10.0.2.2:7292/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(kaupassakavija);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/KaupassakavijaLisays", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success)  // Jos onnistuu näytetään alert viesti
                {

                    await DisplayAlert("Valmis!", "Kaupassakävijä on nyt tallennettu onnistuneesti ja valmiina valittavaksi!", "Sulje");
                    await Navigation.PushAsync(new KaupassakavijatPage()); //Päivitetään sivu

                }
                else
                {
                    await DisplayAlert("Virhe", "Virhe palvelimella.", "Sulje");
                }

            }
            catch (Exception ex) // Otetaan poikkeus ex muuttujaan ja sijoitetaan errorMessageen
            {

                string errorMessage = ex.GetType().Name + ": " + ex.Message;

            }

        }

        // MUOKATAAN KAUPASSAKÄVIJÄÄ
        private void Muokkaa_Clicked(object sender, EventArgs e)
        {

        }

        //POISTETAAN KAUPASSAKÄVIJÄ
        private void Poista_Clicked(object sender, EventArgs e)
        {

        }

        
    }
}
