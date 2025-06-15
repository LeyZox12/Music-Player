using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using SFML.Window;
using System.Diagnostics;
using System.IO;
using TagLib;
using System.Security.AccessControl;


/// TO DO GET SONGS METADATA AND ADD COVER TO ALBUMS


namespace music_player_core
{
    internal static class Program
    {
        static SimpleWindow window = new SimpleWindow();
        static Font font = new Font("res\\font.otf");
        const String path = "C:\\Users\\solat\\Music";
        const int size = 200;
        const int spacing = 5;
        const int fontSize = 30;
        static string currentArtist = "None";
        static string currentAlbum = "None";
        static string currentSong = "None";
        static Music currentMusic;
        private static void PlaySong(string path)
        {
            if(currentMusic != null) currentMusic.Stop();
            currentMusic = new Music(path);
            
            currentMusic.Play();
            window.buttons.Clear();
            window.sliders.Add(new Slider(
                new RectangleShape((100, 5)) { FillColor = Color.White, Position = (412, 412) },
                new CircleShape(15) { FillColor = Color.White, Position = (412, 412) },
                0.5f));
            window.sliders.Add(new Slider(
                new RectangleShape((400, 5)) { FillColor = Color.White, Position = (56, 450) },
                new CircleShape(15) { FillColor = Color.White, Position = (412, 412) },
                0.0f));
        }
        private static void LoadSongUI()
        {
            window.buttons.Clear();
            //window.buttons.Add(new Button())
        }
        private static void ScanSongs(string path)
        {
            window.buttons.Clear();
            var songs = Directory.EnumerateFiles(path, "*.mp3");
            var i = 0;
            window.buttons.Add(new Button(
                new RectangleShape((100, 50))
                {
                    Position = (0, -55),
                    FillColor = Color.White
                },
                new Text()
                {
                    Font = font,
                    DisplayedString = "Go Back",
                    FillColor = Color.Black,
                    CharacterSize = 20,
                    Position = (0, -55)
                },
                () => { Console.WriteLine(path.Substring(0, path.Length - currentAlbum.Length)); ScanAlbums(path.Substring(0, path.Length - currentAlbum.Length));}));
            foreach (var song in songs)
            {

                var fixedSong = song.Substring(path.Length + 1);
                Console.WriteLine(fixedSong);
                window.buttons.Add(new Button(
                    new RectangleShape((fixedSong.Length * fontSize * 0.75f, fontSize * 1.5f))
                    {
                        Position = (0, i * fontSize * 1.7f),
                        FillColor = Color.White
                    }, new Text()
                    {
                        Font = font,
                        DisplayedString = fixedSong,
                        FillColor = Color.Black,
                        CharacterSize = fontSize,
                        Position = (0, i * fontSize * 1.7f)
                    },
                    () =>
                    {
                        PlaySong(song);
                        currentSong = fixedSong;
                    }));
                i++;
            }
        }
        private static void ScanAlbums(string path)
        {
            window.buttons.Clear();
            var albums = Directory.EnumerateDirectories(path);
            window.buttons.Add(new Button(
                new RectangleShape((100, 50)) 
                { 
                    Position = (0, -55), 
                    FillColor = Color.White 
                }, 
                new Text()
                { 
                    Font = font, 
                    DisplayedString = "Go Back", 
                    FillColor = Color.Black, 
                    CharacterSize = 20, 
                    Position = (0, -55) 
                }, 
                () => { ScanArtists();}));
            var i = 0;
            foreach (var album in albums)
            {
                var fixedAlbum = album.Substring(path.Length + 1);
                Console.WriteLine(album);
                var songsPath = Directory.EnumerateFiles(album);
                var firstSong = songsPath.First();
                Console.WriteLine(songsPath);

                var savePath = "res/" + fixedAlbum;
                var file = TagLib.File.Create(firstSong);
                var coverPath = "covers/" + fixedAlbum + ".jpg";

                foreach (IPicture picture in file.Tag.Pictures)
                {
                    System.IO.File.WriteAllBytes(coverPath, picture.Data.Data);
                }
                var img = new Texture("covers/failsafe.png");
                if (System.IO.File.Exists(coverPath))
                    img = new Texture(coverPath);
                window.buttons.Add(new Button(
                    new RectangleShape((size, size)) 
                    { 
                        Position = (0, i * (size+spacing)), 
                        Texture = img 
                    }, new Text() 
                    { 
                        Font = font, 
                        DisplayedString = fixedAlbum, 
                        FillColor = Color.White, 
                        CharacterSize = 20, 
                        Position = (size, i * (size + spacing)) }, 
                    () => 
                    {
                        ScanSongs(album);
                        currentAlbum = fixedAlbum;
                    }));
                i++;
            }
        }
        private static void ScanArtists()
        {
            window.buttons.Clear();
            Console.WriteLine("Scanning local 'music' folder");
            var artists = Directory.EnumerateDirectories(path);
            var i = 0;
            foreach (var artist in artists)
            {
                var fixedArtist = artist.Substring(path.Length+1);
                Console.WriteLine(fixedArtist);
                window.buttons.Add(new Button(
                    new RectangleShape((fixedArtist.Length * fontSize * 0.75f, fontSize * 1.5f))
                    { 
                        Position = (0, i * fontSize * 1.7f), 
                        FillColor = Color.White
                    }, new Text() 
                    { 
                        Font = font, 
                        DisplayedString = fixedArtist, 
                        FillColor = Color.Black, 
                        CharacterSize = fontSize, 
                        Position = (0, i * fontSize * 1.7f)
                    }, 
                    () =>
                    { 
                        currentArtist = fixedArtist; 
                        ScanAlbums(artist); 
                    }));
                i++;
            }
            Thread.Sleep(1000);
        }
        private static void Main()
        {

            var circle = new CircleShape(256)
            {
                FillColor = Color.White
            };
            
            var running = true;
            var setPathButton = new Button(
                new RectangleShape((110, 50)), 
                new Text() { Font = font, CharacterSize = 20, FillColor = Color.Black, DisplayedString = "Load music"}, 
                () => 
                {
                    ScanArtists();
                }
                );
            window.start();
            window.buttons.Add(setPathButton);
            while(running)
            {
                float buffer = 0;
                if (window.sliders.Count > 0)
                    buffer = window.sliders[1].value;
                window.Run();
                if (window.sliders.Count > 0)
                {
                    currentMusic.Volume = window.sliders[0].value * 100.0f;
                    if(buffer - window.sliders[1].value != 0)
                        currentMusic.PlayingOffset = SFML.System.Time.FromSeconds((window.sliders[1].value * currentMusic.Duration.AsSeconds()));
                    else
                        window.sliders[1].SetValue(currentMusic.PlayingOffset.AsSeconds() / currentMusic.Duration.AsSeconds());
                }
            }
        }
    }
    internal class Slider
    {
        public Slider(RectangleShape sprite, CircleShape knob, float value) 
        { 
            this.sprite = sprite;
            knob.Origin = (knob.Radius, knob.Radius);
            knob.Position = (sprite.Position.X + (sprite.Size.X) * value, sprite.Position.Y);
            this.knob = knob;
            this.value = value;
            grabbed = false;
        }
        private float clamp(float val, float minval, float maxval)
        {
            if(val < minval) return minval;
            if(val > maxval) return maxval;
            return val;
        }
        public void SetValue(float val)
        {
            this.value = val;
            knob.Position = new Vector2f(sprite.Position.X + (sprite.Size.X * value), knob.Position.Y);
        }
        public float UpdateValue(Vector2f mousepos)
        {
            Vector2f diff = new Vector2f(mousepos.X - knob.Position.X, mousepos.Y - knob.Position.Y);
            if (Double.Hypot(diff.X, diff.Y) < knob.Radius && mousepos.X > sprite.Position.X && mousepos.X < sprite.Position.X + sprite.Size.X || grabbed)
            {
                grabbed = true;
                knob.Position = new Vector2f(clamp(mousepos.X, sprite.Position.X, sprite.Position.X + sprite.Size.X), knob.Position.Y);
                value = (knob.Position.X - sprite.Position.X) / sprite.Size.X;
            }
            return this.value;
        }
        public RectangleShape sprite;
        public CircleShape knob;
        public float value;
        public bool grabbed;
    }
    internal class Button
    {
        public Button(RectangleShape Sprite, Text Text, Action OnClick)
        {
            this.Sprite = Sprite;
            this.Text = Text;
            this.Onclick = OnClick;
        }
        public RectangleShape Sprite { get; set; }
        public Text Text { get; set;}
        public Action Onclick { get; set;}
    }
    internal class SimpleWindow
    {
        static List<Drawable> waitingToDraw = new List<Drawable>();
        public List<Button> buttons = new List<Button>();
        public List<Slider> sliders = new List<Slider>();
        static RenderWindow window = new RenderWindow(new VideoMode(512, 512), "Music Player");
        static bool holding = false;
        const float SCROLL_SPEED = 40.0f;
        public void start()
        {
            window.MouseButtonReleased += Window_Release;
            window.MouseWheelScrolled += Window_Scroll;
            window.MouseButtonPressed += Window_Click;
            window.SetFramerateLimit(30);
        }
        public void Run()
        {
            
            window.DispatchEvents();
            window.Clear();
            if(holding)
            {
                for (int i = 0; i < sliders.Count; i++)
                {
                    sliders[i].UpdateValue(new Vector2f(Mouse.GetPosition(window).X, Mouse.GetPosition(window).Y));
                }
            }
            foreach (var element in waitingToDraw)
            {
                window.Draw(element);
            }
            foreach (var button in buttons)
            {
                if(button.Sprite.Position.Y > -button.Sprite.Size.Y && button.Sprite.Position.Y < 512+button.Sprite.Size.Y)
                {
                    window.Draw(button.Sprite);
                    window.Draw(button.Text);
                }
            }
            foreach(var slider in sliders)
            {
                window.Draw(slider.sprite);
                window.Draw(slider.knob);
            }
            waitingToDraw.Clear();
            window.Display();
        }

        public void AddToDraw(Drawable drawable)
        {
            waitingToDraw.Add(drawable);
        }
        private void Window_Release(object sender, MouseButtonEventArgs e)
        {
            holding = false;
            for(int i = 0; i < buttons.Count; i++)
            {
                var pos = buttons[i].Sprite.Position;
                var size = buttons[i].Sprite.Size;
                if (e.X > pos.X && e.Y > pos.Y && e.X < pos.X + size.X && e.Y < pos.Y + size.Y)
                {
                    buttons[i].Onclick.Invoke();
                    return;
                }
            }
            for(int i = 0; i < sliders.Count; i++)
                sliders[i].grabbed = false;
        }
        private void Window_Click(object sender, MouseButtonEventArgs e)
        {
            holding = true;
        }
        private void Window_Scroll(object sender, MouseWheelScrollEventArgs e)
        {
            for(int i = 0; i < buttons.Count;i++)
            {
                buttons[i].Sprite.Position += (0, e.Delta * SCROLL_SPEED);
                buttons[i].Text.Position += (0, e.Delta * SCROLL_SPEED);
            }
        }
    }
}