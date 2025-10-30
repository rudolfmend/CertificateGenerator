using CertificateGenerator.Data;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Svg.Converter;
using System;
using System.Diagnostics;
using System.IO;

namespace CertificateGenerator.Helpers
{
    /// <summary>
    /// Generátor moderných PDF certifikátov s Caduceus dizajnom
    /// obsahujúci PNG logo, SVG vlnky a elegantný rám
    /// </summary>
    public static class CaduceusCertificateGenerator
    {
        /// <summary>
        /// Vygeneruje PDF certifikát s Caduceus dizajnom (vlnovky, logo, oblý rám)
        /// </summary>
        /// <param name="filePath">Cesta k výslednému PDF súboru</param>
        /// <param name="organizerName">Názov organizátora</param>
        /// <param name="eventTopic">Názov školenia/podujatia</param>
        /// <param name="eventDate">Dátum konania</param>
        /// <param name="participantName">Meno účastníka</param>
        /// <param name="eventLocation">Miesto konania (napr. "Bratislava")</param>
        /// <param name="pageSize">Veľkosť papiera (A4, Letter, atď.)</param>
        /// <param name="template">Šablóna s nastaveniami farieb a fontov (voliteľné)</param>
        public static void GenerateCaduceusCertificate(
            string filePath,
            string organizerName,
            string eventTopic,
            DateTime? eventDate,
            string participantName,
            string eventLocation = "Bratislava",
            PageSize pageSize = null,
            CertificateTemplateModel template = null)
        {
            // Default hodnoty
            pageSize = pageSize ?? PageSize.A4;
            template = template ?? GetDefaultCaduceusTemplate();

            // Cesty k resources
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string svgTopPath = System.IO.Path.Combine(projectDirectory, "Resources", "waves_top.svg");
            string svgBottomPath = System.IO.Path.Combine(projectDirectory, "Resources", "waves_bottom.svg");
            string logoPath = System.IO.Path.Combine(projectDirectory, "Resources", "caduceus_without_background.png");

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            {
                // Vytvor stránku PRED Document objektom
                pdf.AddNewPage(pageSize);

                using (var document = new Document(pdf))
                {
                    float width = pageSize.GetWidth();
                    float height = pageSize.GetHeight();

                    // Nastavenie okrajov
                    document.SetMargins(
                        template.MarginTop,
                        template.MarginRight,
                        template.MarginBottom,
                        template.MarginLeft
                    );

                    // Načítanie fontov
                    PdfFont titleFont = GetFont(template.TitleFontFamily);
                    PdfFont textFont = GetFont(template.TextFontFamily);

                    // === 1. OZDOBNÝ RÁM S OBLÝMI ROHMI ===
                    if (template.ShowBorder)
                    {
                        AddRoundedBorder(pdf, width, height, template);
                    }

                    // === 2. HORNÉ VLNOVKY ===
                    if (File.Exists(svgTopPath))
                    {
                        AddTopWaves(pdf, svgTopPath, width, height);
                    }

                    // === 3. LOGO CADUCEUS (PNG - ľavý horný roh) ===
                    if (File.Exists(logoPath))
                    {
                        AddCaduceusLogoPng(document, logoPath, width, height);
                    }

                    // === 4. TEXTOVÝ OBSAH ===
                    AddCertificateContent(
                        document,
                        titleFont,
                        textFont,
                        template,
                        organizerName,
                        eventTopic,
                        eventDate,
                        participantName,
                        eventLocation
                    );

                    // === 5. DOLNÉ VLNOVKY (zrkadlené) ===
                    if (File.Exists(svgBottomPath))
                    {
                        AddBottomWaves(pdf, svgBottomPath, width);
                    }
                }
            }
        }

        /// <summary>
        /// Pridá oblý decentný rám okolo certifikátu
        /// </summary>
        private static void AddRoundedBorder(PdfDocument pdf, float width, float height, CertificateTemplateModel template)
        {
            var borderCanvas = new PdfCanvas(pdf.GetFirstPage());
            borderCanvas.SaveState();

            // Nastavenie čiary
            float borderWidth = template.BorderWidth > 0 ? template.BorderWidth : 1.5f;
            borderCanvas.SetLineWidth(borderWidth);
            borderCanvas.SetStrokeColor(ParseColor(template.BorderColor ?? "#0055AA"));

            // Výpočet marginu a rádiusu rohov
            float margin = 25f;  // ~9 mm
            float cornerRadius = 15f;

            // Nakreslenie rámu s oblými rohmi
            borderCanvas.RoundRectangle(
                margin,
                margin,
                width - 2 * margin,
                height - 2 * margin,
                cornerRadius
            );
            borderCanvas.Stroke();
            borderCanvas.RestoreState();
        }

        /// <summary>
        /// Pridá PNG Caduceus logo do ľavého horného rohu
        /// </summary>
        private static void AddCaduceusLogoPng(Document document, string logoPath, float width, float height)
        {
            try
            {
                ImageData imageData = ImageDataFactory.Create(logoPath);
                Image logo = new Image(imageData);

                // Responzívna veľkosť (max 10% šírky stránky)
                float maxLogoWidth = width * 0.10f;
                float maxLogoHeight = 100f;

                logo.ScaleToFit(maxLogoWidth, maxLogoHeight);

                // Pozícia v ľavom hornom rohu
                float logoX = 45f;
                float logoY = height - 120f;

                logo.SetFixedPosition(logoX, logoY);
                document.Add(logo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Chyba pri načítaní PNG loga: {ex.Message}");
            }
        }

        /// <summary>
        /// Pridá horné dekoratívne vlnovky (tmavšie modré gradienty)
        /// </summary>
        private static void AddTopWaves(PdfDocument pdf, string svgTopPath, float width, float height)
        {
            try
            {
                var page = pdf.GetFirstPage();
                var canvas = new PdfCanvas(page);
                canvas.SaveState();

                // Responzívne škálovanie
                float svgWidth = Math.Min(800, width * 0.95f);
                float svgHeight = 150f;
                float scale = svgWidth / 800f;

                float xPos = (width - svgWidth) / 2;
                float yPos = height - svgHeight;

                // Matica transformácie pre škálovanie a pozíciu
                canvas.ConcatMatrix(scale, 0, 0, scale, xPos, yPos);

                using (var svgStream = File.OpenRead(svgTopPath))
                {
                    SvgConverter.DrawOnCanvas(svgStream, canvas, 0, 0);
                }

                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Chyba pri vykreslení horných vlniek: {ex.Message}");
            }
        }

        /// <summary>
        /// Pridá dolné dekoratívne vlnovky (svetlejšie modré, zrkadlené)
        /// </summary>
        private static void AddBottomWaves(PdfDocument pdf, string svgBottomPath, float width)
        {
            try
            {
                var page = pdf.GetLastPage();
                var canvas = new PdfCanvas(page);
                canvas.SaveState();

                // Responzívne škálovanie
                float svgWidth = Math.Min(800, width * 0.95f);
                float scale = svgWidth / 800f;

                float xPos = (width - svgWidth) / 2;
                float yPos = 0f; // Pri dolnom okraji

                // Matica transformácie pre škálovanie a pozíciu
                canvas.ConcatMatrix(scale, 0, 0, scale, xPos, yPos);

                using (var svgStream = File.OpenRead(svgBottomPath))
                {
                    SvgConverter.DrawOnCanvas(svgStream, canvas, 0, 0);
                }

                canvas.RestoreState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Chyba pri vykreslení dolných vlniek: {ex.Message}");
            }
        }

        /// <summary>
        /// Pridá textový obsah certifikátu
        /// </summary>
        private static void AddCertificateContent(
            Document document,
            PdfFont titleFont,
            PdfFont textFont,
            CertificateTemplateModel template,
            string organizerName,
            string eventTopic,
            DateTime? eventDate,
            string participantName,
            string eventLocation)
        {
            Color titleColor = ParseColor(template.TitleColor);
            Color textColor = ParseColor(template.TextColor);
            Debug.WriteLine("private static void AddCertificateContent()");

            // Hlavný nadpis
            document.Add(new Paragraph("\n\n\n\nCERTIFIKÁT O ABSOLVOVANÍ")
                .SetFont(titleFont)
                .SetFontSize(template.TitleFontSize)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(titleColor));

            // Úvodný text
            document.Add(new Paragraph("\nTento certifikát potvrdzuje, že")
                .SetFont(textFont)
                .SetFontSize(template.TextFontSize)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(textColor));

            // Meno účastníka (zvýraznené)
            document.Add(new Paragraph(participantName)
                .SetFont(titleFont)
                .SetFontSize(template.HeaderFontSize + 4)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLACK));

            // Popis školenia
            document.Add(new Paragraph($"úspešne absolvoval(a) vzdelávacie podujatie\n\n\"{eventTopic}\"")
                .SetFont(textFont)
                .SetFontSize(template.TextFontSize)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(textColor));

            Debug.WriteLine(document.ToString());

            // Dátum a miesto
            string dateString = eventDate.HasValue ? eventDate.Value.ToString("dd.MM.yyyy") : "___________";
            document.Add(new Paragraph($"\nDátum: {dateString}        Miesto: {eventLocation}")
                .SetFont(textFont)
                .SetFontSize(template.TextFontSize - 1)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(textColor));

            // Podpisy
            Debug.WriteLine(document.ToString());
            AddSignatureTable(document, textFont, template, organizerName);
        }

        /// <summary>
        /// Pridá tabuľku s podpismi (garant a organizátor)
        /// </summary>
        private static void AddSignatureTable(Document document, PdfFont textFont, CertificateTemplateModel template, string organizerName)
        {
            document.Add(new Paragraph("\n\n\n"));

            Table signatureTable = new Table(2).UseAllAvailableWidth();
            signatureTable.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            // Ľavý podpis (Garant)
            Cell leftCell = new Cell()
                .Add(new Paragraph("______________________")
                    .SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph("garant podujatia")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(textFont)
                    .SetFontSize(template.TextFontSize - 2))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingRight(20);

            // Pravý podpis (Organizátor)
            Cell rightCell = new Cell()
                .Add(new Paragraph("______________________")
                    .SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph(string.IsNullOrWhiteSpace(organizerName) ? "riaditeľ" : organizerName)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(textFont)
                    .SetFontSize(template.TextFontSize - 2))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingLeft(20);

            signatureTable.AddCell(leftCell);
            signatureTable.AddCell(rightCell);

            document.Add(signatureTable);
        }

        /// <summary>
        /// Vráti predvolenú Caduceus šablónu s profesionálnym dizajnom
        /// </summary>
        public static CertificateTemplateModel GetDefaultCaduceusTemplate()
        {
            return new CertificateTemplateModel
            {
                Name = "Moderný Caduceus",
                TitleFontFamily = "Arial-Bold",
                HeaderFontFamily = "Arial-Bold",
                TextFontFamily = "Arial",
                TitleColor = "#003C7D",      // Tmavomodrá
                TextColor = "#1F2937",       // Tmavosivá
                AccentColor = "#0055AA",     // Stredne modrá
                BorderColor = "#0055AA",     // Modrý rám
                TitleFontSize = 28,
                HeaderFontSize = 22,
                TextFontSize = 14,
                ShowBorder = true,
                BorderWidth = 2,
                MarginTop = 40,
                MarginRight = 40,
                MarginBottom = 40,
                MarginLeft = 40
            };
        }

        /// <summary>
        /// Načíta font s podporou Windows systémových fontov
        /// </summary>
        private static PdfFont GetFont(string fontFamily)
        {
            try
            {
                // Skús načítať systémový font
                if (fontFamily.Contains("Arial") || fontFamily.Contains("Calibri") ||
                    fontFamily.Contains("Georgia") || fontFamily.Contains("Times"))
                {
                    string fontPath = FindSystemFont(fontFamily);
                    if (!string.IsNullOrEmpty(fontPath) && File.Exists(fontPath))
                    {
                        return PdfFontFactory.CreateFont(fontPath, "Cp1250", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                    }
                }

                // Fallback na Helvetica
                return PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA, "Cp1250");
            }
            catch
            {
                return PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA, "Cp1250");
            }
        }

        /// <summary>
        /// Hľadá systémový font v Windows Fonts priečinku
        /// </summary>
        private static string FindSystemFont(string fontFamily)
        {
            string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            if (fontFamily.Contains("Bold") && fontFamily.Contains("Italic"))
            {
                fontFamily = fontFamily.Replace("-BoldItalic", "").Replace("Bold", "").Replace("Italic", "").Trim();
                return System.IO.Path.Combine(fontsFolder, $"{fontFamily}bi.ttf");
            }
            else if (fontFamily.Contains("Bold"))
            {
                fontFamily = fontFamily.Replace("-Bold", "").Replace("Bold", "").Trim();
                return System.IO.Path.Combine(fontsFolder, $"{fontFamily}bd.ttf");
            }
            else if (fontFamily.Contains("Italic"))
            {
                fontFamily = fontFamily.Replace("-Italic", "").Replace("Italic", "").Trim();
                return System.IO.Path.Combine(fontsFolder, $"{fontFamily}i.ttf");
            }

            return System.IO.Path.Combine(fontsFolder, $"{fontFamily.Trim()}.ttf");
        }

        /// <summary>
        /// Parsuje hex farbu na iText Color objekt
        /// </summary>
        private static Color ParseColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return ColorConstants.BLACK;

            hexColor = hexColor.TrimStart('#');

            if (hexColor.Length == 6)
            {
                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return new DeviceRgb(r, g, b);
            }

            return ColorConstants.BLACK;
        }
    }
}
