import sys
import pandas as pd
import io
import openpyxl
import json
import datetime

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

#df = pd.read_excel("files/Excel.xlsx")
#print(df.columns)
#df.columns = pd.Index(['1', '2', '3', '4', '5', '6', '7', '8', '9'], dtype='str')
#print(df.columns)
#print(df["1"].to_string())
#print(df.to_numpy()[2:])


class XLSX_extractor:
    def __init__(self):
        pass

    def extract(self, filename):
        return pd.read_excel(filename).to_numpy()

    def cut(self, df, beg=0, end=0):
        return df[beg:len(df)-end]

    def convert_to_dict(self, df): # формат json {количество_столбцов:число, столбец0:список_всех_элементов_столбца0, столбец1:список_всех_элементов_столбца1, ... столбецN:список_всех_элементов_столбцаN}
        table_length = len(df[0])
        result = {"number_of_columns": table_length}

        for ind in range(len(df[0])):
            result[f"column_{ind}"] = []
            for row in df:
                elem = row[ind]
                # System.Text.Json не умеет читать NaN/Infinity как валидный JSON.
                # Конвертируем отсутствующие значения (NaN/NaT) в None -> null.
                if pd.isna(elem):
                    result[f"column_{ind}"].append(None)
                    continue
                if isinstance(elem, (pd.Timestamp, datetime.datetime)):
                    result[f"column_{ind}"].append(str(elem)[:-9])
                    continue
                result[f"column_{ind}"].append(elem)
        return result


if __name__ == "__main__":
    extractor = XLSX_extractor()
    input_file = sys.argv[1]
    result = extractor.extract(input_file)
    if len(sys.argv) > 2 and sys.argv[2] == "cut":
        result = extractor.cut(result, beg=int(sys.argv[3]), end=int(sys.argv[4]))
    result = extractor.convert_to_dict(result)
    print(json.dumps(result, ensure_ascii=False))

