import sys
import pandas as pd
import io
import openpyxl
import json

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

if __name__ == "__main__":
    extractor = XLSX_extractor()
    input_file = sys.argv[1]
    result = extractor.extract(input_file)
    if len(sys.argv) > 2 and sys.argv[2] == "cut":
        result = extractor.cut(result, beg=int(sys.argv[3]), end=int(sys.argv[4]))
    #print(json.dumps(result, ensure_ascii=False))
    print(result)
