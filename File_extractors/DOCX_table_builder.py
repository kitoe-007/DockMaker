import sys
import json
import io

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class DOCX_table_builder:
    def __init__(self):
        pass

    def set_cell_border(self, cell, **kwargs):
        tc = cell._tc
        tc_pr = tc.get_or_add_tcPr()
        tc_borders = OxmlElement("w:tcBorders")
        for edge, value in kwargs.items():
            element = OxmlElement(f"w:{edge}")
            element.set(qn("w:val"), value.get("val", "single"))
            element.set(qn("w:sz"), str(value.get("sz", 4)))
            element.set(qn("w:space"), "0")
            element.set(qn("w:color"), value.get("color", "000000"))
            tc_borders.append(element)
        tc_pr.append(tc_borders)

    def format_header_cell(self, cell, text):
        cell.text = str(text)
        for paragraph in cell.paragraphs:
            paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for run in paragraph.runs:
                run.bold = True
        self.set_cell_border(
            cell,
            top={"val": "single"},
            bottom={"val": "single"},
            left={"val": "single"},
            right={"val": "single"},
        )

    def format_data_cell(self, cell, text):
        cell.text = "" if text is None else str(text)
        self.set_cell_border(
            cell,
            top={"val": "single"},
            bottom={"val": "single"},
            left={"val": "single"},
            right={"val": "single"},
        )

    def build(self, data, filename):
        headers = list(data.keys())
        columns = []
        for header in headers:
            columns.append(data[header])

        row_count = max(len(column) for column in columns)
        document = Document()
        table = document.add_table(rows=row_count + 1, cols=len(headers))
        table.style = "Table Grid"

        for col_index, header in enumerate(headers):
            self.format_header_cell(table.cell(0, col_index), header)

        for row_index in range(row_count):
            for col_index, column in enumerate(columns):
                value = column[row_index] if row_index < len(column) else ""
                self.format_data_cell(table.cell(row_index + 1, col_index), value)

        document.save(filename)


if __name__ == "__main__":
    builder = DOCX_table_builder()
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    with open(input_file, 'r', encoding='utf-8') as file:
        data = json.load(file)
    builder.build(data, output_file)
