import json
import sys
import math
from docx import Document

DATE_KEY = "дата"
SECTION_KEY = "Раздел"
WORK_KEY = "наименование материала"


def normalize_section(name):
    if not name:
        return ""
    return str(name).strip().upper().replace(" ", "").replace("-", "")


def section_matches(current_section, allowed_sections):
    current = normalize_section(current_section)
    if not current:
        return False
    return any(normalize_section(section) == current for section in allowed_sections)


def get_value(data, key, index):
    val = data[key][index]
    if isinstance(val, float) and math.isnan(val):
        return ""
    return str(val).strip()


def create_table_from_json(data, sections, output_path="demo.docx"):
    document = Document()
    table = document.add_table(rows=1, cols=4)
    table.style = "Table Grid"

    hdr_cells = table.rows[0].cells
    hdr_cells[0].text = "№/пп"
    hdr_cells[1].text = "Дата выполнения работ"
    hdr_cells[2].text = (
        "Наименование работ, выполняемых в процессе строительства, "
        "реконструкции, капитального ремонта объекта капитального строительства"
    )
    hdr_cells[3].text = (
        "Должность, фамилия, инициалы, подпись уполномоченного "
        "представителя лица, осуществляющего строительство"
    )

    row_count = len(data[DATE_KEY])
    current_date = ""
    current_section = ""
    works_by_date = {}

    for i in range(row_count):
        date = get_value(data, DATE_KEY, i)
        section = get_value(data, SECTION_KEY, i)
        work = get_value(data, WORK_KEY, i)

        if date:
            current_date = date
            current_section = section
        elif section:
            current_section = section

        if work and section_matches(current_section, sections) and current_date:
            works_by_date.setdefault(current_date, []).append(work)

    row_number = 0
    for date in sorted(works_by_date):
        works = works_by_date[date]
        row_number += 1
        row_cells = table.add_row().cells
        row_cells[0].text = str(row_number)
        row_cells[1].text = date
        row_cells[2].text = ", ".join(works)
        row_cells[3].text = ""

    document.save(output_path)


if __name__ == "__main__":
    input_json = "output.json"
    sections = ("АК-1",)
    output_path = "demo.docx"

    if len(sys.argv) > 1:
        input_json = sys.argv[1]
    if len(sys.argv) > 2:
        sections = tuple(part.strip() for part in sys.argv[2].split(",") if part.strip())
    if len(sys.argv) > 3:
        output_path = sys.argv[3]

    with open(input_json, "r", encoding="utf-8") as file:
        result = json.load(file)

    create_table_from_json(result, sections, output_path)
    print(json.dumps({"Status" : "Success"}))
