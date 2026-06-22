import sys
import json
import io

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

OJR_HEADER_LINES = [
    ("РАЗДЕЛ 3", True),
    ("Сведения о выполнении работ в процессе строительства, реконструкции, капитального ремонта объекта", False),
    ("капитального строительства", False),
]

# Подзаголовок шапки ЖВК (по форме «ЖВК ЭМ5 Флотация.docx»)
ZHV_SUBTITLE = (
    "входного учета и контроля качества получаемых деталей, "
    "материалов, конструкций и оборудования"
)


class DOCX_table_builder:
    def __init__(self):
        pass

    def format_data_cell(self, cell, text):
        cell.text = "" if text is None else str(text)

    def add_centered_paragraph(self, document, text, is_bold=False):
        paragraph = document.add_paragraph()
        paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
        run = paragraph.add_run(text)
        run.bold = is_bold

    def add_ojr_header(self, document):
        for text, is_bold in OJR_HEADER_LINES:
            self.add_centered_paragraph(document, text, is_bold)

    def add_zhv_header(self, document, meta):
        """Шапка журнала входного контроля; поля приходят из UI через ключи _object_name и т.д."""
        self.add_centered_paragraph(document, "Журнал", True)
        self.add_centered_paragraph(document, ZHV_SUBTITLE, False)

        object_name = meta.get("_object_name", "").strip()
        object_address = meta.get("_object_address", "").strip()
        organization = meta.get("_organization", "").strip()
        started_date = meta.get("_started_date", "").strip()
        finished_date = meta.get("_finished_date", "").strip()

        if object_name:
            document.add_paragraph(f"Наименование объекта  {object_name}")
        if object_address:
            document.add_paragraph(f"Адрес объекта  {object_address}")
        if organization:
            document.add_paragraph(f"Наименование строительной организации   {organization}")

        document.add_paragraph()
        # Пустые даты — шаблонные подчёркивания, как в бумажной форме
        if started_date:
            document.add_paragraph(f"Начат {started_date}")
        else:
            document.add_paragraph("Начат _________________ 20___ г.")

        if finished_date:
            document.add_paragraph(f"Окончен {finished_date}")
        else:
            document.add_paragraph("Окончен _____________________ 20___ г.")

        document.add_paragraph()

    def build(self, data, filename):
        # Ключи с «_» — метаданные шапки; column_N — столбцы таблицы (первая строка — заголовки)
        meta = {key: (values[0] if values else "") for key, values in data.items() if key.startswith("_")}
        headers = [key for key in data.keys() if not key.startswith("_")]
        columns = [data[header] for header in headers]

        document = Document()
        template = meta.get("_template", "OJR")

        if template == "ZHV":
            self.add_zhv_header(document, meta)
        else:
            self.add_ojr_header(document)

        if not columns:
            document.save(filename)
            return

        row_count = max(len(column) for column in columns)
        table = document.add_table(rows=row_count, cols=len(headers))
        table.style = "Table Grid"

        for row_index in range(row_count):
            for col_index, column in enumerate(columns):
                value = column[row_index] if row_index < len(column) else ""
                self.format_data_cell(table.cell(row_index, col_index), value)

        document.save(filename)


if __name__ == "__main__":
    builder = DOCX_table_builder()
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    with open(input_file, 'r', encoding='utf-8-sig') as file:
        data = json.load(file)
    builder.build(data, output_file)
