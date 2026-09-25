using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EcommerceApp.Services.Reportes
{
    public static class PdfReportStyle
    {
        public const string Navy = "#0B1220";
        public const string Blue = "#4F7CFF";
        public const string BlueLight = "#8AA4FF";
        public const string Green = "#34D399";
        public const string Yellow = "#FBBF24";
        public const string Red = "#FB7185";
        public const string Gray = "#64748B";
        public const string LightGray = "#E2E8F0";
        public const string Background = "#F8FAFC";
        public const string White = "#FFFFFF";
        public const string Text = "#172033";

        public static void ConfigurePage(
            PageDescriptor page,
            string titulo,
            string subtitulo)
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);

            page.Header().Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(52)
                        .Height(52)
                        .Background(Navy)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("ES")
                        .FontSize(16)
                        .Bold()
                        .FontColor(White);

                    row.RelativeItem()
                        .PaddingLeft(14)
                        .Column(info =>
                        {
                            info.Item()
                                .Text("ExamSecure")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Navy);

                            info.Item()
                                .Text("Plataforma de evaluación académica segura")
                                .FontSize(8)
                                .FontColor(Gray);
                        });

                    row.ConstantItem(190)
                        .AlignRight()
                        .Column(info =>
                        {
                            info.Item()
                                .Text(titulo.ToUpper())
                                .FontSize(10)
                                .Bold()
                                .FontColor(Blue);

                            info.Item()
                                .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                .FontSize(8)
                                .FontColor(Gray);
                        });
                });

                column.Item()
                    .Height(3)
                    .Background(Blue);

                column.Item()
                    .PaddingTop(8)
                    .Column(info =>
                    {
                        info.Item()
                            .Text(titulo)
                            .FontSize(21)
                            .Bold()
                            .FontColor(Navy);

                        info.Item()
                            .Text(subtitulo)
                            .FontSize(9)
                            .FontColor(Gray);
                    });
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem()
                    .Text("ExamSecure • Plataforma de evaluación académica segura")
                    .FontSize(8)
                    .FontColor(Gray);

                row.ConstantItem(160)
                    .AlignRight()
                    .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                    .FontSize(8)
                    .FontColor(Gray);
            });
        }

        public static void StatCard(
            IContainer container,
            string label,
            string value,
            string accent)
        {
            container
                .Background(White)
                .Border(1)
                .BorderColor(LightGray)
                .Padding(12)
                .Column(column =>
                {
                    column.Item()
                        .Text(value)
                        .FontSize(20)
                        .Bold()
                        .FontColor(accent);

                    column.Item()
                        .Text(label)
                        .FontSize(8)
                        .FontColor(Gray);
                });
        }

        public static void TableHeader(
            IContainer container,
            string text)
        {
            container
                .Background(Navy)
                .Padding(6)
                .Text(text)
                .FontSize(7.5f)
                .Bold()
                .FontColor(White);
        }

        public static void TableCell(
            IContainer container,
            string text,
            bool alternate = false)
        {
            container
                .Background(alternate ? Background : White)
                .BorderBottom(1)
                .BorderColor(LightGray)
                .Padding(6)
                .Text(text)
                .FontSize(7.5f)
                .FontColor(Text);
        }

        public static void SectionTitle(
            IContainer container,
            string text)
        {
            container
                .Padding(6)
                .Text(text)
                .FontSize(11)
                .Bold()
                .FontColor(Navy);
        }

        public static string EstadoColor(string estado)
        {
            return estado switch
            {
                "Activo" => Green,
                "Publicado" => Blue,
                "Cerrado" => Red,
                "Borrador" => Yellow,
                _ => Gray
            };
        }
    }
}
