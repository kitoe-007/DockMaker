using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.classes
{
    internal class UpdateZhvPreviewHeaderClass
    {
        // Текст шапки ЖВК в предпросмотре синхронизируется с полями параметров
        public static void Update(
            TextBlock objectNamePreview,
            TextBlock objectAddressPreview,
            TextBlock organizationPreview,
            TextBlock startedDatePreview,
            TextBlock finishedDatePreview,
            string objectName,
            string objectAddress,
            string organization,
            string startedDate,
            string finishedDate)
        {
            objectNamePreview.Text = FormatLabeledField("Наименование объекта", objectName);
            objectAddressPreview.Text = FormatLabeledField("Адрес объекта", objectAddress);
            organizationPreview.Text = FormatLabeledField("Наименование строительной организации", organization);
            startedDatePreview.Text = FormatLabeledField("Начат", startedDate);
            finishedDatePreview.Text = FormatLabeledField("Окончен", finishedDate);
        }
        private static string FormatLabeledField(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : $"{label}  {value.Trim()}";
        }
    }
}
