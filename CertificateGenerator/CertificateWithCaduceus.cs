using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Svg.Converter;
using System.Diagnostics;
using System.IO;

namespace CertificateGenerator
{
    internal class CertificateWithCaduceus
    {
        static void GenerateCertificateWithCaduceus(string[] args)
        {
            string dest = "Certificate_WithCaduceus.pdf";
            string svgTopPath = "waves_top.svg";
            string svgBottomPath = "waves_bottom.svg";
            string logoPath = "caduceus_logo.svg";

            using (var writer = new PdfWriter(dest))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                var pageSize = pdf.GetDefaultPageSize();
                float width = pageSize.GetWidth();
                float height = pageSize.GetHeight();

                // === Logo Caduceus ===
                using (var logoStream = File.OpenRead(logoPath))
                {
                    var logo = SvgConverter.ConvertToImage(logoStream, pdf);
                    logo.SetFixedPosition(50, height - 130); // pozícia loga (x, y)
                    logo.SetWidth(80); // šírka loga
                    document.Add(logo);
                }

                // === Horné vlnovky ===
                using (var svgStream = File.OpenRead(svgTopPath))
                {
                    var svgTop = SvgConverter.ConvertToImage(svgStream, pdf);
                    svgTop.SetFixedPosition((width - 800) / 2, height - 200);
                    document.Add(svgTop);
                }

                // === Textový obsah ===
                document.Add(new Paragraph("\n\n\n\nCERTIFIKÁT O ABSOLVOVANÍ")
                    .SetFontSize(28)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(new DeviceRgb(0, 60, 130)));

                document.Add(new Paragraph("\nTento certifikát potvrdzuje, že")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("Karafová Mary")
                    .SetFontSize(22)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.BLACK));

                document.Add(new Paragraph("úspešne absolvoval(a) vzdelávacie podujatie\n„Moderné trendy v zdravotníctve 2025“")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\nDátum: 29.10.2025        Miesto: Bratislava")
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n\n______________________               ______________________\nLektor                                          Riaditeľ")
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER));

                // === Dolné vlnovky ===
                using (var svgStreamBottom = File.OpenRead(svgBottomPath))
                {
                    var page = pdf.GetLastPage();
                    var canvas = new PdfCanvas(page);

                    canvas.SaveState();
                    canvas.ConcatMatrix(1, 0, 0, -1, (width - 800) / 2, 160);
                    SvgConverter.DrawOnCanvas(svgStreamBottom, canvas, 0, 0);
                    canvas.RestoreState();
                }

                // === Ozdobný rám ===
                var borderCanvas = new PdfCanvas(pdf.GetFirstPage());
                borderCanvas.SaveState();

                borderCanvas.SetLineWidth(1.2f);
                borderCanvas.SetStrokeColor(new DeviceRgb(0, 85, 170));

                float margin = 28f;  // cca 10 mm
                float cornerRadius = 16f;

                borderCanvas.RoundRectangle(margin, margin,
                    width - 2 * margin, height - 2 * margin, cornerRadius);
                borderCanvas.Stroke();
                borderCanvas.RestoreState();

                document.Close();
            }

            Debug.WriteLine("✅ Certifikát s logom Caduceus, rámom a vlnkami bol vytvorený: " + System.IO.Path.GetFullPath(dest));
        }
    }
}
