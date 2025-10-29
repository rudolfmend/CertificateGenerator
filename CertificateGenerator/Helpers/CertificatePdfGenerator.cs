using CertificateGenerator.Data;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Colorspace;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.IO;
using System.Diagnostics;
using iText.IO.Font;

namespace CertificateGenerator.Helpers
{
    /// <summary>
    /// Helper pre generovanie PDF certifikátov podľa šablón
    /// </summary>
    public class CertificatePdfGenerator
    {
        public static void GeneratePdf(
            string filePath,
            CertificateTemplateModel template,
            string organizerName,
            string eventTopic,
            DateTime? eventDate,
            string participantName,
            DateTime? birthDate,
            string registrationNumber,
            string notes,
            PageSize pageSize)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdfDocument = new PdfDocument(writer))
            {
                // KRITICKÉ: Vytvor stránku PRED Document objektom
                pdfDocument.AddNewPage(pageSize);

                using (Document document = new Document(pdfDocument))
                {

                    // Nastavenie okrajov
                    document.SetMargins(
                        template.MarginTop,
                        template.MarginRight,
                        template.MarginBottom,
                        template.MarginLeft
                    );

                    // Načítanie fontov
                    PdfFont titleFont = GetFont(template.TitleFontFamily);
                    PdfFont headerFont = GetFont(template.HeaderFontFamily);
                    PdfFont textFont = GetFont(template.TextFontFamily);

                    // Farby
                    Color titleColor = ParseColor(template.TitleColor);
                    Color textColor = ParseColor(template.TextColor);
                    Color accentColor = ParseColor(template.AccentColor);

                    // Rámček okolo celého dokumentu
                    if (template.ShowBorder)
                    {
                        AddPageBorder(pdfDocument, pageSize, template);
                    }

                    // Horná dekorácia(vlnovka)
                    if (template.ShowTopDecoration)
                    {
                        PdfCanvas canvas = new PdfCanvas(pdfDocument.GetFirstPage());

                        // Pozícia: tesne pod horným okrajom
                        float decorY = pageSize.GetHeight() - template.MarginTop + 15;
                        float decorStartX = template.MarginLeft;
                        float decorEndX = pageSize.GetWidth() - template.MarginRight;

                        DrawWavyLine(
                            canvas,
                            decorStartX,        // Začiatok X
                            decorY,             // Y pozícia
                            decorEndX,          // Koniec X
                            5,                  // Amplitúda vlny (výška)
                            270,                 // Vlnová dĺžka
                            ParseColor(template.TopDecorationColor),
                            template.TopDecorationThickness
                        );
                    }

                    // Logo na začiatku
                    if (!string.IsNullOrWhiteSpace(template.LogoPath) &&
                        template.LogoPosition == "TOP" &&
                        File.Exists(template.LogoPath))
                    {
                        AddLogo(document, template, HorizontalAlignment.CENTER);
                    }

                    // Vlastný text v záhlaví
                    if (!string.IsNullOrWhiteSpace(template.CustomHeaderText))
                    {
                        document.Add(new Paragraph(template.CustomHeaderText)
                            .SetFont(textFont)
                            .SetFontSize(template.TextFontSize)
                            .SetFontColor(textColor)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(15));
                    }

                    // Titulok certifikátu
                    if (template.ShowTitle && !string.IsNullOrWhiteSpace(template.CertificateTitle))
                    {
                        TextAlignment alignment = template.TitleAlignment == "LEFT" ? TextAlignment.LEFT :
                                                 template.TitleAlignment == "RIGHT" ? TextAlignment.RIGHT :
                                                 TextAlignment.CENTER;

                        // Titulok bez SetUnderline() (nefunguje spoľahlivo v PDF)
                        Paragraph title = new Paragraph(template.CertificateTitle)
                            .SetFont(titleFont)
                            .SetFontSize(template.TitleFontSize)
                            .SetFontColor(titleColor)
                            .SetTextAlignment(alignment)
                            .SetMarginBottom(template.TitleUnderlined ? 5 : 20);

                        document.Add(title);

                        // Ak je požadované podčiarknutie, pridaj vizuálnu ozdobnú čiaru
                        if (template.TitleUnderlined)
                        {
                            PdfCanvas canvas = new PdfCanvas(pdfDocument.GetFirstPage());
                            float pageWidth = pageSize.GetWidth();

                            // Vypočítaj Y pozíciu čiary
                            float logoHeight = (template.LogoPosition == "TOP" && !string.IsNullOrWhiteSpace(template.LogoPath) && File.Exists(template.LogoPath)) ?
                                template.LogoHeight + 20 : 0;
                            float headerTextHeight = !string.IsNullOrWhiteSpace(template.CustomHeaderText) ? 30 : 0;

                            float currentY = pageSize.GetHeight() - template.MarginTop - logoHeight - headerTextHeight - template.TitleFontSize - 8;

                            // Vypočítaj šírku textu pre centrovanie čiary
                            float textWidth = titleFont.GetWidth(template.CertificateTitle, template.TitleFontSize);
                            float lineStartX = template.MarginLeft;
                            float lineEndX = pageWidth - template.MarginRight;

                            if (alignment == TextAlignment.CENTER)
                            {
                                lineStartX = (pageWidth - textWidth) / 2;
                                lineEndX = lineStartX + textWidth;
                            }
                            else if (alignment == TextAlignment.RIGHT)
                            {
                                lineStartX = pageWidth - template.MarginRight - textWidth;
                                lineEndX = pageWidth - template.MarginRight;
                            }
                            else // LEFT
                            {
                                lineEndX = template.MarginLeft + textWidth;
                            }

                            // Vlnovková čiara pod titulkom
                            DrawWavyLine(
                                canvas,
                                lineStartX,
                                currentY,
                                lineEndX,
                                2,  // Amplitúda
                                12, // Frekvencia
                                titleColor,
                                0.8f   // Hrúbka
                            );

                            document.Add(new Paragraph().SetMarginBottom(15));
                        }
                    }

                    // Oddeľovacia čiara
                    if (template.ShowSeparatorLine)
                    {
                        AddSeparator(document, template, accentColor);
                    }


                    // === POLIA PODĽA PORADIA A LAYOUTU ===
                    Debug.WriteLine($"[PDF Generator] ContentLayout={template.ContentLayout}, FieldOrder={template.FieldOrder}");

                    var fieldOrder = template.GetFieldOrderList();
                    var fieldData = new System.Collections.Generic.List<FieldData>();

                    // Zozbieraj všetky viditeľné polia podľa poradia
                    foreach (var fieldId in fieldOrder)
                    {
                        string label = "";
                        string value = "";
                        bool isVisible = false;

                        switch (fieldId)
                        {
                            case "Organizer":
                                if (template.ShowOrganizer && !string.IsNullOrWhiteSpace(organizerName))
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelOrganizer) ? "Organizátor:" : template.LabelOrganizer;
                                    value = organizerName;
                                    isVisible = true;
                                }
                                break;

                            case "EventTopic":
                                if (template.ShowEventTopic && !string.IsNullOrWhiteSpace(eventTopic))
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelEventTopic) ? "Názov podujatia:" : template.LabelEventTopic;
                                    value = eventTopic;
                                    isVisible = true;
                                }
                                break;

                            case "EventDate":
                                if (template.ShowEventDate && eventDate.HasValue)
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelEventDate) ? "Dátum:" : template.LabelEventDate;
                                    value = eventDate.Value.ToString("d. M. yyyy");
                                    isVisible = true;
                                }
                                break;

                            case "Name":
                                if (template.ShowName && !string.IsNullOrWhiteSpace(participantName))
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelParticipant) ? "Meno účastníka:" : template.LabelParticipant;
                                    value = participantName;
                                    isVisible = true;
                                }
                                break;

                            case "BirthDate":
                                if (template.ShowBirthDate && birthDate.HasValue)
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelBirthDate) ? "Dátum narodenia:" : template.LabelBirthDate;
                                    value = birthDate.Value.ToString("d. M. yyyy");
                                    isVisible = true;
                                }
                                break;

                            case "RegistrationNumber":
                                if (template.ShowRegistrationNumber && !string.IsNullOrWhiteSpace(registrationNumber))
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelRegistrationNumber) ? "Registračné číslo:" : template.LabelRegistrationNumber;
                                    value = registrationNumber;
                                    isVisible = true;
                                }
                                break;

                            case "Notes":
                                if (template.ShowNotes && !string.IsNullOrWhiteSpace(notes))
                                {
                                    label = string.IsNullOrWhiteSpace(template.LabelNotes) ? "Poznámky:" : template.LabelNotes;
                                    value = notes;
                                    isVisible = true;
                                }
                                break;
                        }

                        if (isVisible)
                        {
                            fieldData.Add(new FieldData { Label = label, Value = value });
                        }
                    }

                    // === VYKRESLENIE PODĽA LAYOUTU ===
                    if (template.ContentLayout == "MODERN")
                    {
                        // MODERNÝ LAYOUT - individuálne umiestnenie polí

                        // 1. TÉMA SEMINÁRA - hore, centrované
                        if (template.ShowEventTopic && !string.IsNullOrWhiteSpace(eventTopic))
                        {
                            document.Add(new Paragraph(string.IsNullOrWhiteSpace(template.LabelEventTopic) ? "Téma seminára" : template.LabelEventTopic)
                                .SetFont(headerFont)
                                .SetFontSize(template.HeaderFontSize)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetMarginBottom(5));

                            document.Add(new Paragraph(eventTopic)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize + 2)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetMarginBottom(25));
                        }

                        // 2. MENO ÚČASTNÍKA - veľké, centrované
                        if (template.ShowName && !string.IsNullOrWhiteSpace(participantName))
                        {
                            document.Add(new Paragraph(participantName)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize + 6)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetMarginBottom(10));
                        }

                        // 3. DÁTUM NARODENIA a REGISTRAČNÉ ČÍSLO - vedľa siebie, centrované
                        Table infoTable = new Table(new float[] { 1, 1 }).UseAllAvailableWidth()
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetMarginBottom(30);

                        string birthInfo = "";
                        string regInfo = "";

                        if (template.ShowBirthDate && birthDate.HasValue)
                        {
                            birthInfo = birthDate.Value.ToString("d. M. yyyy");
                        }

                        if (template.ShowRegistrationNumber && !string.IsNullOrWhiteSpace(registrationNumber))
                        {
                            regInfo = "Reg.č. " + registrationNumber;
                        }

                        Cell birthCell = new Cell()
                            .Add(new Paragraph(birthInfo)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        Cell regCell = new Cell()
                            .Add(new Paragraph(regInfo)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        infoTable.AddCell(birthCell);
                        infoTable.AddCell(regCell);
                        document.Add(infoTable);

                        // 4. DÁTUM KONANIA - vpravo dole
                        if (template.ShowEventDate && eventDate.HasValue)
                        {
                            document.Add(new Paragraph(string.IsNullOrWhiteSpace(template.LabelEventDate) ? "Dátum konania podujatia" : template.LabelEventDate)
                                .SetFont(headerFont)
                                .SetFontSize(template.HeaderFontSize - 1)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetMarginTop(20)
                                .SetMarginBottom(3));

                            document.Add(new Paragraph(eventDate.Value.ToString("d. M. yyyy"))
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize + 2)
                                .SetFontColor(textColor)
                                .SetTextAlignment(TextAlignment.RIGHT));
                        }

                        // 5. ORGANIZÁTOR - ak je viditeľný
                        if (template.ShowOrganizer && !string.IsNullOrWhiteSpace(organizerName))
                        {
                            document.Add(new Paragraph(string.IsNullOrWhiteSpace(template.LabelOrganizer) ? "Organizátor:" : template.LabelOrganizer)
                                .SetFont(headerFont)
                                .SetFontSize(template.HeaderFontSize)
                                .SetFontColor(textColor)
                                .SetMarginTop(15)
                                .SetMarginBottom(5));

                            document.Add(new Paragraph(organizerName)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor)
                                .SetMarginBottom(15));
                        }

                        // 6. POZNÁMKY - ak sú viditeľné
                        if (template.ShowNotes && !string.IsNullOrWhiteSpace(notes))
                        {
                            document.Add(new Paragraph(string.IsNullOrWhiteSpace(template.LabelNotes) ? "Poznámky:" : template.LabelNotes)
                                .SetFont(headerFont)
                                .SetFontSize(template.HeaderFontSize)
                                .SetFontColor(textColor)
                                .SetMarginBottom(5));

                            document.Add(new Paragraph(notes)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor));
                        }
                    }
                    else if (template.ContentLayout == "TWO_COLUMN")
                    {
                        // Dvojstĺpcový layout
                        Table table = new Table(2).UseAllAvailableWidth()
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        foreach (var field in fieldData)
                        {
                            Cell labelCell = new Cell()
                                .Add(new Paragraph(field.Label)
                                    .SetFont(headerFont)
                                    .SetFontSize(template.HeaderFontSize)
                                    .SetFontColor(textColor)
                                    .SetTextAlignment(TextAlignment.LEFT))
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                .SetPaddingBottom(8);

                            Cell valueCell = new Cell()
                                .Add(new Paragraph(field.Value)
                                    .SetFont(textFont)
                                    .SetFontSize(template.TextFontSize)
                                    .SetFontColor(textColor)
                                    .SetTextAlignment(TextAlignment.LEFT))
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                                .SetPaddingBottom(8);

                            table.AddCell(labelCell);
                            table.AddCell(valueCell);
                        }

                        document.Add(table);
                    }
                    else
                    {
                        // Vertikálny layout (pôvodný)
                        foreach (var field in fieldData)
                        {
                            document.Add(new Paragraph(field.Label)
                                .SetFont(headerFont)
                                .SetFontSize(template.HeaderFontSize)
                                .SetFontColor(textColor)
                                .SetMarginBottom(5));

                            document.Add(new Paragraph(field.Value)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor)
                                .SetMarginBottom(15));
                        }
                    }

                    // Vlastný text v pätičke
                    if (!string.IsNullOrWhiteSpace(template.CustomFooterText))
                    {
                        TextAlignment footerAlignment = template.CustomFooterAlignment == "LEFT" ? TextAlignment.LEFT :
                                                       template.CustomFooterAlignment == "RIGHT" ? TextAlignment.RIGHT :
                                                       TextAlignment.CENTER;

                        Paragraph footer = new Paragraph(template.CustomFooterText)
                            .SetFont(textFont)
                            .SetFontSize(template.TextFontSize - 1)
                            .SetFontColor(textColor)
                            .SetTextAlignment(footerAlignment)
                            .SetMarginTop(30);

                        document.Add(footer);
                    }

                    // Dolná dekorácia (vlnovka)
                    if (template.ShowBottomDecoration)
                    {
                        PdfCanvas canvas = new PdfCanvas(pdfDocument.GetFirstPage());

                        // Pozícia: tesne nad spodným okrajom
                        float decorY = template.MarginBottom - 15;
                        float decorStartX = template.MarginLeft;
                        float decorEndX = pageSize.GetWidth() - template.MarginRight;

                        DrawWavyLine(
                            canvas,
                            decorStartX,
                            decorY,
                            decorEndX,
                            5,
                            270,
                            ParseColor(template.BottomDecorationColor),
                            template.BottomDecorationThickness
                        );
                    }

                    // Logo na konci
                    if (!string.IsNullOrWhiteSpace(template.LogoPath) &&
                        template.LogoPosition == "BOTTOM" &&
                        File.Exists(template.LogoPath))
                    {
                        AddLogo(document, template, HorizontalAlignment.CENTER);
                    }
                }
            }
        }

        /// <summary>
        /// Vytvorí generickú šablónu pre náhľad
        /// </summary>
        public static void GeneratePreviewPdf(string filePath, CertificateTemplateModel template, PageSize pageSize)
        {
            // Dummy data pre náhľad
            GeneratePdf(
                filePath,
                template,
                organizerName: "Vzdelávacia inštitúcia s.r.o.",
                eventTopic: "Programovanie v C# a .NET",
                eventDate: DateTime.Now,
                participantName: "Ján Novák",
                birthDate: new DateTime(1990, 5, 15),
                registrationNumber: "CER-2024-001",
                notes: "Absolvoval všetky moduly s výborným hodnotením",
                pageSize: pageSize
            );
        }

        /// <summary>
        /// Vytvorí nápovedu k šablóne (zobrazenie názvu polí)
        /// </summary>
        public static void GenerateTemplateLegend(string filePath)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdfDocument = new PdfDocument(writer))
            using (Document document = new Document(pdfDocument, PageSize.A4))
            {
                document.Add(new Paragraph("Popis polí certifikátu")
                    .SetFontSize(18)
                    .SetMarginBottom(20));

                document.Add(new Paragraph("Organizátor: Názov organizácie, ktorá udeľuje certifikát")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Názov podujatia: Téma seminára alebo kurzu")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Dátum: Dátum konania podujatia")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Meno účastníka: Celé meno osoby, ktorej sa certifikát udeľuje")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Dátum narodenia: Dátum narodenia účastníka")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Registračné číslo: Unikátny identifikátor certifikátu")
                    .SetMarginBottom(10));
                document.Add(new Paragraph("Poznámky: Ďalšie informácie alebo komentáre")
                    .SetMarginBottom(10));
            }
        }


        /// <summary>
        /// Pridá logo do dokumentu
        /// </summary>
        private static void AddLogo(Document document, CertificateTemplateModel template, HorizontalAlignment alignment)
        {
            try
            {
                ImageData imageData = ImageDataFactory.Create(template.LogoPath);
                Image logo = new Image(imageData);
                logo.SetWidth(template.LogoWidth);
                logo.SetHeight(template.LogoHeight);
                logo.SetHorizontalAlignment(alignment);
                logo.SetMarginBottom(20);

                document.Add(logo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri načítaní loga: {ex.Message}");
            }
        }

        /// <summary>
        /// Pridá oddeľovaciu čiaru
        /// </summary>
        private static void AddSeparator(Document document, CertificateTemplateModel template, Color color)
        {
            if (template.SeparatorStyle == "DOUBLE_LINE")
            {
                // Dvojitá čiara
                LineSeparator line1 = new LineSeparator(new SolidLine(1));
                line1.SetStrokeColor(color);
                line1.SetMarginBottom(2);
                document.Add(line1);

                LineSeparator line2 = new LineSeparator(new SolidLine(1));
                line2.SetStrokeColor(color);
                line2.SetMarginBottom(20);
                document.Add(line2);
            }
            else if (template.SeparatorStyle == "DASHED")
            {
                // Prerušovaná čiara
                LineSeparator line = new LineSeparator(new DashedLine());
                line.SetStrokeColor(color);
                line.SetMarginBottom(20);
                document.Add(line);
            }
            else
            {
                // Štandardná plná čiara
                LineSeparator line = new LineSeparator(new SolidLine(1));
                line.SetStrokeColor(color);
                line.SetMarginBottom(20);
                document.Add(line);
            }
        }

        /// <summary>
        /// Pridá rámček okolo celej stránky
        /// </summary>
        private static void AddPageBorder(PdfDocument pdfDocument, PageSize pageSize, CertificateTemplateModel template)
        {
            PdfCanvas canvas = new PdfCanvas(pdfDocument.GetFirstPage());
            Color borderColor = ParseColor(template.BorderColor);

            canvas.SetStrokeColor(borderColor);
            canvas.SetLineWidth(template.BorderWidth);

            // Vonkajší rámček
            canvas.Rectangle(
                template.MarginLeft / 2,
                template.MarginBottom / 2,
                pageSize.GetWidth() - template.MarginLeft / 2 - template.MarginRight / 2,
                pageSize.GetHeight() - template.MarginTop / 2 - template.MarginBottom / 2
            );
            canvas.Stroke();
        }

        /// <summary>
        /// Vykreslí vlnovkovú čiaru na canvas
        /// </summary>
        private static void DrawWavyLine(
            PdfCanvas canvas,
            float startX,
            float y,
            float endX,
            float amplitude,
            float frequency,
            Color color,
            float thickness)
        {
            canvas.SaveState();
            canvas.SetStrokeColor(color);
            canvas.SetLineWidth(thickness);

            canvas.MoveTo(startX, y);

            float length = endX - startX;
            int steps = (int)(length / frequency) * 10;
            float stepSize = length / steps;

            for (int i = 0; i <= steps; i++)
            {
                float x = startX + i * stepSize;
                float waveY = y + amplitude * (float)Math.Sin((i * Math.PI * 2) / (steps / (length / frequency)));
                canvas.LineTo(x, waveY);
            }

            canvas.Stroke();
            canvas.RestoreState();
        }

        /// <summary>
        /// Parsuje hex farbu na iText Color objekt
        /// </summary>
        private static Color ParseColor(string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor) || !hexColor.StartsWith("#"))
            {
                return ColorConstants.BLACK;
            }

            try
            {
                string hex = hexColor.TrimStart('#');
                if (hex.Length != 6) return ColorConstants.BLACK;

                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);

                return new DeviceRgb(r, g, b);
            }
            catch
            {
                return ColorConstants.BLACK;
            }
        }

        /// <summary>
        /// Získa font podľa názvu s podporou slovenských diakritických znakov
        /// </summary>
        private static PdfFont GetFont(string fontFamily)
        {
            try
            {
                string fontPath = null;

                switch (fontFamily?.ToUpper())
                {
                    case "TIMES-ROMAN":
                    case "TIMES":
                        // Prioritne hľadáme fonty s dobrou podporou diakritiky
                        fontPath = FindBestFont(new[] {
                            "C:\\Windows\\Fonts\\times.ttf",
                            "C:\\Windows\\Fonts\\calibri.ttf",
                            "C:\\Windows\\Fonts\\arial.ttf"
                        });
                        break;

                    case "TIMES-BOLD":
                        fontPath = FindBestFont(new[] {
                            "C:\\Windows\\Fonts\\timesbd.ttf",
                            "C:\\Windows\\Fonts\\calibrib.ttf",
                            "C:\\Windows\\Fonts\\arialbd.ttf"
                        });
                        break;

                    case "TIMES-ITALIC":
                        fontPath = FindBestFont(new[] {
                            "C:\\Windows\\Fonts\\timesi.ttf",
                            "C:\\Windows\\Fonts\\calibrii.ttf",
                            "C:\\Windows\\Fonts\\ariali.ttf"
                        });
                        break;

                    case "HELVETICA":
                    case "HELVETICA-BOLD":
                    case "COURIER":
                    default:
                        fontPath = FindBestFont(new[] {
                            "C:\\Windows\\Fonts\\calibri.ttf",
                            "C:\\Windows\\Fonts\\arial.ttf"
                        });
                        break;
                }

                if (!string.IsNullOrEmpty(fontPath) && File.Exists(fontPath))
                {
                    // KRITICKÉ: Použitie PdfEncodings.IDENTITY_H pre správnu podporu Unicode (slovenčina)
                    return PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                }

                // Fallback na embedded font s Unicode podporou
                return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Font error: {ex.Message}");
                try
                {
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }
                catch
                {
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                }
            }
        }

        /// <summary>
        /// Nájde prvý existujúci font z daného zoznamu
        /// </summary>
        private static string FindBestFont(string[] fontPaths)
        {
            foreach (var fontPath in fontPaths)
            {
                if (File.Exists(fontPath))
                {
                    return fontPath;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Helper trieda pre uloženie údajov poľa
    /// </summary>
    internal class FieldData
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
