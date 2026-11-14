using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BasketballRulesCms.App.Models;

namespace BasketballRulesCms.App.Services;

public static class SeedService
{
    private record SeedEntry(int No, string Title, string Body);

    private static readonly SeedEntry[] Seeds =
    {
        new(1, "Trajanje utakmice i produžeci",
            "Prema FIBA pravilniku, utakmica se igra u četiri četvrtine po 10 minuta. " +
            "Pauze između prve i druge, te treće i četvrte četvrtine traju po 2 minuta, " +
            "dok poluvrijeme traje 15 minuta. Ako je rezultat nerešen na kraju regularne igre, " +
            "igraju se produžeci od po 5 minuta sve dok se ne utvrdi pobednik."),

        new(2, "Lične greške i slobodna bacanja",
            "Lična greška je nedozvoljeni fizički kontakt s protivnikom. " +
            "Svaki igrač smije skupiti 5 ličnih grešaka (FIBA) — petom greška dovodi do diskvalifikacije. " +
            "Kada ekipa skupi 5 timskih grešaka u jednoj četvrtini, svaka naredna greška " +
            "daje protivniku dva slobodna bacanja."),

        new(3, "Kretanje s loptom — koraci i dvostruko vođenje",
            "Igrač koji drži loptu smije napraviti najviše dva koraka bez vođenja. " +
            "Više od dva koraka bez vođenja proglašava se greška 'koraci' (traveling). " +
            "Dvostruko vođenje nastaje kada igrač zaustavi vođenje pa ponovo počne."),

        new(4, "Pravilo 3 sekunde u reketu",
            "Napadački igrač ne smije boraviti u protivničkom reketu (trosekundna zona) " +
            "duže od 3 uzastopne sekunde dok njegova ekipa kontrolira loptu. " +
            "Kršenje ovog pravila daje loptu protivniku."),

        new(5, "Pravilo 24 sekunde — napadački sat",
            "Ekipa koja kontrolira loptu mora pokušati šut na koš u roku od 24 sekunde. " +
            "Sat se resetuje na 24 sekunde ako lopta dotakne obruč. " +
            "Ovo pravilo je uvedeno da bi se igra ubrzala i spriječilo čuvanje lopte."),
    };

    public static void EnsureSeedData()
    {
        AppPaths.EnsureDataFolders();
        var doc = AppServices.Rules.Load();
        if (doc.Items.Count > 0) return;

        var baseTime = DateTimeOffset.UtcNow.AddDays(-7);
        for (int i = 0; i < Seeds.Length; i++)
        {
            var s  = Seeds[i];
            var id = Guid.NewGuid();
            var imageRel = CreateSeedImage(id, s.No, s.Title);
            var rtfRel   = Path.Combine("Data", "rtf", $"{id:N}.rtf");

            doc.Items.Add(new BasketballRule
            {
                Id = id, RuleNumber = s.No, Title = s.Title,
                ImagePath = imageRel, RtfPath = rtfRel,
                AddedAt = baseTime.AddDays(i),
            });

            var flow = new FlowDocument { FontFamily = new FontFamily("Segoe UI"), FontSize = 14 };
            flow.Blocks.Add(new Paragraph(new Run(s.Body)));
            AppServices.Rtf.SaveFlowDocument(rtfRel, flow);
        }
        AppServices.Rules.Save(doc);
    }

    private static string CreateSeedImage(Guid id, int no, string title)
    {
        const int W = 640, H = 360;
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            var bg     = Frozen(0x15, 0x15, 0x15);
            var stripe = Frozen(0xFF, 0x6B, 0x00);
            var surf   = Frozen(0x1F, 0x1F, 0x1F);
            var muted  = Frozen(0x55, 0x55, 0x55);
            dc.DrawRectangle(bg, null, new Rect(0, 0, W, H));
            dc.DrawRectangle(stripe, null, new Rect(0, 0, 8, H));

            var ballPen = new Pen(stripe, 8); ballPen.Freeze();
            var linePen = new Pen(stripe, 5); linePen.Freeze();
            double cx = W - 100, cy = 88, r = 64;
            dc.DrawEllipse(surf, ballPen, new Point(cx, cy), r, r);
            dc.DrawLine(linePen, new Point(cx - r - 5, cy), new Point(cx + r + 5, cy));
            dc.DrawLine(linePen, new Point(cx, cy - r - 5), new Point(cx, cy + r + 5));
            DrawArc(dc, linePen, cx, cy, r, 200, 340);
            DrawArc(dc, linePen, cx, cy, r, 20,  160);

            var dpi = VisualTreeHelper.GetDpi(dv).PixelsPerDip;
            var badge = new SolidColorBrush(Color.FromArgb(200, 0xFF, 0x6B, 0x00)); badge.Freeze();
            dc.DrawRoundedRectangle(badge, null, new Rect(20, 16, 128, 40), 4, 4);
            DrawText(dc, $"PRAVILO {no}", "Impact", 21, Brushes.Black, new Point(26, 24), dpi);
            DrawText(dc, title, "Segoe UI", 22, Brushes.White, new Point(20, 72), dpi, W - 150);
            DrawText(dc, "Basketball Rules CMS — FIBA", "Segoe UI", 13, muted, new Point(20, H - 32), dpi);
            dc.DrawRectangle(stripe, null, new Rect(0, H - 5, W, 5));
        }

        var rtb = new RenderTargetBitmap(W, H, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(dv);

        var abs = Path.Combine(AppPaths.ImagesDirectory, $"{id:N}.png");
        using var fs = File.Create(abs);
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(rtb));
        enc.Save(fs);
        return AppPaths.ToRelativePath(abs);
    }

    private static SolidColorBrush Frozen(byte r, byte g, byte b)
    {
        var b2 = new SolidColorBrush(Color.FromRgb(r, g, b)); b2.Freeze(); return b2;
    }

    private static void DrawText(DrawingContext dc, string text, string font,
        double size, Brush fg, Point origin, double dpi, double maxW = 0)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, new Typeface(font), size, fg, dpi);
        if (maxW > 0) { ft.MaxTextWidth = maxW; ft.Trimming = TextTrimming.CharacterEllipsis; }
        dc.DrawText(ft, origin);
    }

    private static void DrawArc(DrawingContext dc, Pen pen,
        double cx, double cy, double r, double a0, double a1)
    {
        var s = Pt(cx, cy, r, a0); var e = Pt(cx, cy, r, a1);
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(s, false, false);
            ctx.ArcTo(e, new Size(r, r), 0, false, SweepDirection.Clockwise, true, false);
        }
        g.Freeze();
        dc.DrawGeometry(null, pen, g);
    }

    private static Point Pt(double cx, double cy, double r, double deg)
    {
        var rad = deg * Math.PI / 180;
        return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
    }
}
