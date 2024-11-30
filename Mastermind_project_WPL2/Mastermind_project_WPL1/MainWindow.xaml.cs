using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mastermind_project_WPL1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int attempts = 1;
        private string targetColorCode;
        private bool debugMode = false;
        private DispatcherTimer timer;
        private int remainingTime = 10;

        public MainWindow()
        {
            InitializeComponent();

            // Initialiseer de timer
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

            targetColorCode = generateRandomColorCode();
            debugTextBox.Text = targetColorCode;
            updateWindowTitle();

            // Vul de comboBoxen met kleuren
            string[] colors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
            comboBox1.ItemsSource = colors;
            comboBox2.ItemsSource = colors;
            comboBox3.ItemsSource = colors;
            comboBox4.ItemsSource = colors;

            // Event handlers voor kleurenselectie
            comboBox1.SelectionChanged += (s, e) => updateLabel(label1, comboBox1.SelectedItem.ToString());
            comboBox2.SelectionChanged += (s, e) => updateLabel(label2, comboBox2.SelectedItem.ToString());
            comboBox3.SelectionChanged += (s, e) => updateLabel(label3, comboBox3.SelectedItem.ToString());
            comboBox4.SelectionChanged += (s, e) => updateLabel(label4, comboBox4.SelectedItem.ToString());

            // Sneltoets voor debug-modus
            this.KeyDown += MainWindow_KeyDown;

            // Start de timer bij de eerste codegeneratie
            startCountdown();
        }

        // Sneltoets-event voor debug-modus
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.Modifiers == ModifierKeys.Control)
            {
                toggleDebugMode();
            }
        }

        /// <summary>
        /// Schakelt de debug-modus in of uit.
        /// In debug-modus wordt de gegenereerde kleurencode zichtbaar in de debugTextBox.
        /// Activering gebeurt via de sneltoets CTRL + F12.
        /// </summary>
        private void toggleDebugMode()
        {
            debugMode = !debugMode;
            debugTextBox.Visibility = debugMode ? Visibility.Visible : Visibility.Collapsed;
        }

        // Methode om random kleurencode te genereren
        private string generateRandomColorCode()
        {
            // Beschikbare kleuren
            string[] colors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
            Random random = new Random();
            string[] randomColors = new string[4];

            // Genereer vier willekeurige kleuren
            for (int i = 0; i < 4; i++)
            {
                randomColors[i] = colors[random.Next(colors.Length)];
            }

            // Combineer de kleuren met een scheidingsteken
            return string.Join(" - ", randomColors);
        }

        // Methode om de achtergrondkleur te tonen
        private void updateLabel(Label label, string color)
        {
            label.Content = color;
            label.Background = color switch
            {
                "Rood" => Brushes.Red,
                "Geel" => Brushes.Yellow,
                "Oranje" => Brushes.Orange,
                "Wit" => Brushes.White,
                "Groen" => Brushes.Green,
                "Blauw" => Brushes.Blue,
                _ => Brushes.Transparent
            };
        }

        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            // Haal de geselecteerde kleuren op uit de comboBoxen
            string[] selectedColors = {
                comboBox1.SelectedItem?.ToString(),
                comboBox2.SelectedItem?.ToString(),
                comboBox3.SelectedItem?.ToString(),
                comboBox4.SelectedItem?.ToString()
            };

            // Haal de gegenereerde kleurencode op
            string[] targetColors = targetColorCode.Split(" - ");

            // Controleer of de geselecteerde kleuren overeenkomen met de gegenereerde code
            if (selectedColors.SequenceEqual(targetColors))
            {
                stopCountdown();
                MessageBox.Show("Code gekraakt in " + attempts + " pogingen", "WINNER");
                this.Close();
                return;
            }

            // Maak een array van booleans om bij te houden welke kleuren al als correct gemarkeerd zijn
            bool[] correctPositions = new bool[4];
            Array.Fill(correctPositions, false);

            // Controleer de geselecteerde kleuren en pas de randkleur aan
            updateBorder(label1, selectedColors[0], targetColors, 0, ref correctPositions);
            updateBorder(label2, selectedColors[1], targetColors, 1, ref correctPositions);
            updateBorder(label3, selectedColors[2], targetColors, 2, ref correctPositions);
            updateBorder(label4, selectedColors[3], targetColors, 3, ref correctPositions);

            // Verhoog het aantal pogingen en update de titel
            attempts++;
            updateWindowTitle();

            // Start de timer opnieuw bij een poging
            startCountdown();
        }

        // Methode om de randen de juiste kleur te geven op basis van de ingevulde kleur
        private void updateBorder(Label label, string selectedColor, string[] targetColors, int index, ref bool[] correctPositions)
        {
            if (selectedColor == null)
            {
                label.BorderBrush = Brushes.Transparent;
                label.BorderThickness = new Thickness(0);
                return;
            }

            // Controleer of de kleur op de juiste plaats staat
            if (targetColors[index] == selectedColor)
            {
                label.BorderBrush = Brushes.Red; // Rood = correcte plaats
                label.BorderThickness = new Thickness(4);
                correctPositions[index] = true; // Markeer deze positie als correct
            }
            else if (Array.Exists(targetColors, color => color == selectedColor))
            {
                // Controleer of de kleur ergens anders voorkomt en nog niet als correct is gemarkeerd
                bool alreadyMarked = false;

                for (int i = 0; i < targetColors.Length; i++)
                {
                    if (targetColors[i] == selectedColor && !correctPositions[i])
                    {
                        alreadyMarked = true;
                        break;
                    }
                }

                if (alreadyMarked)
                {
                    label.BorderBrush = Brushes.Yellow; // Geel = verkeerde plaats
                    label.BorderThickness = new Thickness(4);
                }
                else
                {
                    label.BorderBrush = Brushes.Transparent; // Geen effect
                    label.BorderThickness = new Thickness(0);
                }
            }
            else
            {
                label.BorderBrush = Brushes.Transparent; // Niet aanwezig in de code
                label.BorderThickness = new Thickness(0);
            }
        }

        // Methode om de window title te updaten
        private void updateWindowTitle()
        {
            if (attempts >= 10)
            {
                MessageBox.Show("Je hebt verloren! De code was: " + targetColorCode, "FAILED");
                this.Close();
                return;
            }
            else
            {
                this.Title = $"Poging {attempts} - Tijd: {remainingTime} seconden";
            }
        }

        // Timer Tick-event
        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                stopCountdown();
            }

            // Controleer of het spel al gewonnen is
            if (this.IsActive == false)
            {
                timer.Stop();
                return;
            }

            updateWindowTitle();
        }

        /// <summary>
        /// Start de countdown-timer van 10 seconden.
        /// Wordt aangeroepen telkens wanneer een nieuwe poging begint of wanneer een nieuwe code wordt gegenereerd.
        /// Reset de resterende tijd en activeert de timer om het Tick-event te verwerken.
        /// </summary>
        private void startCountdown()
        {
            remainingTime = 10;
            timer.Start();
        }

        /// <summary>
        /// Stopt de countdown-timer wanneer de tijd op is.
        /// Wordt automatisch aangeroepen door het Tick-event van de timer.
        /// Informeert de speler dat de beurt verloren is en verhoogt het aantal pogingen.
        /// </summary>
        private void stopCountdown()
        {

            // Controleer of het maximum aantal pogingen is bereikt
            if (attempts >= 10)
            {
                MessageBox.Show("Je hebt verloren! De code was: " + targetColorCode, "FAILED");
                this.Close();
                return;
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Tijd voorbij! Je hebt je beurt verloren.", "Beurt Verloren", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Verhoog het aantal pogingen
            attempts++;

            startCountdown();

            updateWindowTitle();
        }
    }
}