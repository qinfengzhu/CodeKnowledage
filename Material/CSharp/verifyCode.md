### 加法验证码

1. 核心代码
```
/// <summary>
/// 加法验证码
/// </summary>
internal class MvcCaptchaImage
{
    #region Static

    /// <summary>
    /// Gets the cached captcha.
    /// </summary>
    public static MvcCaptchaImage GetCachedCaptcha(string guid)
    {
        if (String.IsNullOrEmpty(guid))
            return null;
        var options = new MvcCaptchaOptions();
        return new MvcCaptchaImage(options, guid);
    }

    private static readonly string[] RandomFontFamily = { "arial", "arial black", "comic sans ms", "courier new", "estrangelo edessa", "franklin gothic medium", "georgia", "lucida console", "lucida sans unicode", "mangal", "microsoft sans serif", "palatino linotype", "sylfaen", "tahoma", "times new roman", "trebuchet ms", "verdana" };
    private static readonly Color[] RandomColor = { Color.Red, Color.Green, Color.Blue, Color.Black, Color.Purple, Color.Orange };

    #endregion

    private readonly Random _rand;

    #region Public Properties

    /// <summary>
    /// Returns a GUID that uniquely identifies this Captcha
    /// </summary>
    /// <value>The unique id.</value>
    internal string UniqueId { get; private set; }

    /// <summary>
    /// Gets the randomly generated Captcha text.
    /// </summary>
    /// <value>The text.</value>
    public string Text { get; private set; }

    public string Value { get; private set; }

    public MvcCaptchaOptions CaptchaOptions { get; set; }


    #endregion

    internal MvcCaptchaImage() : this(new MvcCaptchaOptions(),"pvcaptchaImage") { }

    internal MvcCaptchaImage(MvcCaptchaOptions options,string guid)
    {
        CaptchaOptions = options;
        UniqueId = guid;
        _rand = new Random();
        //Text = GenerateRandomText();
    }

    internal void ResetText()
    {
        Text = GenerateRandomText();
    }

    /// <summary>
    /// Returns a random font family from the font whitelist
    /// </summary>
    private string GetRandomFontFamily()
    {
        return RandomFontFamily[_rand.Next(0, RandomFontFamily.Length)];
    }

    /// <summary>
    /// generate random text for the CAPTCHA
    /// </summary>
    private string GenerateRandomTextOld()
    {
        string txtChars = CaptchaOptions.TextChars;
        if (string.IsNullOrEmpty(txtChars))
            txtChars = "ACDEFGHJKLMNPQRSTUVWXYZ2346789";
        var sb = new StringBuilder(CaptchaOptions.TextLength);
        int maxLength = txtChars.Length;
        for (int n = 0; n <= CaptchaOptions.TextLength - 1; n++)
            sb.Append(txtChars.Substring(_rand.Next(maxLength), 1));


        Value = sb.ToString();

        return sb.ToString();
    }

    private string GenerateRandomText()
    {
        Random ran = new Random();
        int RandKey = ran.Next(55, 99);

        int Fandkey = ran.Next(1, 54);

        int LastKey = RandKey - Fandkey;

        int Switch = ran.Next(1, 9);

        Value = RandKey.ToString();

        string str = "";

        if (Switch > 5)
        { str = Fandkey + "加" + LastKey; }
        else
        { str = LastKey + "加" + Fandkey; }

        return str;
    }
    /// <summary>
    /// Returns a random point within the specified x and y ranges
    /// </summary>
    private PointF RandomPoint(int xmin, int xmax, int ymin, int ymax)
    {
        //return new PointF(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax));
        return new PointF(xmin, _rand.Next(ymin, ymax));
    }

    /// <summary>
    /// Get Random color.
    /// </summary>
    private Color GetRandomColor()
    {
        return RandomColor[_rand.Next(0, RandomColor.Length)];
    }

    /// <summary>
    /// Returns a random point within the specified rectangle
    /// </summary>  
    private PointF RandomPoint(Rectangle rect)
    {
        return RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom);
    }

    /// <summary>
    /// Returns a GraphicsPath containing the specified string and font
    /// </summary>  
    private static GraphicsPath TextPath(string s, Font f, Rectangle r)
    {
        var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        var gp = new GraphicsPath();
        gp.AddString(s, f.FontFamily, (int)f.Style, f.Size, r, sf);
        return gp;
    }

    /// <summary>
    /// Returns the CAPTCHA font in an appropriate size
    /// </summary>
    private Font GetFont()
    {
        float fsize;
        string fname = GetRandomFontFamily();

        switch (CaptchaOptions.FontWarp)
        {
            case Level.Low:
                fsize = Convert.ToInt32(CaptchaOptions.Height * 0.8);
                break;
            case Level.Medium:
                fsize = Convert.ToInt32(CaptchaOptions.Height * 0.85);
                break;
            case Level.High:
                fsize = Convert.ToInt32(CaptchaOptions.Height * 0.9);
                break;
            case Level.Extreme:
                fsize = Convert.ToInt32(CaptchaOptions.Height * 0.95);
                break;
            default:
                fsize = Convert.ToInt32(CaptchaOptions.Height * 0.7);
                break;
        }
        return new Font(fname, fsize, FontStyle.Bold);
    }

    /// <summary>
    /// Renders the CAPTCHA image
    /// </summary>
    internal Bitmap RenderImage()
    {
        var bmp = new Bitmap(CaptchaOptions.Width, CaptchaOptions.Height, PixelFormat.Format24bppRgb);

        using (var gr = Graphics.FromImage(bmp))
        {
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.Clear(Color.White);

            int charOffset = 0;
            double charWidth = CaptchaOptions.Width / CaptchaOptions.TextLength;
            Rectangle rectChar;

            foreach (char c in Text)
            {
                // establish font and draw area
                using (Font fnt = GetFont())
                {
                    using (Brush fontBrush = new SolidBrush(GetRandomColor()))
                    {
                        rectChar = new Rectangle(Convert.ToInt32(charOffset * charWidth), 0, Convert.ToInt32(charWidth), CaptchaOptions.Height);
                        var tfont = fnt;

                        if (c.ToString() == "+" || c.ToString() == "加")
                        {
                            // fnt.Name=RandomFontFamily[0];
                            tfont = new Font(RandomFontFamily[0], Convert.ToInt32(CaptchaOptions.Height * 0.7), FontStyle.Bold);
                        }
                        // warp the character
                        GraphicsPath gp = TextPath(c.ToString(), tfont, rectChar);
                        WarpText(gp, rectChar);
                        // draw the character
                        gr.FillPath(fontBrush, gp);
                        charOffset += 1;
                    }
                }
            }

            var rect = new Rectangle(new Point(0, 0), bmp.Size);
            AddNoise(gr, rect);
            AddLine(gr, rect);
        }
        return bmp;
    }

    /// <summary>
    /// Warp the provided text GraphicsPath by a variable amount
    /// </summary>
    /// <param name="textPath">The text path.</param>
    /// <param name="rect">The rect.</param>
    private void WarpText(GraphicsPath textPath, Rectangle rect)
    {
        float warpDivisor;
        float rangeModifier;

        switch (CaptchaOptions.FontWarp)
        {
            case Level.Low:
                warpDivisor = 6F;
                rangeModifier = 1F;
                break;
            case Level.Medium:
                warpDivisor = 5F;
                rangeModifier = 1.3F;
                break;
            case Level.High:
                warpDivisor = 4.5F;
                rangeModifier = 1.4F;
                break;
            case Level.Extreme:
                warpDivisor = 4F;
                rangeModifier = 1.5F;
                break;
            default:
                return;
        }

        var rectF = new RectangleF(Convert.ToSingle(rect.Left), 0, Convert.ToSingle(rect.Width), rect.Height);

        int hrange = Convert.ToInt32(rect.Height / warpDivisor);
        int wrange = Convert.ToInt32(rect.Width / warpDivisor);
        int left = rect.Left - Convert.ToInt32(wrange * rangeModifier);
        int top = rect.Top - Convert.ToInt32(hrange * rangeModifier);
        int width = rect.Left + rect.Width + Convert.ToInt32(wrange * rangeModifier);
        int height = rect.Top + rect.Height + Convert.ToInt32(hrange * rangeModifier);

        if (left < 0)
            left = 0;
        if (top < 0)
            top = 0;
        if (width > CaptchaOptions.Width)
            width = CaptchaOptions.Width;
        if (height > CaptchaOptions.Height)
            height = CaptchaOptions.Height;

        PointF leftTop = RandomPoint(left, left + wrange, top, top + hrange);
        PointF rightTop = RandomPoint(width - wrange, width, top, top + hrange);
        PointF leftBottom = RandomPoint(left, left + wrange, height - hrange, height);
        PointF rightBottom = RandomPoint(width - wrange, width, height - hrange, height);

        var points = new[] { leftTop, rightTop, leftBottom, rightBottom };
        var m = new Matrix();
        m.Translate(0, 0);
        textPath.Warp(points, rectF, m, WarpMode.Perspective, 0);
    }


    /// <summary>
    /// Add a variable level of graphic noise to the image
    /// </summary>
    private void AddNoise(Graphics g, Rectangle rect)
    {
        int density;
        int size;

        switch (CaptchaOptions.BackgroundNoise)
        {
            case Level.None:
                goto default;
            case Level.Low:
                density = 30;
                size = 40;
                break;
            case Level.Medium:
                density = 18;
                size = 40;
                break;
            case Level.High:
                density = 16;
                size = 39;
                break;
            case Level.Extreme:
                density = 12;
                size = 38;
                break;
            default:
                return;
        }
        var br = new SolidBrush(GetRandomColor());
        int max = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / size);
        for (int i = 0; i <= Convert.ToInt32((rect.Width * rect.Height) / density); i++)
            g.FillEllipse(br, _rand.Next(rect.Width), _rand.Next(rect.Height), _rand.Next(max), _rand.Next(max));
        br.Dispose();
    }

    /// <summary>
    /// Add variable level of curved lines to the image
    /// </summary>
    private void AddLine(Graphics g, Rectangle rect)
    {
        int length;
        float width;
        int linecount;

        switch (CaptchaOptions.LineNoise)
        {
            case Level.None:
                goto default;
            case Level.Low:
                length = 4;
                width = Convert.ToSingle(CaptchaOptions.Height / 31.25);
                linecount = 1;
                break;
            case Level.Medium:
                length = 5;
                width = Convert.ToSingle(CaptchaOptions.Height / 27.7777);
                linecount = 1;
                break;
            case Level.High:
                length = 3;
                width = Convert.ToSingle(CaptchaOptions.Height / 25);
                linecount = 2;
                break;
            case Level.Extreme:
                length = 3;
                width = Convert.ToSingle(CaptchaOptions.Height / 22.7272);
                linecount = 3;
                break;
            default:
                return;
        }

        var pf = new PointF[length + 1];
        using (var p = new Pen(GetRandomColor(), width))
        {
            for (int l = 1; l <= linecount; l++)
            {
                for (int i = 0; i <= length; i++)
                    pf[i] = RandomPoint(rect);

                g.DrawCurve(p, pf, 1.75F);
            }
        }
    }
}

internal class MvcCaptchaOptions
{
    #region private fields

    private int _width;
    private int _height;
    private int _length;
    private string _chars;

    #endregion

    #region public properties

    /// <summary>
    /// 验证字符长度（字符个数）
    /// </summary>
    public int TextLength
    {
        get { return _length; }
        set { _length = value < 3 ? 3 : value; }
    }

    /// <summary>
    /// 生成验证码用的字符
    /// </summary>
    public string TextChars
    {
        get { return _chars; }
        set { _chars = (string.IsNullOrEmpty(value) || value.Trim().Length < 3) ? "ACDEFGHJKLMNPQRSTUVWXYZ2346789" : value.Trim(); }
    }

    /// <summary>
    /// Font warp factor
    /// </summary>
    public Level FontWarp { get; set; }

    /// <summary>
    /// Background Noise level
    /// </summary>
    public Level BackgroundNoise { get; set; }

    /// <summary>
    /// 线条杂色级别
    /// </summary>
    public Level LineNoise { get; set; }


    /// <summary>
    /// Width of captcha image
    /// </summary>
    public int Width
    {
        get { return _width; }
        set { _width = value < (TextLength * 18) ? TextLength * 18 : value; }
    }

    /// <summary>
    /// Height of captcha image
    /// </summary>
    public int Height
    {
        get { return _height; }
        set
        {
            _height = value < 32 ? 32 : value;
        }
    }

    private string _inputBoxId;

    /// <summary>
    /// 客户端验证码输入文本框的Id
    /// </summary>
    public string ValidationInputBoxId
    {
        get
        {
            if (DelayLoad && string.IsNullOrEmpty(_inputBoxId))
                throw new ArgumentNullException("ValidationInputBoxId", "设置DelayLoad为true时必须指定ValidationInputBoxId的值");
            return _inputBoxId;
        }
        set
        {
            _inputBoxId = value;
        }
    }

    private string _captchaImageContainerId;
    public string CaptchaImageContainerId
    {
        get
        {
            if (DelayLoad && string.IsNullOrEmpty(_captchaImageContainerId))
                throw new ArgumentNullException("CaptchaImageContainerId",
                                                "设置DelayLoad为true时必须指定CaptchaImageContainerId的值");
            return _captchaImageContainerId;
        }
        set { _captchaImageContainerId = value; }
    }

    public string ReloadLinkText
    {
        get; set;
    }

    /// <summary>
    /// 是否延迟加载（验证文本框获得焦点时才生成并加载验证图片）
    /// </summary>
    public bool DelayLoad { get; set; }

    #endregion

    #region constructor

    public MvcCaptchaOptions()
    {
        FontWarp = Level.Low;
        BackgroundNoise = Level.Low;
        LineNoise = Level.Low;
        ReloadLinkText = "换一张";
        Width = 100;
        Height = 32;
        TextLength = 7;
    }

    #endregion
}
internal enum Level
{
    None,
    Low,
    Medium,
    High,
    Extreme
}
```

2. 具体调用
```
var ci = MvcCaptchaImage.GetCachedCaptcha(guid);
#region 构建图片流
ci.ResetText();
string value = ci.Value;
Cache.Set(guid,int.Parse(value), 5); //实现跨pv服务器缓存图片计算值,方便下次从缓存中根据guid获取
byte[] imageBuffer;
using (var b = ci.RenderImage())
{
    using (MemoryStream stream = new MemoryStream())
    {
        b.Save(stream, ImageFormat.Gif);              
        imageBuffer = stream.ToArray();
    }
}
var respimg = new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new System.Net.Http.ByteArrayContent(imageBuffer),
};
respimg.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");
#endregion
```
