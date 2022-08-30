using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        // 7 izbor ambijentalne komponente
        string[] boje = {"crvena", "zelena", "plava", "zuta", "ljubicasta", "narandzasta"};

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Computer"), "PC.3DS", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_world.AnimacijaPokrenuta)
            {
                switch (e.Key)
                {
                    case Key.F4: this.Close(); break;
                    case Key.W:
                        if (m_world.RotationX > 0)
                            m_world.RotationX -= 5.0f;
                        break;
                    case Key.S:
                        if (m_world.RotationX < 90)
                            m_world.RotationX += 5.0f;
                        break;
                    case Key.A: m_world.RotationY -= 5.0f; break;
                    case Key.D: m_world.RotationY += 5.0f; break;
                    case Key.Add: m_world.SceneDistance -= 700.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 700.0f; break;
                    case Key.F2:
                        OpenFileDialog opfModel = new OpenFileDialog();
                        bool result = (bool)opfModel.ShowDialog();
                        if (result)
                        {

                            try
                            {
                                World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                                m_world.Dispose();
                                m_world = newWorld;
                                m_world.Initialize(openGLControl.OpenGL);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                            }
                        }
                        break;
                    case Key.C:
                        if (!m_world.AnimacijaPokrenuta)
                        {
                            m_world.pokreniAnimaciju();
                        }
                        break;
                }
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxVelicinaRacunara.ItemsSource = m_world.VelicineRacunara;
            comboBoxIzborAmbijentalne.ItemsSource = boje;
        }

        private void comboBoxVelicinaRacunara_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxVelicinaRacunara.SelectedIndex != 0 && !m_world.AnimacijaPokrenuta)
            {
                m_world.FaktorSkaliranja = (float)(comboBoxVelicinaRacunara.SelectedItem);
            }
        }

        private void dugmePomeriLevo_Click(object sender, RoutedEventArgs e)
        {
            if(m_world.Pomeraj > -610 && !m_world.AnimacijaPokrenuta)
            {
                m_world.Pomeraj = m_world.Pomeraj - 20;
            }
            
        }

        private void dugmePomeriDesno_Click(object sender, RoutedEventArgs e)
        {
         if(m_world.Pomeraj < 610 && !m_world.AnimacijaPokrenuta)
            {
                m_world.Pomeraj = m_world.Pomeraj + 20;
            }   
        }

        private void comboBoxIzborAmbijentalne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!m_world.AnimacijaPokrenuta)
            {
                if ((String)comboBoxIzborAmbijentalne.SelectedItem == "crvena")
                {
                    m_world.AmbijentalnaReflektor[0] = 1f;
                    m_world.AmbijentalnaReflektor[1] = 0f;
                    m_world.AmbijentalnaReflektor[2] = 0f;
                }
                else if ((String)comboBoxIzborAmbijentalne.SelectedItem == "zelena")
                {
                    m_world.AmbijentalnaReflektor[0] = 0f;
                    m_world.AmbijentalnaReflektor[1] = 1f;
                    m_world.AmbijentalnaReflektor[2] = 0f;
                }
                else if ((String)comboBoxIzborAmbijentalne.SelectedItem == "plava")
                {
                    m_world.AmbijentalnaReflektor[0] = 0f;
                    m_world.AmbijentalnaReflektor[1] = 0f;
                    m_world.AmbijentalnaReflektor[2] = 1f;
                }
                else if ((String)comboBoxIzborAmbijentalne.SelectedItem == "zuta")
                {
                    m_world.AmbijentalnaReflektor[0] = 1f;
                    m_world.AmbijentalnaReflektor[1] = 1f;
                    m_world.AmbijentalnaReflektor[2] = 0f;
                }
                else if ((String)comboBoxIzborAmbijentalne.SelectedItem == "ljubicasta")
                {
                    m_world.AmbijentalnaReflektor[0] = 1f;
                    m_world.AmbijentalnaReflektor[1] = 0f;
                    m_world.AmbijentalnaReflektor[2] = 1f;
                }
                else if ((String)comboBoxIzborAmbijentalne.SelectedItem == "narandzasta")
                {
                    m_world.AmbijentalnaReflektor[0] = 1f;
                    m_world.AmbijentalnaReflektor[1] = 0.5f;
                    m_world.AmbijentalnaReflektor[2] = 0f;
                }
            }
            
        }

        private void checkBoxTackasto_Click(object sender, RoutedEventArgs e)
        {
            if (!m_world.AnimacijaPokrenuta)
            {
                if ((bool)checkBoxTackasto.IsChecked)
                {
                    m_world.UkljucenoTackasto = true;
                }
                else
                {
                    m_world.UkljucenoTackasto = false;
                }
            }
        }

        private void checkBoxReflektorsko_Click(object sender, RoutedEventArgs e)
        {
            if (!m_world.AnimacijaPokrenuta)
            {
                if ((bool)checkBoxReflektorsko.IsChecked)
                {
                    m_world.UkljucenoReflektorsko = true;
                }
                else
                {
                    m_world.UkljucenoReflektorsko = false;
                }
            }
        }
    }
}
