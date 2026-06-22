import sys
import json
import io

from docx import Document

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


class DOCX_table_builder:
    def __init__(self):
        pass

    def format_data_cell(self, cell, text):
        cell.text = "" if text is None else str(text)

    def build(self, data, filename):
        headers = list(data.keys())
        columns = [data[header] for header in headers]

        if not columns:
            document = Document()
            document.save(filename)
            return

        row_count = max(len(column) for column in columns)
        document = Document()
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
