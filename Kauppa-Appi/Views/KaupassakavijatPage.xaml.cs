using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using RuokaAppiBackend.Models; //<- Käytetään backendistä tuotuja modeleita
using System.Collections;

namespace Kauppa_Appi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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
                    client.BaseAddress = new Uri("https://10.0.2.2:7292/"); /*< -lokaali*/
                    //client.BaseAddress = new Uri(Constants.ServiceUri); //<- Käytetään Constants.cs:ssä määritetty azuren osoitetta
                    string json = await client.GetStringAsync("api/kaupassakavijat");

                    IEnumerable<Kaupassakavijat> kaupassakavijat = JsonConvert.DeserializeObject<Kaupassakavijat[]>(json);
                    
                    // asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:
                    ObservableCollection<Kaupassakavijat> dataa2 = new ObservableCollection<Kaupassakavijat>(kaupassakavijat);
                    dataa = dataa2;

                    // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                    kaList.ItemsSource = dataa; //<- Kaupassakävijät listaus

                    // Tyhjennetään latausilmoitus label
                    kavija_lataus.Text = "";
                }

                catch (Exception e)
                {
                    //Jos ei onnistuttu lataamaan kaupassakävijöitä; annetaan virhe ilmoitus
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

                }
            }

        }

        // HAKUTOIMINTO - haetaan kaupassakävijöitä
        private void OnSearchBarButtonPressed(object sender, EventArgs args)
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;
            searchBar.TextChanged += OnTextChanged; //teksti muuttuu

            //Haetaan (jos kaupassakävijässä on esim "i"-kirjain, näytetään kaikki kaupassakävijät, joiden nimiin sisätyy i-kirjain
            kaList.ItemsSource = dataa.Where(x => x.Nimi.ToLower().Contains(searchText.ToLower()));
        }

        //Hakukenttä palautuu aiempaan näkymään (jossa näkyi kaikki kaupassakävijät)
        void OnTextChanged(object sender, EventArgs e) //OnTextChanged - teksti muuttuu
        {
            SearchBar searchBar = (SearchBar)sender;
            string searchText = searchBar.Text;
            kaList.ItemsSource = dataa.Where(x => x.Nimi.ToLower().Contains(searchText.ToLower()));
        }

        async void navbutton_Clicked(object sender, EventArgs e) //buttoni kauppaostoksiin siirtymistä varten
        {
            Kaupassakavijat kaup = (Kaupassakavijat)kaList.SelectedItem;
            if (kaup == null) //jos kaupassakävijää ei olla valittu - eli ei ole id:tä
            {
                await DisplayAlert("Valinta puuttuu", "Valitse kaupassakävijä jatkaaksesi!", "OK"); //ilmoitus käyttäjälle
                return;
            }

            else
            {
                int kaupId = kaup.IdKavija; //kaupId = valitun kaupassakävijän id
                await Navigation.PushAsync(new KauppaostoksetPage(kaupId)); //Navigoidaan KauppaOstoksetPagelle
                                                                           //kaupId = valitun kaupassakävijän id
            }
        }

        //LISÄTÄÄN KAUPASSAKAVIJA
        async private void Lisaa_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Käyttäjä syöttää kaupassakävijän nimen
                string nimi = await DisplayPromptAsync("Nimi", "Anna kaupassakävijän nimi (etunimi riittää)");

                //Uuden kaupassakävijän lisäys
                Kaupassakavijat kavija = new Kaupassakavijat()
                {
                    //Käyttäjä syöttää
                    Nimi = nimi,

                    //Tulee automaattisesti
                    Active = true,// -> true - uusi kaupassakävijä
                    CreatedAt = DateTime.Now
                };

                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                //#else
                //HttpClient client = new HttpClient();
                //#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7292/"); //lokaalia ajoa varten
                //client.BaseAddress = new Uri(Constants.ServiceUri); //Constans.cs:ssä määritetty azure osoite

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(kavija);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/KaupassakavijaLisays", content);


                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success boolean muuttujaan (joka on true tai false)
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success)  // Jos onnistuu -> success = true
                {
                    //näytetään alert viesti käyttäjälle kaupassakävijän lisäyksen onnistumisesta
                    await DisplayAlert("Valmis!", "Kaupassakävijä tallennettu onnistuneesti!", "Sulje"); 
                    await Navigation.PushAsync(new KaupassakavijatPage()); //Päivitetään sivu uudelleen

                }

                else //muuten - jos ei onnistu -> success = false
                {
                    await DisplayAlert("Virhe", "Virhe palvelimella", "Sulje"); //annetaan virheilmoitus
                    Debug.WriteLine("Epäonnistui");
                }

            }
            catch (Exception ex) // Otetaan poikkeus ex muuttujaan ja sijoitetaan errorMessageen
            {

                string errorMessage = ex.GetType().Name + ": " + ex.Message;

            }
        }

        //POISTETAAN KAUPASSAKÄVIJÄ
        private async void Poista_Clicked(object sender, EventArgs e) // <- poista nappi
        {
            Kaupassakavijat kaid = (Kaupassakavijat)kaList.SelectedItem; //Valitaan poistettava kaupassakävijä

            if (kaid == null) //Jos valintaa ei ole
            {
                //Ilmoitetaan käyttäjälle
                await DisplayAlert("Valinta puuttuu!", "Valitse ensin poistettava kaupassakävijä", "OK");
                return;
            }
            else //<- ollaan valittu poistettava kaupassakävijä
            {
                int id = kaid.IdKavija; //valitun kaupassakävijän id on poistettava kaupassakävijä
                id = kaid.IdKavija; //poistettavan kaupassakävijän id
           

            try
            {
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
                    //client.BaseAddress = new Uri(Constants.ServiceUri); //Constants:ssa oleva azure backendi osoite
                    client.BaseAddress = new Uri("https://10.0.2.2:7292/"); //lokaalia ajoa varten



                    string input = JsonConvert.SerializeObject(id); //konvertoidaan jsoniksi
                    StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                    //Viedään delete pyyntö osoitteella,ja valitulla id:llä
                    HttpResponseMessage response = await client.DeleteAsync("/api/kaupassakavijat/" +id); //poistettava kaupassakävijä ja id

                    string reply = await response.Content.ReadAsStringAsync(); //otetaan vastaan http-vastaus


                    if (response.IsSuccessStatusCode)  // Jos onnistuu näytetään alert viesti (Http statuskoodi on 200)
                    {
                        await DisplayAlert("Valmis!", "Kaupassakävijä on nyt poistettu onnistuneesti kaupassakävijät -listalta", "Sulje");
                        await Navigation.PushAsync(new KaupassakavijatPage()); //Päivitetään sivu uudelleen

                    }

                    //Jos poisto ei onnistu
                    else
                    {
                        //kaupassakävijöitä, joilla on "dataa" ei voida poistaa
                        await DisplayAlert("Virhe", "Tätä kaupassakävijää ei voida poistaa.", "Sulje"); //ilmoitetaan käyttäjälle virheestä

                    }

                }

            catch (Exception ex)
            {
              string errorMessage = ex.GetType().Name + ": " + ex.Message;

            }


            }
        }

        //MUOKATAAN KAUPASSAKÄVIJÄÄ

        private async void Muokkaa_Clicked(object sender, EventArgs e) // <- muokkaus nappi
        {
            Kaupassakavijat ka = (Kaupassakavijat)kaList.SelectedItem; //Valitaan muokattava kaupassakävijä

            if (ka == null) //Jos valintaa ei ole
            {
                await DisplayAlert("Valinta puuttuu", "Valitse ensin muokattava kaupassakävijä", "OK");
                return; //palataan samalle sivulle
            }
            else
            {
                int id = ka.IdKavija; //muokattavan kaupassakävijän id on muokattava kaupassakävijä
                id = ka.IdKavija; //muokattavan kaupassakävijän id

                try
                {
                    string nimi = await DisplayPromptAsync("Nimi", "Anna kaupassakävijän nimi");

                    Kaupassakavijat muokattavaKavija = new Kaupassakavijat()
                    {
                        //Käyttäjä syöttää
                        Nimi = nimi,

                        //Tulee automaattisesti
                        IdKavija = id, //kaupassakävijän id on valittu kaupassakävijä
                        Active = true,
                        CreatedAt = DateTime.Now
                    };

                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
                    //#else
                    //HttpClient client = new HttpClient();
                    //#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:7292/");
                    //client.BaseAddress = new Uri(Constants.ServiceUri);

                    // Muutetaan em. data objekti Jsoniksi
                    string input = JsonConvert.SerializeObject(muokattavaKavija);
                    StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                    //Viedään put pyyntö osoitteella, valitulla id:llä ja syötetyillä tiedoilla
                    HttpResponseMessage message = await client.PutAsync("/api/kaupassakavijat/" + id.ToString(), content);

                    // Otetaan vastaan palvelimen vastaus
                    string reply = await message.Content.ReadAsStringAsync();


                    if (message.IsSuccessStatusCode)  // Jos onnistuu näytetään alert viesti (Http statuskoodi on 200)
                    {
                        await DisplayAlert("Valmis!", "Kaupassakävijää muokattu onnistuneesti!", "Sulje");
                        await Navigation.PushAsync(new KaupassakavijatPage()); //Päivitetään sivu
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

        //OHJESIVULLE
        private void Ohjeisiin_Clicked(object sender, EventArgs e) // <- ohjeisiin button
        {
            Navigation.PushAsync(new OhjePage()); //Navigoidaan ohje sivulle

        }
    }
}
