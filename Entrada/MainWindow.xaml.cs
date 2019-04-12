using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp; //procesamiento digital de señales

namespace Entrada
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn waveIn;
        public MainWindow()
        {
            InitializeComponent();
            LlenarComboDispositivos();
        }
        public void LlenarComboDispositivos()
        {
           for(int i=0; i < WaveIn.DeviceCount; i++)
            {
                WaveInCapabilities capacidades = WaveIn.GetCapabilities(i);
                cmbDispositivos.Items.Add(capacidades.ProductName);
            }
            cmbDispositivos.SelectedIndex = 0;
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            waveIn = new WaveIn();

            //Formato de audio
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
            //Buffer
            waveIn.BufferMilliseconds = 250;
            //¿Que hacer cuando hay muestras disponibles?
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();

        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;
            float acumulador = 0.0f;

            double numeroDeMuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroDeMuestrasComplejas = 0;
            int bitsMaximos = 0;

            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximos < numeroDeMuestras);
            exponente--;
            numeroDeMuestrasComplejas = bitsMaximos / 2;

            Complex[] señalCompleja = new Complex[numeroDeMuestrasComplejas];



            for (int i = 0; i<bytesGrabados; i+=2)
            {
                //<< es una instruccion de bajo nivel
                // Transformando 2 bytes separdos en una muestra de 16 bits
                //1.-Toma el segundo byte y le antepone 8 0's al inicio
                //2.- Hace un 0R con el primer byte, al cual automaticamente se le llenan 8 0's al final
                short muestra =
                    (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32bits = (float)muestra / 32768.0f;
                acumulador += Math.Abs(muestra32bits);
                
                if(i/2 < numeroDeMuestrasComplejas)
                {
                    //X es la parte real de los complex
                    //Y es la parte imaginaria
                    señalCompleja[i / 2].X = muestra32bits;
                }
                

                

            }
            float promedio = acumulador / (bytesGrabados/2.0f);
            sldMicrofono.Value = (double)promedio;

            //es parte de la libreria Naudio para obtener la transformada de fourier
            //el numero de muestras tiene que ser una potencia de 2, en el analisis en tiempo real se suele deshacer de las muestras sobrantes para realizar esto
            //tiempo a frecuencia forward true
            //frecuencia a tiempo forward false
            //m es el numero de muestras
            //FastFourierTransform.FFT()

        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }
    }
}
