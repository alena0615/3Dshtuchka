using System.Drawing.Drawing2D;

namespace Image3D;

public class MainForm : Form
{
    Bitmap? img;
    HeightMap? map;
    Point lastMouse;
    readonly Camera camera = new();
    readonly Pen pen = new(Color.Pink, 1);
    readonly FlowLayoutPanel panel1 = new() { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(67, 67, 67), WrapContents = false, Padding = new Padding(6, 8, 6, 0) };
    readonly TrackBar trackBar1 = new() { Width = 160, Minimum = 20, Maximum = 150, Value = 60, TickStyle = TickStyle.None };
    readonly TrackBar trackBar2 = new() { Width = 160, Maximum = 100, Value = 25, TickStyle = TickStyle.None };
    readonly CheckBox checkBox1 = Box("Цвет", true), checkBox2 = Box("инверсия"), checkBox3 = Box("вид сверху");

    public MainForm()
    {
        Text = "пепе 3D";
        ClientSize = new Size(1100, 700);
        BackColor = Color.Black;
        DoubleBuffered = true;
        Controls.Add(panel1);
        var button1 = new Button { Text = "отткрыть", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(10, 2, 10, 2), MinimumSize = new Size(0, 30), ForeColor = Color.White, FlatStyle = FlatStyle.System };
        panel1.Controls.AddRange(new Control[] { button1, Caption("детализация"), trackBar1, Caption("высота"), trackBar2, checkBox1, checkBox2, checkBox3 });

        button1.Click += (s, e) => OpenFile();
        trackBar1.Scroll += (s, e) => { MakeGrid(); Fit(true); };
        trackBar2.Scroll += (s, e) => { camera.ZScale = trackBar2.Value / 25.0; Invalidate(); };
        checkBox1.CheckedChanged += (s, e) => Invalidate();
        checkBox2.CheckedChanged += (s, e) => { MakeGrid(); Invalidate(); };
        checkBox3.CheckedChanged += (s, e) => { camera.TopView = checkBox3.Checked; Invalidate(); };
        MouseDown += (s, e) => lastMouse = e.Location;
        MouseMove += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                camera.Rotation += (e.X - lastMouse.X) * 0.01;
                camera.Elevation += (e.Y - lastMouse.Y) * 0.01;
                Invalidate();
            }
            lastMouse = e.Location;
        };
        MouseWheel += (s, e) => { camera.Zoom(e.Delta > 0 ? 1.1 : 1 / 1.1, e.X, e.Y); Invalidate(); };
        Resize += (s, e) => Fit();
    }

    static Label Caption(string text) => new() { Text = text, AutoSize = true, ForeColor = Color.White, Margin = new Padding(14, 7, 4, 0) };

    static CheckBox Box(string text, bool on = false) => new() { Text = text, Checked = on, AutoSize = true, ForeColor = Color.White, Margin = new Padding(14, 6, 4, 0) };

    static void Error(string text) => MessageBox.Show(text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

    void OpenFile()
    {
        using var dlg = new OpenFileDialog { Filter = "Картинки и FDF|*.png;*.jpg;*.jpeg;*.bmp;*.fdf|Все файлы|*.*" };
        if (dlg.ShowDialog() == DialogResult.OK) Open(dlg.FileName);
    }

    void Open(string file)
    {
        bool fdf = Path.GetExtension(file).ToLower() == ".fdf";
        try
        {
            if (fdf) map = FdfLoader.Load(file);
            else
            {
                using var tmp = new Bitmap(file);
                var copy = new Bitmap(tmp);
                img?.Dispose();
                img = copy;
            }
        }
        catch (FdfError ex)
        {
            Error("ошибка в файле " + Path.GetFileName(file) + ":\n" + ex.Message);
            return;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException || !fdf && ex is ArgumentException)
        {
            Error((fdf ? "не получилось открыть файл: " : "не получилось открыть картинку: ") + ex.Message);
            return;
        }
        if (fdf)
        {
            img?.Dispose();
            img = null;
        }
        else
        {
            checkBox2.Checked = ImageLoader.IsLight(img!);
            MakeGrid();
        }
        Fit();
    }

    void MakeGrid()
    {
        if (img != null) map = ImageLoader.MakeGrid(img, trackBar1.Value, checkBox2.Checked);
    }

    void Fit(bool keepZoom = false)
    {
        if (map != null) camera.Fit(map, ClientSize.Width, ClientSize.Height - panel1.Height, panel1.Height, keepZoom);
        Invalidate();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        var (dx, dy) = keyData switch { Keys.Left => (-20, 0), Keys.Right => (20, 0), Keys.Up => (0, -20), Keys.Down => (0, 20), _ => (0, 0) };
        if (dx == 0 && dy == 0) return base.ProcessCmdKey(ref msg, keyData);
        camera.Move(dx, dy);
        Invalidate();
        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (map == null) return;
        e.Graphics.SmoothingMode = SmoothingMode.None;
        var pts = new PointF[map.Cols, map.Rows];
        foreach (var (i, j) in map)
        {
            var (x, y) = camera.Project(i, j, map.Heights[i, j]);
            pts[i, j] = new PointF((float)x, (float)y);
        }
        foreach (var (i, j) in map)
            foreach (var (i2, j2) in map.Neighbors(i, j))
                DrawLine(e.Graphics, pts[i, j], pts[i2, j2], map.Colors[i, j], map.Colors[i2, j2]);
    }

    void DrawLine(Graphics g, PointF a, PointF b, Color ca, Color cb)
    {
        if (new[] { a.X, a.Y, b.X, b.Y }.Any(v => Math.Abs(v) > 100000)) return;
        if (!checkBox1.Checked) ca = cb = Color.Pink;
        int steps = ca == cb || Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)) < 16 ? 1 : 8;
        for (int k = 1; k <= steps; k++)
        {
            float t0 = (float)(k - 1) / steps, t1 = (float)k / steps;
            var start = new PointF(a.X + (b.X - a.X) * t0, a.Y + (b.Y - a.Y) * t0);
            pen.Color = Camera.LerpColor(ca, cb, (k - 0.5) / steps);
            g.DrawLine(pen, k == 1 ? a : start, k == steps ? b : new PointF(a.X + (b.X - a.X) * t1, a.Y + (b.Y - a.Y) * t1));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) img?.Dispose();
        base.Dispose(disposing);
    }
}
