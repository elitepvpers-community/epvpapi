namespace epvpapi
{
    /// <summary>
    /// Represents a chat smiley that can be used in messages
    /// </summary>
    public class Smiley
    {
        /// <summary>
        /// Default root directory where smilies are stored
        /// </summary>
        private static string _DefaultDirectory = "http://www.elitepvpers.com/forum/images/smilies/";
        public static string DefaultDirectory
        {
            get { return _DefaultDirectory; }
            set { _DefaultDirectory = value; }
        }

        /// <summary>
        /// Path (URL) to the <c>Smiley</c>
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// File name and extension
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// String that triggers the interpretation of text which interprets the text as smiley
        /// </summary>
        public string Interpretation { get; set; }

        public Smiley(string file, string interpretation) :
            this(file, DefaultDirectory + file, interpretation)
        { }

        public Smiley(string file, string path, string interpretation)
        {
            File = file;
            Path = path;
            Interpretation = interpretation;
        }

        public static Smiley BigGrin
        {
            get { return new Smiley("biggrin.gif", ":D"); }
        }

        public static Smiley Awesome
        {
            get { return new Smiley("awesome.gif", ":awesome:"); }
        }

        public static Smiley Frown
        {
            get { return new Smiley("frown.gif", ":("); }
        }

        public static Smiley Smile
        {
            get { return new Smiley("smile.gif", ":)"); }
        }

        public static Smiley RedFace
        {
            get { return new Smiley("redface.gif", ":o"); }
        }

        public static Smiley Wink
        {
            get { return new Smiley("wink.gif", ";)"); }
        }

        public static Smiley Tongue
        {
            get { return new Smiley("tongue.gif", ":p"); }
        }

        public static Smiley Cool
        {
            get { return new Smiley("cool.gif", ":cool:"); }
        }

        public static Smiley RollEyes
        {
            get { return new Smiley("rolleyes.gif", ":rolleyes:"); }
        }

        public static Smiley Mad
        {
            get { return new Smiley("mad.gif", ":mad:"); }
        }

        public static Smiley Eek
        {
            get { return new Smiley("eek.gif", ":eek:"); }
        }

        public static Smiley Confused
        {
            get { return new Smiley("confused.gif", ":confused:"); }
        }

        public static Smiley RTFM
        {
            get { return new Smiley("rtfm.gif", ":rtfm:"); }
        }

        public static Smiley Pimp
        {
            get { return new Smiley("pimp.gif", ":pimp"); }
        }

        public static Smiley Mofo
        {
            get { return new Smiley("mofo.gif", ":mofo:"); }
        }

        public static Smiley HandsDown
        {
            get { return new Smiley("handsdown.gif", ":handsdown:"); }
        }

        public static Smiley Bandit
        {
            get { return new Smiley("bandit.gif", ":bandit:"); }
        }

        public static Smiley Facepalm
        {
            get { return new Smiley("facepalm.gif", ":facepalm:"); }
        }

        public override string ToString()
        {
            return Interpretation;
        }
    }
}
