// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Collections;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Diska
        /// </summary>
        private float m_diskRotation = 30f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 3000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        /// <summary>
        ///	 Identifikatori tekstura za jednostavniji pristup teksturama
        /// </summary>
        private enum TextureObjects {Drvo = 0,  Tepih};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "..//..//images//drvo.jpg", "..//..//images//tepih.jpg" };

        // Parametri za animaciju
        private DispatcherTimer timer1;
        private Boolean animacijaPokrenuta;

        private float[] pozicijaDiska;
        private int broj_Okreta;

        // Atributi potrebni za tačku 7.
        private ArrayList velicineRacunara = new ArrayList();
        private float faktorSkaliranja;
        private float pomeraj;
        private float[] ambijentalnaReflektor;

        // Svetlosni izvori
        private Boolean ukljucenoTackasto;
        private Boolean ukljucenoReflektorsko;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public Boolean AnimacijaPokrenuta
        {
            get { return animacijaPokrenuta; }
            set { animacijaPokrenuta = value; }
        }

        public float[] PozicijaDiska
        {
            get { return pozicijaDiska; }
            set { pozicijaDiska = value; }
        }

        public ArrayList VelicineRacunara
        {
            get { return velicineRacunara; }
            set { velicineRacunara = value; }
        }

        public float FaktorSkaliranja
        {
            get { return faktorSkaliranja; }
            set { faktorSkaliranja = value; }
        }

        public float Pomeraj
        {
            get { return pomeraj; }
            set { pomeraj = value; }
        }

        public float[] AmbijentalnaReflektor
        {
            get { return ambijentalnaReflektor; }
            set { ambijentalnaReflektor = value; }
        }

        public Boolean UkljucenoTackasto
        {
            get { return ukljucenoTackasto; }
            set { ukljucenoTackasto = value; }
        }

        public Boolean UkljucenoReflektorsko
        {
            get { return ukljucenoReflektorsko; }
            set { ukljucenoReflektorsko = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
            animacijaPokrenuta = false;
            pozicijaDiska = new float[] { 300f, 3.0f, 0f };

            faktorSkaliranja = 1;
            float doli = 0.05f;
            while (doli < 1)
            {
                velicineRacunara.Add(doli);
                doli = doli + 0.05f;
                doli = (float)Math.Round(doli, 2);
            }
            float suma = 1;
            while(suma <= 10)
            {
                velicineRacunara.Add(suma);
                suma = suma + 0.5f;
            }
            pomeraj = 0f;
            broj_Okreta = 0;
            ambijentalnaReflektor = new float[] { 1.0f, 0f, 0f };
            ukljucenoReflektorsko = false;
            ukljucenoTackasto = true;

        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            //1.
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            //2KT 1.
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            // Teksture
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            // Definiše način stapanja teksture sa materijalom - blending
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            // Filtriranje
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            // Wrapping -definiše kako se rukuje koordinatama teksture koje su van opsega 
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            // Učitavanje tekstura
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; i++)
            {
                // Pridruživanje teksture odgovarajućem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiranje slike zbog koordinatnog sistema openGL-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // Pretvaranje u RGB format (RGBA ako treba providnost, alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                // Kreiranje teksture sa RGB formatom
                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGB8, image.Width, image.Height, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                // Brisanje slike koja nam više ne treba jer smo napravili teksturu
                image.UnlockBits(imageData);
                image.Dispose();

            }

            m_scene.LoadScene();
            m_scene.Initialize();

        }
         

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            // 6. kamera
            gl.LookAt(-150f, 0f, -200f, 0f, 0f, -100f, 0f, 1f, 0f);

            SetupLightining(gl);
            // 9.
            SetupRedLightining(gl);

            // 7
            gl.PushMatrix();
            gl.Scale(faktorSkaliranja, faktorSkaliranja, faktorSkaliranja);
            gl.Translate(pomeraj, 0f, 0f);
            m_scene.Draw();
            gl.PopMatrix();

            //DEGUB za svetlo
            /*gl.PushMatrix();
            gl.Translate(0.0f, 1000.0f, 0.0f);
            gl.Scale(100f, 100f, 100f);
            //gl.Color(1f, 1f, 0f);
            Cylinder cylinderLamp = new Cylinder();
            cylinderLamp.CreateInContext(gl);
            cylinderLamp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();*/

            //Podloga
            gl.PushMatrix();
            gl.Translate(0f, -800f, 0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Tepih]);
            // 10.  
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            // Skaliranje teksture koristeći teksture matricu
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(3, 3, 3);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            // Kreiranje podloge
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0f, 1f, 0f); // ovo proveriti
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(2000.0f, 0.0f, -2000.0f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-2000.0f, 0.0f, -2000.0f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(-2000.0f, 0.0f, 2000.0f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2000.0f, 0.0f, 2000.0f);
            gl.End();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);  // Zbog 10. moram ovde vratiti na MODULATE da mi ne utiče i na sto
            // Za odvojene matrice moraju se praviti posebni pushevi i popovi - modelview i texture
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PopMatrix();
            //gl.Enable(OpenGL.GL_AUTO_NORMAL);// - automatsko generisanje normala ništa mi ne menja
            
            //Sto - površina
            gl.PushMatrix();
            //gl.Color(0.623529f, 0.623529f, 0.372549f);
            gl.Translate(0f, -30.0f, 0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Drvo]);
            Cube cube = new Cube();
            gl.Scale(700.0f, 28.0f, 400.0f);
            gl.Enable(OpenGL.GL_NORMALIZE);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            
            //Sto - noge
            gl.PushMatrix();
            gl.Translate(600f, -400f, 0f);
            gl.Scale(25f, 380f, 300f);
            Cube cube1 = new Cube();
            cube1.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-600f, -400f, 0f);
            gl.Scale(25f, 380f, 300f);
            Cube cube2 = new Cube();
            cube2.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //Disk
            gl.PushMatrix();
            gl.Color(0.439216f, 0.858824f, 0.576471f);
            // Početna pozicija diska, menja se kad krene animacija
            gl.Translate(pozicijaDiska[0], pozicijaDiska[1], pozicijaDiska[2]);
            gl.Scale(50f, 0, 50f);
            gl.Rotate(90f, 0f, 0f);

            gl.FrontFace(OpenGL.GL_CW);
            Disk disk = new Disk();
            disk.InnerRadius = 0.2f;
            disk.OuterRadius = 1f;
            disk.Slices = 10;
            disk.Loops = 120;
            disk.CreateInContext(gl);
            gl.Rotate(m_diskRotation, 0, 0, 1);
            disk.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            //Morao sam ubaciti, jer mi se menja svuda orijentacija kad promenim kod diska (a suprotne su...)
            gl.FrontFace(OpenGL.GL_CCW);

            //2D tekst
            gl.PushMatrix();
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);
            gl.DrawText(m_width - 450, 120, 1.0f, 1.0f, 0.0f, "Tahoma", 10, "Predmet: Racunarska grafika");
            gl.DrawText(m_width - 450, 95, 1.0f, 1.0f, 0.0f, "Tahoma", 10, "Sk.god: 2019/20.");
            gl.DrawText(m_width - 450, 70, 1.0f, 1.0f, 0.0f, "Tahoma", 10, "Ime: Nikola ");
            gl.DrawText(m_width - 450, 45, 1.0f, 1.0f, 0.0f, "Tahoma", 10, "Prezime: Magdic");
            gl.DrawText(m_width - 450, 20, 1.0f, 1.0f, 0.0f, "Tahoma", 10, "Sifra zad: 6.2");
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();

            gl.PopMatrix();

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);  //1
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        private void SetupLightining(OpenGL gl)
        {
            float[] ambijentalnaKomponenta = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] difuznaKomponenta = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] spekularnaKomponenta = { 1.0f, 1.0f, 1.0f, 1.0f };
            //Pridruživanje komponenti svetlosnom izvoru
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, spekularnaKomponenta);
            //Podešavanje parametara tačkastog svetlosnog izvora, 180.0 cut-off ugao čini ovo reflektorsko osvetljenje tačkastim
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            //Uključuje proračun osvetljenja - rad sa svetlima (za isključivanje disable)
            gl.Enable(OpenGL.GL_LIGHTING);
            //Uključivanje svetlosnog izvora
            if (ukljucenoTackasto)
            {
                gl.Enable(OpenGL.GL_LIGHT0);
            }
            else
            {
                gl.Disable(OpenGL.GL_LIGHT0);
            }
            //Pozicioniranje svetlosnog izvora
            float[] pozicija = { 0.0f, 1000.0f, 0.0f, 1.0f };
            //Proveriti da li treba lightfv
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicija);

            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        // 9. Proveriti da li je dobro usmerena
        public void SetupRedLightining(OpenGL gl)
        {
            float[] ambijentalnaKomponenta = ambijentalnaReflektor;
            float[] difuznaKomponenta = { 1.0f, 0f, 0f };
            float[] smer = { 0.0f, -1.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30.0f);
            if (ukljucenoReflektorsko)
            {
                gl.Enable(OpenGL.GL_LIGHT1);
            }else
            {
                gl.Disable(OpenGL.GL_LIGHT1);
            }
            float[] pozicija = { 0.0f, 200f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija);
        }

        public void pokreniAnimaciju()
        {
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(5);
            timer1.Tick += new EventHandler(UpdateAnimation1);
            timer1.Start();
            animacijaPokrenuta = true;
            Pomeraj = 0;
            FaktorSkaliranja = 1;
        }

        // Podizanje diska sa stola
        public void UpdateAnimation1(object sender, EventArgs e)
        {
            if(pozicijaDiska[1] < 350)
            {
                pozicijaDiska[1] += 5;
                pozicijaDiska[2] += 5;
            }
            else
            {
                if (pozicijaDiska[0] > 0)
                {
                    pozicijaDiska[0] -= 5;
                }
                else
                {
                    timer1.Stop();
                    timer1 = new DispatcherTimer();
                    timer1.Interval = TimeSpan.FromMilliseconds(5);
                    timer1.Tick += new EventHandler(UpdateAnimation2);
                    timer1.Start();
                }
            }
            
        }

        // Rotiranje diska
        public void UpdateAnimation2(object sender, EventArgs e)
        {
            if(broj_Okreta < 100)
            {
                m_diskRotation += 5;
                broj_Okreta++;
            }
            else
            {
                timer1.Stop();
                timer1 = new DispatcherTimer();
                timer1.Interval = TimeSpan.FromMilliseconds(5);
                timer1.Tick += new EventHandler(UpdateAnimation3);
                timer1.Start();
            }
            
        }

        //Ubacivanje diska u čitač
        public void UpdateAnimation3(object sender, EventArgs e)
        {
            if (pozicijaDiska[2] > 120)
            {
                pozicijaDiska[2] -= 5;
            }
            else
            {
                timer1.Stop();
                timer1 = new DispatcherTimer();
                timer1.Interval = TimeSpan.FromMilliseconds(5);
                timer1.Tick += new EventHandler(UpdateAnimation4);
                timer1.Start();
            }

        }

        //Izbacivanje diska iz čitača
        public void UpdateAnimation4(object sender, EventArgs e)
        {
            if (pozicijaDiska[2] < 350)
            {
                pozicijaDiska[2] += 5;
            }
            else
            {
                if(pozicijaDiska[0] < 300)
                {
                    pozicijaDiska[0] += 5;
                }
                else
                {
                    timer1.Stop();
                    timer1 = new DispatcherTimer();
                    timer1.Interval = TimeSpan.FromMilliseconds(5);
                    timer1.Tick += new EventHandler(UpdateAnimation5);
                    timer1.Start();
                }
            }

        }

        // Vraćanje diska na sto
        public void UpdateAnimation5(object sender, EventArgs e)
        {
            if(pozicijaDiska[2] > 5)
            {
                pozicijaDiska[1] -= 5;
                pozicijaDiska[2] -= 5;
            }
            else
            {
                timer1.Stop();
                animacijaPokrenuta = false;
                broj_Okreta = 0;
            }
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
