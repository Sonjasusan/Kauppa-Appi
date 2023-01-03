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
                    //client.BaseAddress = new Uri("https://10.0.2.2:7292/"); <-lokaali                   
                    client.BaseAddress = new Uri(Constants.ServiceUri); //<- Käytetään Constants.cs:ssä määritetty azuren osoitetta
                    string json = await client.GetStringAsync("api/kaupassakavijat");

                    IEnumerable<Kaupassakavijat> kaupassakavijat = JsonConvert.DeserializeObject<Kaupassakavijat[]>(json);
                    // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto,
                    // pääsee siihen käsiksi.
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

            //Jos nimi sisältää pienen tai ison kirjaimen
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
                await DisplayAlert("Valinta puuttuu", "Valitse kaupassakävijä jatkaaksesi!", "OK");
                return;
            }

            else
            {
                int kaupId = kaup.IdKavija;
                await Navigation.PushAsync(new KauppaostoksetPage(kaupId)); //Navigoidaan KauppaOstoksetPagelle
                                                                            //kaupId = valitun kaupassakävijän id
            }
        }

        //LISÄTÄÄN KAUPASSAKAVIJA
        async private void Lisaa_Clicked(object sender, EventArgs e)
        {
            try
            {
                string nimi = await DisplayPromptAsync("Nimi", "Anna kaupassakävijän nimi (etunimi riittää)");

                Kaupassakavijat kavija = new Kaupassakavijat()
                {
                    //Käyttäjä syöttää
                    Nimi = nimi,

                    //Tulee automaattisesti
                    Active = true,
                    CreatedAt = DateTime.Now
                };

                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
                //#else
                //HttpClient client = new HttpClient();
                //#endif
                //client.BaseAddress = new Uri("https://10.0.2.2:7292/"); //lokaalia ajoa varten
                client.BaseAddress = new Uri(Constants.ServiceUri); //Constans.cs:ssä määritetty azure osoite

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
        private void Poista_Clicked(object sender, EventArgs e)
        {
            //tähän tulee poisto
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        //MUOKATAAN KAUPASSAKÄVIJÄÄ
        private void Muokkaa_Clicked(object sender, EventArgs e)
        {
            //tähän tulee muokkaus
        }

        //OHJESIVULLE
        private void Ohjeisiin_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new OhjePage()); //Navigoidaan Kaupassakävijät sivulle

        }
    }
}
