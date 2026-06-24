using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Синхронизация текстовой шапки предпросмотра ЖВК с полями формы.
    /// </summary>
    internal class UpdateZhvPreviewHeaderClass
    {
        /// <summary>
        /// Обновляет блоки предпросмотра шапки ЖВК по значениям из полей ввода.
        /// </summary>
        /// <param name="objectNamePreview">TextBlock наименования объекта.</param>
        /// <param name="objectAddressPreview">TextBlock адреса объекта.</param>
        /// <param name="organizationPreview">TextBlock организации.</param>
        /// <param name="startedDatePreview">TextBlock даты начала.</param>
        /// <param name="finishedDatePreview">TextBlock даты окончания.</param>
        /// <param name="objectName">Значение поля «Наименование объекта».</param>
        /// <param name="objectAddress">Значение поля «Адрес объекта».</param>
        /// <param name="organization">Значение поля «Организация».</param>
        /// <param name="startedDate">Значение поля «Начат».</param>
        /// <param name="finishedDate">Значение поля «Окончен».</param>
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

        /// <summary>
        /// Формирует строку «Метка  значение» или пустую строку, если значение не задано.
        /// </summary>
        /// <param name="label">Подпись поля.</param>
        /// <param name="value">Значение поля.</param>
        private static string FormatLabeledField(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : $"{label}  {value.Trim()}";
        }
    }
}
