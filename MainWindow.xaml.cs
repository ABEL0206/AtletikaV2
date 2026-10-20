using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AtletikaV2
{
    public class Versenyzo
    {
        private int rajtszam;
        private string nev;
        private string versenyszam;
        private string nem;
        private int eletkor;
        private bool or;
        private bool vr;
        private bool er;

        public int Rajtszam { get => rajtszam; set => rajtszam = value; }
        public string Nev { get => nev; set => nev = value; }
        public string Versenyszam { get => versenyszam; set => versenyszam = value; }
        public string Nem { get => nem; set => nem = value; }
        public int Eletkor { get => eletkor; set => eletkor = value; }
        public bool OR { get => or; set => or = value; }
        public bool VR { get => vr; set => vr = value; }
        public bool ER { get => er; set => er = value; }

        public Versenyzo(int rajtszam, string nev, string versenyszam, string nem, int eletkor, bool or, bool vr, bool er)
        {
            this.rajtszam = rajtszam;
            this.nev = nev;
            this.versenyszam = versenyszam;
            this.nem = nem;
            this.eletkor = eletkor;
            this.or = or;
            this.vr = vr;
            this.er = er;
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<Versenyzo> Versenyzok { get; set; }
        private ICollectionView view;

        public MainWindow()
        {
            InitializeComponent();

            Versenyzok = new ObservableCollection<Versenyzo>();
            dgVersenyzok.ItemsSource = Versenyzok;

            view = CollectionViewSource.GetDefaultView(Versenyzok);
            view.Filter = SzuroFeltetel;

            FrissitSzamlalot();
        }

        private void FrissitSzamlalot()
        {
            if (txtLetszam != null && view != null)
            {
                int látható = view.Cast<object>().Count();
                int összes = Versenyzok.Count;
                txtLetszam.Text = $"Látható: {látható} / Összesen: {összes}";
            }
        }

        private bool SzuroFeltetel(object item)
        {
            if (item is Versenyzo v)
            {
                if (cmbRekordSzuro == null || cmbRekordSzuro.SelectedIndex <= 0)
                {
                    return true;
                }

                switch (cmbRekordSzuro.SelectedIndex)
                {
                    case 1: return v.OR;
                    case 2: return v.VR;
                    case 3: return v.ER;
                    default: return true;
                }
            }
            return false;
        }

        private void Szuro_Valtozott(object sender, SelectionChangedEventArgs e)
        {
            if (view != null)
            {
                view.Refresh();
                FrissitSzamlalot();
            }
        }

        private void Eletkor_Valtozott(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtEletkor != null)
            {
                txtEletkor.Text = $"{e.NewValue} év";
            }
        }

        private void Regisztral_Click(object sender, RoutedEventArgs e)
        {
            string nev = txtNev.Text.Trim();
            if (string.IsNullOrEmpty(nev))
            {
                MessageBox.Show("A versenyző nevének megadása kötelező.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNev.Focus();
                return;
            }
            if (nev.Contains(";") || nev.Contains("|"))
            {
                MessageBox.Show("A névben pontosvessző és függőleges vonal nem használható.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNev.Focus();
                return;
            }

            if (cmbVersenyszam.SelectedItem == null)
            {
                MessageBox.Show("Válasszon versenyszámot.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbVersenyszam.Focus();
                return;
            }
            string versenyszam = ((ComboBoxItem)cmbVersenyszam.SelectedItem).Content.ToString();

            if (rbFerfi.IsChecked != true && rbNo.IsChecked != true)
            {
                MessageBox.Show("Adja meg a versenyző nemét.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string nemKod = rbFerfi.IsChecked == true ? "F" : "N";

            int eletkor = (int)sldEletkor.Value;

            int rajtszam = 0;
            string rajtszamInput = txtRajtszam.Text.Trim();

            List<int> foglaltRajtszamok = Versenyzok.Select(v => v.Rajtszam).ToList();

            if (!string.IsNullOrEmpty(rajtszamInput))
            {
                if (!int.TryParse(rajtszamInput, out rajtszam) || rajtszam < 1 || rajtszam > 9999)
                {
                    MessageBox.Show("A rajtszám 1 és 9999 közötti egész szám lehet.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtRajtszam.Focus();
                    return;
                }

                if (foglaltRajtszamok.Contains(rajtszam))
                {
                    MessageBox.Show("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtRajtszam.Focus();
                    return;
                }
            }
            else
            {
                bool talaltSzabadot = false;
                for (int i = 1; i <= 9999; i++)
                {
                    if (!foglaltRajtszamok.Contains(i))
                    {
                        rajtszam = i;
                        talaltSzabadot = true;
                        break;
                    }
                }

                if (!talaltSzabadot)
                {
                    MessageBox.Show("Nincs több szabad rajtszám.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            bool or = chkOlimpiai.IsChecked == true;
            bool vr = chkVR.IsChecked == true;
            bool er = chkER.IsChecked == true;

            Versenyzo ujVersenyzo = new Versenyzo(rajtszam, nev, versenyszam, nemKod, eletkor, or, vr, er);
            Versenyzok.Add(ujVersenyzo);

            view.Refresh();
            FrissitSzamlalot();

            MessageBox.Show($"A regisztráció sikeres. Kiosztott rajtszám: {rajtszam}", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

            txtNev.Text = string.Empty;
            txtRajtszam.Text = string.Empty;
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = "atletika",
                Filter = "CSV-fájl (*.csv)|*.csv|Szövegfájl (*.txt)|*.txt",
                FilterIndex = 1
            };

            if (sfd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();

                if (sfd.FilterIndex == 1 && ext != ".csv")
                {
                    MessageBox.Show("A kiválasztott fájltípus (CSV) és a kiterjesztés nem egyezik!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (sfd.FilterIndex == 2 && ext != ".txt")
                {
                    MessageBox.Show("A kiválasztott fájltípus (TXT) és a kiterjesztés nem egyezik!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    if (sfd.FilterIndex == 1)
                    {
                        List<string> sorok = new List<string>
                        {
                            "Rajtszam;Nev;Versenyszam;Nem;Eletkor;OR;VR;ER"
                        };

                        foreach (var v in Versenyzok)
                        {
                            int orVal = v.OR ? 1 : 0;
                            int vrVal = v.VR ? 1 : 0;
                            int erVal = v.ER ? 1 : 0;

                            sorok.Add($"{v.Rajtszam};{v.Nev};{v.Versenyszam};{v.Nem};{v.Eletkor};{orVal};{vrVal};{erVal}");
                        }

                        File.WriteAllLines(sfd.FileName, sorok, Encoding.UTF8);
                    }
                    else if (sfd.FilterIndex == 2)
                    {
                        List<string> sorok = new List<string>();

                        for (int i = 0; i < Versenyzok.Count; i++)
                        {
                            var v = Versenyzok[i];
                            string nemText = v.Nem == "F" ? "férfi" : "nő";
                            string orText = v.OR ? "Olimpiai csúccsal rendelkezik" : "Olimpiai csúccsal nem rendelkezik";
                            string vrText = v.VR ? "világcsúccsal rendelkezik" : "világcsúccsal nem rendelkezik";
                            string erText = v.ER ? "Európa-csúccsal rendelkezik" : "Európa-csúccsal nem rendelkezik";

                            sorok.Add(v.Nev);
                            sorok.Add($"A {v.Eletkor} éves {nemText} versenyző a(z) {v.Versenyszam} versenyszámban indul, rajtszáma {v.Rajtszam}. {orText}, {vrText}, {erText}.");

                            if (i < Versenyzok.Count - 1)
                            {
                                sorok.Add(string.Empty);
                            }
                        }

                        File.WriteAllLines(sfd.FileName, sorok, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a fájl mentése során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "CSV-fájl (*.csv)|*.csv"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string[] allLines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                    if (allLines.Length <= 1)
                    {
                        Versenyzok.Clear();
                        view.Refresh();
                        FrissitSzamlalot();
                        return;
                    }

                    List<Versenyzo> ideiglenesLista = new List<Versenyzo>();
                    HashSet<int> ellenorzoRajtszamok = new HashSet<int>();

                    for (int i = 1; i < allLines.Length; i++)
                    {
                        string sor = allLines[i].Trim();
                        if (string.IsNullOrEmpty(sor)) continue;

                        string[] adatok = sor.Split(';');
                        if (adatok.Length >= 8)
                        {
                            if (int.TryParse(adatok[0], out int rajtszam) &&
                                int.TryParse(adatok[4], out int eletkor) &&
                                int.TryParse(adatok[5], out int orVal) &&
                                int.TryParse(adatok[6], out int vrVal) &&
                                int.TryParse(adatok[7], out int erVal))
                            {
                                if (ellenorzoRajtszamok.Contains(rajtszam))
                                {
                                    MessageBox.Show($"Hiba: A(z) {rajtszam} rajtszám többször is szerepel a fájlban!", "Adatkezelési hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                ellenorzoRajtszamok.Add(rajtszam);

                                bool or = orVal == 1;
                                bool vr = vrVal == 1;
                                bool er = erVal == 1;

                                Versenyzo v = new Versenyzo(rajtszam, adatok[1].Trim(), adatok[2].Trim(), adatok[3].Trim(), eletkor, or, vr, er);
                                ideiglenesLista.Add(v);
                            }
                        }
                    }

                    Versenyzok.Clear();
                    foreach (var elem in ideiglenesLista)
                    {
                        Versenyzok.Add(elem);
                    }

                    view.Refresh();
                    FrissitSzamlalot();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a fájl beolvasása során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}