﻿using Newtonsoft.Json;
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
                var duration = TimeSpan.FromSeconds(0.4);
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
                await DisplayAlert("Valinta puuttuu", "Valitse ensin tuote", "OK");
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
                var duration = TimeSpan.FromSeconds(0.2);
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

        //LISÄTÄÄN KAUPPALISTALLE

        private async void Lisaa_Clicked(object sender, EventArgs e)
        {
            try
            {
                //näytetään "ponnahdusikkunat", joihin käyttäjä syöttää tiedot
                string tuote = await DisplayPromptAsync("Tuote", "Anna tuote");
                string kuvaus = await DisplayPromptAsync("Kuvaus", "Anna lisätietoa tuotteesta: ");


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

        private async void Poistobutton_Clicked(object sender, EventArgs e) // poisto buttonin avulla
        {
            KauppaOstokset koid = (KauppaOstokset)koList.SelectedItem; //kauppalistalta valittu tuote

            if (koid == null) //Jos kauppalistalta ei ole valittu mitään - koList.SelectedItem = null
            {
                //Ilmoitetaan käyttäjälle
                await DisplayAlert("Valinta puuttuu", "Valitse ensin poistettava tuote", "OK");
                return;
            }

            else //<- kun ollaan valittu poistettava tuote kauppalistalta
            {
                int id = koid.IdKauppaOstos; //poistettavan tuotteen id on poistettava tuote
                id = koid.IdKauppaOstos; //poistettavan tuotteen id
            

            try
            {
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                client.BaseAddress = new Uri(Constants.ServiceUri); //Constants:ssa oleva azure backendi osoite
                   

                string input = JsonConvert.SerializeObject(id); //konvertoidaan jsoniksi
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                 HttpResponseMessage response = await client.DeleteAsync("/api/kauppaostokset/" +id.ToString()); //poistettavan tuote ja id

                 string reply = await response.Content.ReadAsStringAsync(); //otetaan vastaan http-vastaus


             if (response.IsSuccessStatusCode)  // Jos onnistuu näytetään alert viesti Http statuskoodi on 200
             {
                  await DisplayAlert("Valmis!", "Tuote on nyt poistettu onnistuneesti kauppalistalta", "Sulje");
                  LoadDataFromRestAPI(); //päivitetään sivu (ladataan uudestaan ajamalla LoadDataFromRestAPI(); -metodi)
              }

             //Jos poisto ei onnistu
             else
             {
               await DisplayAlert("Virhe", "Virhe palvelimella", "Sulje"); //ilmoitetaan käyttäjälle virheestä

              }

              }
            catch (Exception ex) // Epäonnistutaan
            {
                string errorMessage = ex.GetType().Name + ": " + ex.Message;

            }
            
            }
        }

        //MUOKKAUS

        private async void Muokkausbutton_Clicked(object sender, EventArgs e)
        {

            KauppaOstokset koip = (KauppaOstokset)koList.SelectedItem; //<- kauppalistalta valittu tuote

            if (koip == null) //Jos kauppalistalta ei ole valittu tuotetta
            {
                //Ilmoitetaan käyttäjälle
                await DisplayAlert("Valinta puuttuu", "Valitse tuote ensin", "OK");
                return;
            }
            else //<- kun ollaan valittu listalta muokattava tuote
            {
                int id = koip.IdKauppaOstos; //muokattavan tuotteen id on muokattava tuote
                id = koip.IdKauppaOstos; //muokttavan tuotteen id
            

            try
            {
                string tuote = await DisplayPromptAsync("Tuote", "Anna tuote");
                string kuvaus = await DisplayPromptAsync("Kuvaus", "Kuvaus: ");
                    
                KauppaOstokset muokattavaTuote = new KauppaOstokset() 
                {
                    //Käyttäjä syöttää
                    Title = tuote,
                    Description = kuvaus,

                    //Tulee automaattisesti
                    IdKauppaOstos = id, //kauppaostoksen id on valitun tuotteen id
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
                string input = JsonConvert.SerializeObject(muokattavaTuote);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");
                
                //Viedään put pyyntö osoitteella, valitulla id:llä ja syötetyillä tiedoilla
                HttpResponseMessage message = await client.PutAsync("/api/kauppaostokset/"+ id.ToString(),content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();


                if (message.IsSuccessStatusCode)  // Jos onnistuu näytetään alert viesti (Http statuskoodi on 200)
                {
                    await DisplayAlert("Valmis!", "Tuotetta muokattu onnistuneesti", "Sulje");
                    LoadDataFromRestAPI(); //päivitetään sivu ajamalla LoadDataFromRestApi -metodin (lataa sivun tietoineen uudelleen)
                }

                else //Jos epäonnistutaan
                {
                    await DisplayAlert("Virhe", "Virhe palvelimella", "Sulje");
                }
            }
                
            catch (Exception ex)
            {
                string errorMessage = ex.GetType().Name + ": " + ex.Message;
            }

            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new OhjePage()); //Navigoidaan Kaupassakävijät sivulle

        }
    }
}