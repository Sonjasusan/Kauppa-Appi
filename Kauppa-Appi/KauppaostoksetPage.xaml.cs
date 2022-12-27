using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Kauppa_Appi.Models;

namespace Kauppa_Appi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KauppaostoksetPage : ContentPage
    {
        ObservableCollection<Kauppaostokset> dataa = new ObservableCollection<Kauppaostokset>();

        int kaupId;
        string lat;
        string lon;

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

        async void LoadDataFromRestAPI()
        {

            try
            {

#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler(); //Lokaalia ajoa varten
                HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                string json = await client.GetStringAsync("api/kauppaostokset");

                IEnumerable<Kauppaostokset> ko = JsonConvert.DeserializeObject<Kauppaostokset[]>(json);

                ObservableCollection<Kauppaostokset> dataa = new ObservableCollection<Kauppaostokset>(ko);
                dataa = dataa;

                // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                koList.ItemsSource = dataa;

                // Tyhjennetään latausilmoitus label
                ko_lataus.Text = "";

            }

            catch (Exception e)
            {
                await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

            }
        }

        public KauppaostoksetPage(int id)
        {
            InitializeComponent();
            kaupId = id;

            //Annetaan latausilmoitukset
            ko_lataus.Text = "Ladataan kauppaostoksia. . .";
            lon_label.Text = "Haetaan sijaintiasi. . .";


            //SIJAINTI
            GetCurrentLocation();
            async void GetCurrentLocation()
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {

                        lon_label.Text = $"Longitude: " + location.Longitude;
                        lat_label.Text = $"Latitude: {location.Latitude}";

                        lat = location.Latitude.ToString();
                        lon = location.Longitude.ToString();

                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    await DisplayAlert("Virhe", fnsEx.ToString(), "Ok");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    await DisplayAlert("Virhe", fneEx.ToString(), "Ok");
                }
                catch (PermissionException pEx)
                {
                    await DisplayAlert("Virhe", pEx.ToString(), "Ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Virhe", ex.ToString(), "OK");
                }
            }

            LoadDataFromRestAPI();


        }

        //ALOITETAAN KAUPPAOSTOKSEN TEKO
        async void startbutton_Clicked(object sender, EventArgs e)
        {
            Kauppaostokset ko = (Kauppaostokset)koList.SelectedItem;
            if (ko == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse tuote", "Ok");
                return;
            }
            try
            {
                KauppaOperation kop = new KauppaOperation()
                {
                    KavijaID = kaupId,
                    KauppaOstosID = ko.IdKauppaOstos,
                    OperationType = "start",
                    Latitude = lat,
                    Longitude = lon,
                    Comment = "Aloitettu"
                };

                //Värähdetään kun painetaan aloitetaan kauppareissu
                var duration = TimeSpan.FromSeconds(1);
                Vibration.Vibrate(duration);

#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler(); //Lokaalia ajoa varten
                HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7292/");


                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(kop);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/kauppaostokset", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                //Muokataan
                if (success == false)
                {
                    await DisplayAlert("Ei voida aloittaa", "Tuote on jo merkattu", "OK"); //Tämä kohta korvataan (ei toimi tässä)
                }
                //Muokataan
                else if (success == true)
                {
                    await DisplayAlert("Kauppareissu aloitettu", "Kauppareissu on aloitettu", "OK"); //Tarkoituksena että tätä tarvitsee  vain kerran
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        //MERKITÄÄN OSTETUKSI
        async void ostobutton_Clicked(object sender, EventArgs e)
        {
            Kauppaostokset ko = (Kauppaostokset)koList.SelectedItem;

            if (ko == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse ostos.", "OK");
                return;
            }

            string result = await DisplayPromptAsync("Kommentti", "Kirjoita kommentti ostosreissusta");

            try
            {

                KauppaOperation op = new KauppaOperation
                {
                    KavijaID = kaupId,
                    KauppaOstosID = ko.IdKauppaOstos,
                    OperationType = "ready",
                    Comment = result,
                    Latitude = lat,
                    Longitude = lon
                };

                //Värähdetään
                var duration = TimeSpan.FromSeconds(1);
                Vibration.Vibrate(duration);
#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7292/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/kauppaostokset", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida lopettaa", "Valitse tuote", "OK");
                }

                if (success == true)
                {
                    await DisplayAlert("Tuote ostettu", "Tuote on merkattu ostetuksi", "OK");

                    await Navigation.PushAsync(new KauppaostoksetPage(kaupId));
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        async void navbutton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KaupassakavijatPage()); //Mennään takaisin kaupassakävijät sivuun


        }


        //LISÄTÄÄN KAUPPALISTALLE
        private async void Lisaa_Clicked(object sender, EventArgs e)
        {
            try
            {
                //näytetään "ponnahdusikkunat", joihin käyttäjä syöttää tiedot
                string tuote = await DisplayPromptAsync("Tuote", "Anna tuote");
                string kuvaus = await DisplayPromptAsync("Kuvaus", "Kuvaus: ");


                Kauppaostokset kauppaostos = new Kauppaostokset()
                {
                    //IdKauppaOstos = kaupId,

                    //Käyttäjä syöttää
                    Title = tuote,
                    Description = kuvaus,

                    //Tulee automaattisesti
                    Inprogress = false,
                    Active = true,
                    Completed = false,
                    CreatedAt = DateTime.Now,
                    LastModifiedAt = DateTime.Now
                };

                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                //#else
                //HttpClient client = new HttpClient();
                //#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7292/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(kauppaostos);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/kauppaostoslisays", content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success)  // Jos onnistuu näytetään alert viesti
                {
                    await DisplayAlert("Valmis!", "Tuote on nyt lisätty onnistuneesti kauppalistalle", "Sulje");
                    LoadDataFromRestAPI(); //ajaa ylläolevan metodin (lataa sivun tietoineen)
                }

                else
                {
                    await DisplayAlert("Virhe", "Virhe palvelimella", "Sulje");
                }

            }
            catch (Exception ex)
            {

                string errorMessage = ex.GetType().Name + ": " + ex.Message;
            }

        }

        private void Poista_Clicked(object sender, EventArgs e)
        {

        }
    }
}