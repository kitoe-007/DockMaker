namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Значения полей формы шаблона для валидации и генерации документа.
    /// </summary>
    internal class TemplateFormFields
    {
        /// <summary>Ключ шаблона: «OJR» или «ZHV».</summary>
        public string TemplateKey { get; set; }

        /// <summary>Путь к Excel-файлу источника данных.</summary>
        public string DataSourcePath { get; set; }

        /// <summary>Текст поля «Обрезать сверху».</summary>
        public string CutTopText { get; set; }

        /// <summary>Текст поля «Обрезать снизу».</summary>
        public string CutBottomText { get; set; }

        /// <summary>Фильтр по разделу (один или несколько через запятую).</summary>
        public string SectionFilter { get; set; }

        /// <summary>ФИО ответственного лица (зависит от шаблона).</summary>
        public string Fio { get; set; }

        /// <summary>Должность и ФИО для ОЖР.</summary>
        public string PositionFio { get; set; }

        /// <summary>Наименование объекта (ЖВК).</summary>
        public string ObjectName { get; set; }

        /// <summary>Адрес объекта (ЖВК).</summary>
        public string ObjectAddress { get; set; }

        /// <summary>Строительная организация (ЖВК).</summary>
        public string Organization { get; set; }

        /// <summary>Дата начала ведения журнала (ЖВК).</summary>
        public string StartedDate { get; set; }

        /// <summary>Дата окончания ведения журнала (ЖВК).</summary>
        public string FinishedDate { get; set; }

        /// <summary>Поставщик по умолчанию для строк (ЖВК).</summary>
        public string Supplier { get; set; }

        /// <summary>Вид контроля по умолчанию (ЖВК).</summary>
        public string ControlType { get; set; }

        /// <summary>Результат контроля по умолчанию (ЖВК).</summary>
        public string ControlResult { get; set; }

        /// <summary>ФИО контролирующего лица (ЖВК).</summary>
        public string InspectorFio { get; set; }
    }
}
