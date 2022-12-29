using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials; //Xamarin essentials -kirjasto 
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RuokaAppiBackend.Models; //<- Käytetään backendistä tuotuja modeleita
using System.Security.Cryptography.X509Certificates;

namespace Kauppa_Appi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KauppaostoksetPage : ContentPage
    {
        ObservableCollection<KauppaOstokset> dataa = new ObservableCollection<KauppaOstokset>();

        int kaupId; //kaupassakävijän id
        string lat; //sijainti leveyspiiri (longitude)
        string lon; //sijainti pituuspiiri (latitude)

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
                //client.BaseAddress = new Uri("https://10.0.2.2:7292/"); <-Lokaali
                client.BaseAddress = new Uri(Constants.ServiceUri); //<- Käytetään Constants.cs:ssä määritetty azuren osoitetta
                string json = await client.GetStringAsync("api/kauppaostokset");

                IEnumerable<KauppaOstokset> ko = JsonConvert.DeserializeObject<KauppaOstokset[]>(json);

                ObservableCollection<KauppaOstokset> dataa = new ObservableCollection<KauppaOstokset>(ko);
                dataa = dataa;

                // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                koList.ItemsSource = dataa; //kauppaostoslista

                // Tyhjennetään latausilmoitus label
                ko_lataus.Text = "";

            }

            catch (Exception e)
            {
                //Jos listauksen lataaminen ei onnistu; näytetään virhe ilmoitus
                await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

            }
        }

        public KauppaostoksetPage(int id)
        {
            InitializeComponent();
            kaupId = id; //kaupassakävijän id

            //Annetaan latausilmoitukset
            ko_lataus.Text = "Ladataan kauppaostoksia. . ."; //kauppaostoslistaus
            lon_label.Text = "Haetaan sijaintiasi. . ."; //sijainti


            //SIJAINTI
            GetCurrentLocation(); //haetaan käyttäjän tämänhetkinen sijainti
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
                    await DisplayAlert("Virhe", fnsEx.ToString(), "OK");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    await DisplayAlert("Virhe", fneEx.ToString(), "OK");
                }
                catch (PermissionException pEx)
                {
                    await DisplayAlert("Virhe", pEx.ToString(), "OK");
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
            KauppaOstokset ko = (KauppaOstokset)koList.SelectedItem;
            if (ko == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse tuote", "OK");
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
                //client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                client.BaseAddress = new Uri(Constants.ServiceUri);


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
            KauppaOstokset ko = (KauppaOstokset)koList.SelectedItem;

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
                //client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                client.BaseAddress = new Uri(Constants.ServiceUri);

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


                KauppaOstokset kauppaostos = new KauppaOstokset()
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
                //client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                client.BaseAddress = new Uri(Constants.ServiceUri);

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(kauppaostos);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/kauppaostoslisays", content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success boolean muuttujaan (joka on true tai false)
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success)  // Jos onnistuu näytetään alert viesti -> success = true
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

        //POISTO

        private async void Poistobutton_Clicked(object sender, EventArgs e)
        {
            KauppaOstokset kop = (KauppaOstokset)koList.SelectedItem;

            if (kop == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse poistettava tuote.", "OK");
                return;
            }

            try
            {              
                     async Task PoistaTuote(int id)
                    {
                        HttpClientHandler insecureHandler = GetInsecureHandler();
                        HttpClient client = new HttpClient(insecureHandler);
                        //#else
                        //HttpClient client = new HttpClient();
                        //#endif
                        client.BaseAddress = new Uri(Constants.ServiceUri);

                        // Lähetetään serialisoitu objekti back-endiin Delete -pyyntönä
                        HttpResponseMessage message = await client.DeleteAsync("/api/kauppaostokset" + id);

                        // Otetaan vastaan palvelimen vastaus
                        string reply = await message.Content.ReadAsStringAsync();

                        //Asetetaan vastaus serialisoituna success boolean muuttujaan (joka on true tai false)
                        bool success = JsonConvert.DeserializeObject<bool>(reply);

                        if (success == true)
                        {
                            await DisplayAlert("Valmis!", "Tuote poistettu onnistuneesti listalta", "Sulje");
                            await Navigation.PushAsync(new KauppaostoksetPage(kaupId));

                        }

                        if (success == false)
                        {
                            await DisplayAlert("Ei voida poistaa", "Valitse tuote", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Virhe palvelimella", "OK");
                        }
                    }
               
            }
            catch (Exception ex)
            {

                string errorMessage = ex.GetType().Name + ": " + ex.Message;
                
            }
        }

    }
}